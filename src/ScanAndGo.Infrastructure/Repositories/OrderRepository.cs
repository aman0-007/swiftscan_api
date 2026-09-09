using System.Data;
using Dapper;
using ScanAndGo.Application.Interfaces;
using ScanAndGo.Domain.Entities;
using ScanAndGo.Infrastructure.Data;

namespace ScanAndGo.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public OrderRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> CreateOrderAsync(Order order, IEnumerable<OrderItem> orderItems)
        {
            using IDbConnection connection = _connectionFactory.CreateConnection();
            connection.Open(); // Must explicitly open connection for transactions
            using IDbTransaction transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert the Order and get the generated UUID back using 'RETURNING Id'
                string orderSql = @"
                    INSERT INTO Orders (UserId, TotalAmount, Status) 
                    VALUES (@UserId, @TotalAmount, @Status) 
                    RETURNING Id;";
                
                var newOrderId = await connection.ExecuteScalarAsync<Guid>(orderSql, order, transaction);

                // 2. Assign the newly generated OrderId to all OrderItems
                foreach (var item in orderItems)
                {
                    item.OrderId = newOrderId;
                }

                // 3. Insert all OrderItems
                string itemsSql = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) 
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice);";

                var insertedItemsCount = await connection.ExecuteAsync(itemsSql, orderItems, transaction);

                // 4. If everything worked, commit it to the database
                transaction.Commit();
                
                return 1 + insertedItemsCount; // Return total rows affected
            }
            catch
            {
                // If anything fails (like a foreign key error), cancel everything
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            return await db.QuerySingleOrDefaultAsync<Order>(
                "SELECT * FROM Orders WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            
            // Join Orders, OrderItems, and Products
            string sql = @"
                SELECT 
                    o.Id, o.UserId, o.TotalAmount, o.Status, o.CreatedAt,
                    oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice,
                    p.Name as ProductName
                FROM Orders o
                LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
                LEFT JOIN Products p ON oi.ProductId = p.Id
                WHERE o.UserId = @UserId
                ORDER BY o.CreatedAt DESC;";

            var orderDictionary = new Dictionary<Guid, Order>();

            // Dapper multi-mapping: Maps the flat SQL rows into nested Order -> OrderItems objects
            await db.QueryAsync<Order, OrderItem, Order>(
                sql,
                (order, orderItem) =>
                {
                    if (!orderDictionary.TryGetValue(order.Id, out var currentOrder))
                    {
                        currentOrder = order;
                        orderDictionary.Add(currentOrder.Id, currentOrder);
                    }

                    if (orderItem != null)
                    {
                        currentOrder.Items.Add(orderItem);
                    }
                    return currentOrder;
                },
                new { UserId = userId },
                splitOn: "Id" // Tells Dapper to split the row when it hits the OrderItem.Id column
            );

            return orderDictionary.Values;
        }
    }
}