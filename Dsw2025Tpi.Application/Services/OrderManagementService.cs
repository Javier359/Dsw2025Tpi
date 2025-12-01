using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrderManagementService
    {
        private readonly IRepository _repository;
        public OrderManagementService(IRepository repository)
        {
            _repository = repository; 
        }
         
        public async Task<OrderModel.OrderResponse> CreateOrderAsync(OrderModel.OrderRequest dtoRequest)
        {
            if (dtoRequest == null)
                throw new ArgumentException("El cuerpo de la petición no puede estar vacio.");

            if (dtoRequest.OrderItems == null || !dtoRequest.OrderItems.Any())
                throw new ArgumentException("La orden debe contener al menos un item.");

            var items = new List<OrderItem>();
            decimal total = 0m;

            foreach (var item in dtoRequest.OrderItems)
            {
                var product = await _repository.GetById<Product>(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Producto '{item.ProductId}' no existe.");

                if (item.Quantity > product.StockQuantity)
                    throw new ArgumentException($"Stock insuficiente -> '{product.Name}'");

                if (!product.IsActive)
                    throw new ArgumentException($"El producto esta inactico.");

                var subTotal = item.Quantity * product.CurrentUnitPrice;
                total += subTotal;

                product.StockQuantity -= item.Quantity;
                await _repository.Update<Product>(product);

                items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.CurrentUnitPrice,
                    SubTotal = subTotal
                });
            }

            var orderEntity = new Order
            {
                CustomerId = dtoRequest.CustomerId,
                Date = DateTime.UtcNow,
                ShippingAddress = dtoRequest.ShippingAddress, 
                BillingAddress = dtoRequest.BillingAddress,
                TotalAmount = total,
            };

            foreach(var oi in items)
                orderEntity.Items.Add(oi);

            var createOrder = await _repository.Add<Order>(orderEntity);

            var responseItems = items
                .Select(item => new OrderModel.OrderItemResponse(
                    item.ProductId,
                    item.Product?.Name ?? "<sin nombre>",
                    item.Quantity,
                    item.UnitPrice,
                    item.SubTotal
                    ))
                .ToList();

            return new OrderModel.OrderResponse(
                createOrder.Id,
                createOrder.Status,
                createOrder.CustomerId,
                createOrder.Date,
                createOrder.ShippingAddress,
                createOrder.BillingAddress,
                createOrder.TotalAmount,
                responseItems
            ); 
        }

        public async Task<List<OrderModel.OrderResponse>> GetOrdersAsync(
            OrderStatus? status = null,
            Guid? customerId = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var orders = await _repository.GetAll<Order>("Items", "Items.Product") ?? new List<Order>();

            if (status.HasValue)
            {
                orders = orders
                    .Where(o => o.Status == status.Value)
                    .ToList();
            }

            if (customerId.HasValue)
            {
                orders = orders
                    .Where(o => o.CustomerId == customerId.Value)
                    .ToList();
            }

            orders = orders
                .OrderByDescending(o => o.Date)
                .ToList();

            var skip = (pageNumber - 1) * pageSize;
            var page = orders
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            if (!page.Any())
                return new List<OrderModel.OrderResponse>();

            var orderList = page.Select(order => new OrderModel.OrderResponse(
                order.Id,
                order.Status,
                order.CustomerId,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.TotalAmount,
                order.Items.Select(i => new OrderModel.OrderItemResponse(
                    i.ProductId,
                    i.Product?.Name ?? "<sin nombre>",
                    i.Quantity,
                    i.UnitPrice,
                    i.SubTotal
                )).ToList()
            )).ToList();

            return orderList;
        }

        public async Task<OrderModel.OrderResponse> GetOrderByIdAsync(Guid id)
        {
            var order = await _repository.GetById<Order>(id, "Items", "Items.Product");

            if (order == null)
                throw new KeyNotFoundException($"La Orden con id:'{id}' no existe");

            return new OrderModel.OrderResponse(
                order.Id,
                order.Status,
                order.CustomerId,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.TotalAmount,
                order.Items.Select(i => new OrderModel.OrderItemResponse(
                    i.ProductId,
                    i.Product?.Name ?? "<sin nombre>",
                    i.Quantity,
                    i.UnitPrice,
                    i.SubTotal
                )).ToList()
             );     
        }

        public async Task<OrderModel.OrderResponse> UpdateStatusAsync(Guid id, OrderStatus newStatus)
        {
            var order = await _repository.GetById<Order>(id);

            if (order == null)
                throw new KeyNotFoundException($"La orden con id: '{id}' no existe.");

            if(order.Status == newStatus)
                return await GetOrderByIdAsync(id);

            order.Status = newStatus;

            await _repository.Update<Order>(order);

            return await GetOrderByIdAsync(id);
        }

    }
}
