using System.Data;
using Dapper;
using ScanAndGo.Application.Interfaces;
using ScanAndGo.Domain.Entities;
using ScanAndGo.Infrastructure.Data;

namespace ScanAndGo.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public ProductRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            return await db.QuerySingleOrDefaultAsync<Product>(
                "SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        }

        public async Task<Product?> GetByBarcodeAsync(string barcode)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            return await db.QuerySingleOrDefaultAsync<Product>(
                "SELECT * FROM Products WHERE Barcode = @Barcode", new { Barcode = barcode });
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            return await db.QueryAsync<Product>("SELECT * FROM Products");
        }

        public async Task<int> CreateAsync(Product product)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            string sql = @"
                INSERT INTO Products (Barcode, Name, Price, StockQuantity) 
                VALUES (@Barcode, @Name, @Price, @StockQuantity)";
            return await db.ExecuteAsync(sql, product);
        }

        public async Task UpdateStockAsync(Guid productId, int newStockQuantity)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            string sql = "UPDATE Products SET StockQuantity = @StockQuantity WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { StockQuantity = newStockQuantity, Id = productId });
        }
    }
}