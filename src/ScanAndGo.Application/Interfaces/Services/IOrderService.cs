using ScanAndGo.Application.DTOs;

namespace ScanAndGo.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CheckoutAsync(Guid userId, CreateOrderRequest request);
        Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(Guid userId);
    }
}