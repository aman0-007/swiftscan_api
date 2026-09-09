using System.Data;
using Dapper;

namespace ScanAndGo.Infrastructure.Data
{
    public class DatabaseMigration
    {
        private readonly DbConnectionFactory _connectionFactory;

        public DatabaseMigration(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void RunLatest(string direction)
        {
            direction = direction.ToLower();
            if (direction != "up" && direction != "down")
            {
                throw new ArgumentException("Direction must be 'up' or 'down'");
            }

            string scriptsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Scripts");
            
            if (!Directory.Exists(scriptsPath))
            {
                scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "ScanAndGo.Infrastructure", "Data", "Scripts");
            }

            if (!Directory.Exists(scriptsPath))
            {
                throw new DirectoryNotFoundException($"Scripts directory not found at: {scriptsPath}");
            }

            string searchPattern = $"*.{direction}.sql";
            var files = Directory.GetFiles(scriptsPath, searchPattern);

            if (files.Length == 0)
            {
                Console.WriteLine($"No {direction} scripts found.");
                return;
            }

            string latestFile = files.OrderBy(f => f).Last();
            string fileName = Path.GetFileName(latestFile);
            
            Console.WriteLine($"Executing migration: {fileName}");
            string sql = File.ReadAllText(latestFile);

            // Utilizing the central connection factory
            using IDbConnection connection = _connectionFactory.CreateConnection();
            connection.Execute(sql);
            
            Console.WriteLine($"Migration {fileName} executed successfully.");
        }
    }
}

