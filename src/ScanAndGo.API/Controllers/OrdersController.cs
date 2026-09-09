using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScanAndGo.Application.DTOs;
using ScanAndGo.Application.Interfaces.Services;

namespace ScanAndGo.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 This locks down the ENTIRE controller. You MUST have a token.
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Helper to extract the secure User ID from the JWT Token
        private Guid GetUserIdFromToken()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdString!);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CreateOrderRequest request)
        {
            try
            {
                Guid secureUserId = GetUserIdFromToken();
                var response = await _orderService.CheckoutAsync(secureUserId, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            Guid secureUserId = GetUserIdFromToken();
            var orders = await _orderService.GetUserOrdersAsync(secureUserId);
            return Ok(orders);
        }
    }
}