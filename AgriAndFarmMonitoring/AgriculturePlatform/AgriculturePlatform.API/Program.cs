using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using AutoMapper;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Infrastructure.Repositories;
using AgriculturePlatform.Application.Validators;
using Microsoft.OpenApi.Models;
using AgriculturePlatform.API.BackgroundServices;
using AgriculturePlatform.Application.Mappings;
using AgriculturePlatform.API.Hubs;
using AgriculturePlatform.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Farm Management Platform API", 
        Version = "v1",
        Description = "API for managing farms, crops, workers, and yields"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Configuration
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-minimum-32-characters-long";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "AgriculturePlatform";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "AgriculturePlatformClients";
var expiryDays = int.Parse(builder.Configuration["JwtSettings:ExpiryDays"] ?? "7");

// Register JWT Service
builder.Services.AddSingleton<IJwtService>(provider =>
    new JwtService(jwtSecretKey, jwtIssuer, jwtAudience, expiryDays));

// Add AuditLog Service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// ✅ SignalR - ONLY ONCE and BEFORE building the app
builder.Services.AddSignalR();

// Background IoT Simulator
builder.Services.AddHostedService<IoTSimulatorBackgroundService>();

// Add AutoMapper - Manual configuration
var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<FieldMappingProfile>();
    cfg.AddProfile<CropCycleMappingProfile>();
    cfg.AddProfile<WorkerMappingProfile>();
    cfg.AddProfile<WorkerProfileMappingProfile>();
    cfg.AddProfile<WorkerFieldAssignmentMappingProfile>();
    cfg.AddProfile<WorkerFieldMappingProfile>();
    cfg.AddProfile<WeatherMappingProfile>();
    cfg.AddProfile<TaskMappingProfile>();
    cfg.AddProfile<WorkerTaskMappingProfile>();
    cfg.AddProfile<SensorMappingProfile>();
    cfg.AddProfile<AlertMappingProfile>();
    cfg.AddProfile<ObservationMappingProfile>();

});

var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton<IMapper>(mapper);

// Register all validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<CreateFieldValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateWorkerTaskStatusValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SensorReadingFilterValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AlertFilterValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateObservationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateObservationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ObservationFilterValidator>();

// Add Excel service
builder.Services.AddScoped<IExcelService, ExcelService>();

// Register Repositories
builder.Services.AddScoped<IFarmRepository, FarmRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IFieldRepository, FieldRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ICropCycleRepository, CropCycleRepository>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<IWorkerFieldAssignmentRepository, WorkerFieldAssignmentRepository>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertThresholdRepository, AlertThresholdRepository>();
builder.Services.AddScoped<IObservationRepository, ObservationRepository>();

// Register Services
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddScoped<ICropCycleService, CropCycleService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IWorkerAuthService, WorkerAuthService>();
builder.Services.AddScoped<IWorkerProfileService, WorkerProfileService>();
builder.Services.AddScoped<IWorkerFieldAssignmentService, WorkerFieldAssignmentService>();
builder.Services.AddScoped<IWorkerFieldService, WorkerFieldService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddSingleton<IWeatherApiService, WeatherApiService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IExcelTaskService, ExcelTaskService>();
builder.Services.AddScoped<IWorkerTaskService, WorkerTaskService>();
builder.Services.AddScoped<ISensorReadingService, SensorReadingService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IIoTSimulatorService, IoTSimulatorService>();
builder.Services.AddScoped<IAlertNotificationService, AlertNotificationService>();
builder.Services.AddScoped<IObservationService, ObservationService>();


// Background Service
builder.Services.AddHostedService<WeatherUpdateBackgroundService>();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
        
        // Add event handlers for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Farm Management API v1");
    c.RoutePrefix = "swagger";
});

// Create database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// ✅ Map Hubs - This is CORRECT after building the app
app.MapHub<MonitoringHub>("/monitoringHub");
app.MapHub<SensorHub>("/sensorHub");

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();