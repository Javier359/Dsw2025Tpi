using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagmentService
    {
        private readonly IRepository _repository;

        public ProductsManagmentService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductModel.ProductResponse> CreateProductAsync (ProductModel.ProductRequest dto)
        {
            if(string.IsNullOrWhiteSpace(dto.Sku) ||
                string.IsNullOrEmpty(dto.Sku) ||
               string.IsNullOrWhiteSpace(dto.Name) ||
               dto.CurrentUnitPrice <= 0 ||
               dto.StockQuantity < 0 )
            {
                throw new ArgumentException("Datos inválidos para crear el producto.");
            }

            var existe = await _repository.First<Product>(p => p.Sku == dto.Sku);
            if (existe != null)
                throw new DuplicatedEntityException($"SKU '{dto.Sku}' ya existe.");

            var product = new Product (
                dto.Sku,
                dto.InternalCode,
                dto.Name,
                dto.Description,
                dto.CurrentUnitPrice,
                dto.StockQuantity
            );

            var created = await _repository.Add<Product>(product);

            return new ProductModel.ProductResponse(
                created.Id,
                created.Sku,
                created.InternalCode,
                created.Name,
                created.Description,
                created.CurrentUnitPrice,
                created.StockQuantity
            );
           
        }

        public async Task<List<ProductModel.ProductResponse>> GetProductsAsync()
        {
            var products = await _repository.GetAll<Product>();

            var activeProducts = products
                .Where(p => p.IsActive)
                .ToList();

            if (!activeProducts.Any())
                return new List<ProductModel.ProductResponse>();

            var productsList = activeProducts
                .Select(p => new ProductModel.ProductResponse(
                    p.Id,
                    p.Sku,
                    p.InternalCode,
                    p.Name,
                    p.Description,
                    p.CurrentUnitPrice,
                    p.StockQuantity))
                .ToList();

            return productsList;
        }

        public async Task<ProductModel.ProductResponse> GetProductByIdAsync(Guid id)
        {
            var product = await _repository.GetById<Product>(id);

            if (product == null)
                throw new KeyNotFoundException($"El Producto con id: '{id}' no existe.");

            return new ProductModel.ProductResponse(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity
                );
        }

        public async Task<ProductModel.ProductResponse> UpdateProductAsync(Guid id, ProductModel.UpdateProductRequest dto)
        {
            var product = await _repository.GetById<Product>(id);

            if (product == null)
                return null;

            if(string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Description) ||
               dto.CurrentUnitPrice <= 0 ||
               dto.StockQuantity < 0)
            {
                throw new ArgumentException("Datos inválidos para actulizar el producto.");
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CurrentUnitPrice = dto.CurrentUnitPrice;
            product.StockQuantity = dto.StockQuantity;

            var update = await _repository.Update<Product>(product);

            return new ProductModel.ProductResponse(
                update.Id,
                update.Sku,
                update.InternalCode,
                update.Name,
                update.Description,
                update.CurrentUnitPrice,
                update.StockQuantity
            );  
        }

        public async Task<bool> DisableProductAsync(Guid id)
        {
            var product = await _repository.GetById<Product>(id);

            if (product == null)
                return false;

            product.IsActive = false;
            await _repository.Update<Product>(product);

            return true;
        }


    
    }
}
