using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanAndGo.Infrastructure.Data;

// Correct path: step up one level from ScanAndGo.Migrator into src, then into ScanAndGo.API
string apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "ScanAndGo.API"));

var configuration = new ConfigurationBuilder()
    .SetBasePath(apiProjectPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Setup Dependency Injection container
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<DbConnectionFactory>();
services.AddTransient<DatabaseMigration>();
services.AddTransient<DatabaseSeeder>();

services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Parse CLI arguments
if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run -- <command>");
    Console.WriteLine("Commands available: 'up', 'down', 'seed'");
    return;
}

string command = args[0].ToLower();

try
{
    if (command == "up" || command == "down")
    {
        var migrator = serviceProvider.GetRequiredService<DatabaseMigration>();
        migrator.RunLatest(command);
    }
    else if (command == "seed")
    {
        var seeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
        seeder.Run();
    }
    else
    {
        Console.WriteLine($"Unknown command: {command}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Execution failed: {ex.Message}");
}

