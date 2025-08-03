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

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel.ProductRequest dto)
        {
            try
            {
                var pruduct = await _service.CreateProductAsync(dto);
                return StatusCode(StatusCodes.Status201Created, pruduct);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message); //400
            }
            catch (DuplicatedEntityException de)
            {
                return Conflict(de.Message); //409
            }
            catch (Exception)
            {
                return Problem("Se produjo un error al guardar el producto.");
            }
        }

        [HttpGet]
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _service.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound(); //404
                return Ok(product);
            }
            catch(KeyNotFoundException knf)
            {
                return NotFound(new { error = knf.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct(Guid id, ProductModel.UpdateProductRequest model)
        {
            try
            {
                var product = await _service.UpdateProductAsync(id, model);

                if(product == null)
                    return NotFound();
                return StatusCode(StatusCodes.Status200OK, model);

            }
            catch(ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/isActive")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                var status = await _service.DisableProductAsync(id);
                if (!status)
                    return NotFound();
                return NoContent(); //204
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

        }
    }
}
