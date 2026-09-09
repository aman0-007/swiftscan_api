using System.Data;
using Dapper;

namespace ScanAndGo.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly DbConnectionFactory _connectionFactory;

        public DatabaseSeeder(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Run()
        {
            Console.WriteLine("Seeding database with realistic dummy data...");
            using IDbConnection connection = _connectionFactory.CreateConnection();

            // 1. Seed Users
            string insertUserSql = @"
                INSERT INTO Users (Email, PasswordHash, Role) 
                VALUES (@Email, @PasswordHash, @Role) 
                ON CONFLICT (Email) DO NOTHING;";

            var users = new[]
            {
                new { Email = "admin@scanandgo.com", PasswordHash = "hashed_pw_admin", Role = "Admin" },
                new { Email = "shopper@scanandgo.com", PasswordHash = "hashed_pw_shopper", Role = "Shopper" }
            };
            connection.Execute(insertUserSql, users);

            // 2. Seed Realistic Products
            string insertProductSql = @"
                INSERT INTO Products (Barcode, Name, Price, StockQuantity) 
                VALUES (@Barcode, @Name, @Price, @StockQuantity) 
                ON CONFLICT (Barcode) DO NOTHING;";

            var products = new[]
            {
                new { Barcode = "123456789012", Name = "Organic Fuji Apples (1kg)", Price = 4.99m, StockQuantity = 150 },
                new { Barcode = "987654321098", Name = "Whole Milk 1 Gallon", Price = 3.49m, StockQuantity = 85 },
                new { Barcode = "456789123456", Name = "Artisan Whole Wheat Bread", Price = 5.99m, StockQuantity = 40 },
                new { Barcode = "112233445566", Name = "Free-Range Large Eggs (12pk)", Price = 6.49m, StockQuantity = 120 },
                new { Barcode = "665544332211", Name = "Columbian Roast Coffee Beans", Price = 14.99m, StockQuantity = 60 },
                new { Barcode = "998877665544", Name = "Sharp Cheddar Cheese Block", Price = 7.25m, StockQuantity = 90 },
                new { Barcode = "445566778899", Name = "Sparkling Mineral Water (6pk)", Price = 4.50m, StockQuantity = 200 },
                new { Barcode = "334455667788", Name = "Himalayan Pink Salt", Price = 3.99m, StockQuantity = 45 }
            };
            
            // Dapper automatically loops through the array and inserts each one safely
            connection.Execute(insertProductSql, products);
            
            Console.WriteLine("Database seeding completed successfully!");
        }
    }
}