using Microsoft.AspNetCore.Mvc;
using ScanAndGo.Application.DTOs;
using ScanAndGo.Application.Interfaces;
using System.Linq; // Required for mapping the list

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

        // GET /api/Products
        // Lists all products available in the database
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepository.GetAllAsync();

            // Map Domain Entities to DTOs to protect internal database fields
            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            });

            return Ok(productDtos);
        }

        // GET /api/Products/{barcode}
        // Looks up a single product by its barcode
        [HttpGet("{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            var product = await _productRepository.GetByBarcodeAsync(barcode);
            
            if (product == null)
                return NotFound(new { message = $"Product with barcode '{barcode}' not found." });

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