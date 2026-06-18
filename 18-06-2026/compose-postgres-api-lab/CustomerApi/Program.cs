using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks();

//  ADD CORS POLICY
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",      // Local Angular dev
                "http://localhost:4200",      // Docker Angular
                "http://customer-ui:80"       // Container name (if needed)
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowAngularApp");

app.MapHealthChecks("/health");

app.MapGet("/", () => "Customer API is running");

app.MapGet("/customers", async (CustomerDbContext db) =>
    await db.Customers.ToListAsync());

app.MapPost("/customers", async (Customer customer, CustomerDbContext db) =>
{
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{customer.Id}", customer);
});

// For lab simplicity only. In real enterprise applications, use migrations in CI/CD.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    db.Database.Migrate();
}

app.Run();

