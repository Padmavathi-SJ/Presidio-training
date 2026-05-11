namespace NotificationSystem.DataAccess.Config
{
    public class DatabaseConfig
    {
        public string Host {get; set;} = string.Empty;
        public int Port {get; set;} = 5432;
        public string DatabaseName {get; set;} = string.Empty;
        public string UserName { get; set;} = string.Empty;
        public string Password {get; set;} = string.Empty;
    
    public string GetConnectionString()
        {
            return $"Host={Host}; Port={Port}; Database={DatabaseName}; UserName={UserName}; Password={Password}";
        }
}
}