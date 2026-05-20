var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add Swagger services instead of OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Comment this if no HTTPS certificate

app.UseAuthorization();
app.MapControllers();
app.Run();