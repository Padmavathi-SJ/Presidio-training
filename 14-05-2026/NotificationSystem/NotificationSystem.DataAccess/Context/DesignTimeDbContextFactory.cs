using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NotificationSystem.DataAccess.Config;
using DotNetEnv;

namespace NotificationSystem.DataAccess.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Load .env from solution root (same as Program.cs)
            Env.Load();

            var databaseConfig = new DatabaseConfig
            {
                Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"),
                DatabaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "notification_system_ef",
                UserName = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""
            };

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(databaseConfig.GetConnectionString());

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}