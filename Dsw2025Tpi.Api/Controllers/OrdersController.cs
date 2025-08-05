using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Authorize] /*se agrega la autorizacion de los endpoint*/
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderManagementService _orderManagementService;
        public OrdersController(OrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest orderDto)
        {
            try
            {
                var order = await _orderManagementService.CreateOrderAsync(orderDto);

                return StatusCode(StatusCodes.Status201Created, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetOrders (
            [FromQuery] OrderStatus? status,
            [FromQuery] Guid? customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var list = await _orderManagementService.GetOrdersAsync(
                    status, 
                    customerId,
                    pageNumber,
                    pageSize
                );

                if (!list.Any())
                    return Ok(new
                    {
                        message = "No se encontró ninguna Order.",
                        data = list
                    });

                return StatusCode(StatusCodes.Status200OK, list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("get/{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var order = await _orderManagementService.GetOrderByIdAsync(id);
                return StatusCode(StatusCodes.Status202Accepted, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("get/{id:guid}/status")]
        public async Task<IActionResult> UpdateOrder (Guid id, [FromBody] OrderModel.UpdateOrderStatusRequest orderStatusDto)
        {
            try
            {
                var orderUpdate = await _orderManagementService.UpdateStatusAsync(id, orderStatusDto.NewStatus);
                return StatusCode(StatusCodes.Status200OK, orderUpdate);

            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
