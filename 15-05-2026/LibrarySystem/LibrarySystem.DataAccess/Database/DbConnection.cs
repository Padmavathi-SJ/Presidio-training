using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using LibrarySystem.DataAccess.Config;

namespace LibrarySystem.DataAccess.Database
{
    public class DatabaseConnection
    {
        private readonly DatabaseConfig _config;

        public DatabaseConnection(DatabaseConfig config)
        {
            _config = config;
        }

        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(_config.GetConnectionString());
            
            connection.Open();
            
            return connection;
        }

        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var connection = new NpgsqlConnection(_config.GetConnectionString());
            
            await connection.OpenAsync();
            
            return connection;
        }

    }
}