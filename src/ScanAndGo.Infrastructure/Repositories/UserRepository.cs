using System.Data;
using Dapper;
using ScanAndGo.Application.Interfaces;
using ScanAndGo.Domain.Entities;
using ScanAndGo.Infrastructure.Data;

namespace ScanAndGo.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public UserRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            return await db.QuerySingleOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using IDbConnection db = _connectionFactory.CreateConnection();
            return await db.QuerySingleOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
        }

        public async Task<int> CreateAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Role))
            {
                user.Role = "Shopper";
            }
            using IDbConnection db = _connectionFactory.CreateConnection();
            string sql = @"
                INSERT INTO Users (Email, PasswordHash, Role) 
                VALUES (@Email, @PasswordHash, @Role)";
            return await db.ExecuteAsync(sql, user);
        }
    }
}