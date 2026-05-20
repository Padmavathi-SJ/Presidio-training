using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using LibrarySystem.DataAccess.Config;
using LibrarySystem.DataAccess.Database;
using Microsoft.Extensions.Configuration; 
using Npgsql;
using DotNetEnv;

namespace LibrarySystem.DataAccess.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "LibrarySystem.Presentation", ".env");
            
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                Console.WriteLine($"loaded .env from : {envPath}");
            }
            else
            {
                var solutionPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "env");

                if (File.Exists(solutionPath))
                {
                   // Try current directory
                Env.Load(solutionPath); 
                }
                 else
                {
                    Console.WriteLine("⚠️ No .env file found, using environment variables");
                    Env.Load();
                }

            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Load connection string from .env or environment
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "library_system";
            var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
            
            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            
            optionsBuilder.UseNpgsql(connectionString);
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}