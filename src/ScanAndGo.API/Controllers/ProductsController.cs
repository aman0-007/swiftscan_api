using Microsoft.AspNetCore.Mvc;
using ScanAndGo.Application.DTOs;
using ScanAndGo.Application.Interfaces;

namespace ScanAndGo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet("{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            var product = await _productRepository.GetByBarcodeAsync(barcode);
            
            if (product == null)
                return NotFound(new { message = $"Product with barcode '{barcode}' not found." });

            // Map Domain Entity to DTO to hide internal database fields
            var productDto = new ProductDto
            {
                Id = product.Id,
                Barcode = product.Barcode,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };

            return Ok(productDto);
        }
    }
}