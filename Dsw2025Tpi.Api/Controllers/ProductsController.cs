using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Authorize] /*se agrega la autorizacion de los endpoint*/
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsManagmentService _service;

        public ProductsController(ProductsManagmentService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel.ProductRequest dto)
        {
            try
            {
                var pruduct = await _service.CreateProductAsync(dto);
                return StatusCode(StatusCodes.Status201Created, pruduct);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _service.GetProductsAsync();

                if (products.Count == 0)
                    return Ok(new
                    {
                        message = "No hay productos cargados.",
                        data = products
                    });

                return StatusCode(StatusCodes.Status200OK, products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

        }

        [HttpGet("get/{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _service.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound(); //404
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }

        }

        [HttpPut("{id:guid}/update")]
        public async Task<IActionResult> UpdateProduct(Guid id, ProductModel.UpdateProductRequest model)
        {
            try
            {
                var product = await _service.UpdateProductAsync(id, model);

                if(product == null)
                    return NotFound(product);
                return StatusCode(StatusCodes.Status200OK, model);

            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/isActive")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                var status = await _service.DisableProductAsync(id);
                if (!status)
                    return StatusCode(StatusCodes.Status404NotFound, $"El producto con id: '{id}' no existe.");

                return StatusCode(StatusCodes.Status202Accepted, $"El producto con id: '{id}' fue desactivado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

        }
    }
}
