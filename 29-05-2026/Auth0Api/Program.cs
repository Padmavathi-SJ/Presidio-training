using Auth0.AspNetCore.Authentication.Api;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddAuth0ApiAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"] 
        ?? throw new InvalidOperationException("Auth0:Domain is not configured");
    options.Audience = builder.Configuration["Auth0:Audience"] 
        ?? throw new InvalidOperationException("Auth0:Audience is not configured");
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth0 API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/public", () => Results.Ok(new { Message = "This endpoint is public" }));
app.MapGet("/api/private", () => Results.Ok(new { Message = "This endpoint requires authentication" })).RequireAuthorization();
app.MapGet("/api/claims", (HttpContext context) =>
{
    return Results.Ok(new
    {
        IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false,
        Claims = context.User.Claims.Select(c => new { c.Type, c.Value })
    });
}).RequireAuthorization();

app.Run();