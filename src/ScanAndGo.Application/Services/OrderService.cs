using ScanAndGo.Application.DTOs;
using ScanAndGo.Application.Interfaces;
using ScanAndGo.Application.Interfaces.Services;
using ScanAndGo.Domain.Entities;

namespace ScanAndGo.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<OrderResponseDto> CheckoutAsync(Guid userId, CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new ArgumentException("Order must contain at least one item.");

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            // 1. Validate products, check stock, and calculate total
            foreach (var itemRequest in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
                
                if (product == null)
                    throw new Exception($"Product with ID {itemRequest.ProductId} not found.");

                if (product.StockQuantity < itemRequest.Quantity)
                    throw new Exception($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");

                totalAmount += product.Price * itemRequest.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price
                });
            }

            // 2. Create the Order entity
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = "Completed", // For Scan & Go, payment is usually assumed successful at this stage
                CreatedAt = DateTime.UtcNow
            };

            // 3. Save to database using the transaction in our repository
            await _orderRepository.CreateOrderAsync(order, orderItems);

            // 4. Update product stock quantities
            foreach (var item in orderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    int newStock = product.StockQuantity - item.Quantity;
                    await _productRepository.UpdateStockAsync(product.Id, newStock);
                }
            }

            // 5. Return the safe DTO to the API
            return new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt
            };
        }

        public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            
            return orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                // Map the nested items
                Items = o.Items.Select(i => new OrderItemResponseDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }
    }
}