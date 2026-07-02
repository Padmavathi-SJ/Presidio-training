using Azure.Identity;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Get Key Vault name from configuration
var keyVaultUri = new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/");

// Add Key Vault configuration
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Blob Service Client
builder.Services.AddSingleton(x =>
{
    var connStr = builder.Configuration["BlobStorageConnectionString"];
    if (string.IsNullOrEmpty(connStr))
    {
        throw new InvalidOperationException("BlobStorageConnectionString is not configured in Key Vault");
    }
    return new BlobServiceClient(connStr);
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();