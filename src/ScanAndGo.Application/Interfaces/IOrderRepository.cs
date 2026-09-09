using ScanAndGo.Domain.Entities;

namespace ScanAndGo.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> orderItems);
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    }
}