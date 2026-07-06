using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Security.Claims; 
using Microsoft.AspNetCore.Http;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Common;


namespace AgriculturePlatform.Infrastructure.Context;

public class AppDbContext : DbContext
{
      private readonly IHttpContextAccessor? _httpContextAccessor;
          public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

     public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) 
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    // Admin DbSets
    public DbSet<Farm> Farms { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    // Crop Monitoring DbSets
    public DbSet<Field> Fields { get; set; }
    public DbSet<CropCycle> CropCycles { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<AlertThreshold> AlertThresholds { get; set; }  // ADD THIS
    public DbSet<Observation> Observations { get; set; }
    public DbSet<WeatherData> WeatherData { get; set; }
    public DbSet<WeatherAlert> WeatherAlerts { get; set; }
    
    // Worker Management DbSets
    public DbSet<Worker> Workers { get; set; }
    public DbSet<WorkerTask> Tasks { get; set; }
    public DbSet<WorkerFieldAssignment> WorkerFieldAssignments { get; set; }
    
    // Yield Reports DbSets
    public DbSet<Harvest> Harvests { get; set; }
    public DbSet<QualityCheck> QualityChecks { get; set; }
    public DbSet<YieldReport> YieldReports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // =============================================
        // FARM CONFIGURATION
        // =============================================
        modelBuilder.Entity<Farm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FarmName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            
            entity.HasMany(e => e.Admins)
                  .WithOne(a => a.Farm)
                  .HasForeignKey(a => a.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // =============================================
        // ADMIN CONFIGURATION
        // =============================================
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            
            entity.HasOne(e => e.Creator)
                  .WithMany(e => e.CreatedAdmins)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => e.FarmId);
            entity.HasIndex(e => e.IsActive);
        });

 // =============================================
        // REFRESH TOKEN
        // =============================================
      
modelBuilder.Entity<RefreshToken>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.Token).IsUnique();
    entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
    entity.Property(e => e.JwtId).IsRequired().HasMaxLength(100);
    entity.Property(e => e.ExpiryDate).HasColumnType("timestamp with time zone");
    entity.Property(e => e.CreatedByIp).HasMaxLength(45);
    entity.Property(e => e.RevokedByIp).HasMaxLength(45);
    
    entity.HasOne(e => e.Admin)
          .WithMany()
          .HasForeignKey(e => e.AdminId)
          .OnDelete(DeleteBehavior.Cascade);
          
    entity.HasOne(e => e.Worker)
          .WithMany()
          .HasForeignKey(e => e.WorkerId)
          .OnDelete(DeleteBehavior.Cascade);
          
    entity.HasIndex(e => new { e.AdminId, e.IsRevoked });
    entity.HasIndex(e => new { e.WorkerId, e.IsRevoked });
    entity.HasIndex(e => e.ExpiryDate);
});    


        // =============================================
        // FIELD CONFIGURATION
        // =============================================
        modelBuilder.Entity<Field>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.AreaHectares).HasPrecision(10, 2);
            entity.Property(e => e.SoilType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.Fields)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Fields)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FarmId, e.Status });
            entity.HasIndex(e => e.AdminId);
            entity.HasIndex(e => e.FieldName);
        });
        
        // =============================================
        // CROP CYCLE CONFIGURATION
        // =============================================
        modelBuilder.Entity<CropCycle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CropType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.GrowthStage).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.CropCycles)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.CropCycles)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FarmId, e.Status });
            entity.HasIndex(e => e.FieldId);
            entity.HasIndex(e => e.PlantingDate);
        });
        
        // =============================================
        // SENSOR READING CONFIGURATION
        // =============================================
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Value).HasPrecision(10, 2);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.RecordedAt).HasColumnType("timestamp with time zone");
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.SensorReadings)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.SensorReadings)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Field)
                  .WithMany(f => f.SensorReadings)
                  .HasForeignKey(e => e.FieldId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.CropCycle)
                  .WithMany(c => c.SensorReadings)
                  .HasForeignKey(e => e.CropCycleId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Relationship with Alerts
            entity.HasMany(e => e.Alerts)
                  .WithOne()
                  .HasForeignKey("SensorReadingId")
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasIndex(e => new { e.FieldId, e.RecordedAt });
            entity.HasIndex(e => new { e.FarmId, e.RecordedAt });
            entity.HasIndex(e => e.SensorType);
        });
        
        // =============================================
        // ALERT CONFIGURATION
        // =============================================
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Severity).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.SensorValue).HasPrecision(10, 2);
            entity.Property(e => e.ThresholdValue).HasPrecision(10, 2);
            entity.Property(e => e.ResolvedAt).HasColumnType("timestamp with time zone");
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.Alerts)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Alerts)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Field)
                  .WithMany(f => f.Alerts)
                  .HasForeignKey(e => e.FieldId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.CropCycle)
                  .WithMany(c => c.Alerts)
                  .HasForeignKey(e => e.CropCycleId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasIndex(e => new { e.FarmId, e.IsResolved });
            entity.HasIndex(e => new { e.FieldId, e.IsResolved });
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.AlertType);
        });
        
        // =============================================
        // ALERT THRESHOLD CONFIGURATION
        // =============================================
        modelBuilder.Entity<AlertThreshold>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CropType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.GrowthStage).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SensorType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MinValue).HasPrecision(10, 2);
            entity.Property(e => e.MaxValue).HasPrecision(10, 2);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.NotificationEmails).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(e => e.Farm)
                  .WithMany()
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany()
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => new { e.FarmId, e.CropType, e.GrowthStage, e.SensorType })
                  .IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        
  // =============================================
// OBSERVATION CONFIGURATION - UPDATED WITH VALIDATION FIELDS
// =============================================
modelBuilder.Entity<Observation>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // Basic properties
    entity.Property(e => e.CropHealth)
          .HasConversion<string>()
          .HasMaxLength(20);
    entity.Property(e => e.PestType)
          .HasMaxLength(100);
    entity.Property(e => e.Notes)
          .HasMaxLength(1000);
    entity.Property(e => e.ObservationDate)
          .HasColumnType("timestamp with time zone")
          .IsRequired();
    
    // NEW: Validation and comments fields
    entity.Property(e => e.ValidationStatus)
          .IsRequired()
          .HasMaxLength(20)
          .HasDefaultValue("pending")
          .HasComment("pending, verified, questioned, invalid");
    entity.Property(e => e.AdminNotes)
          .HasMaxLength(1000)
          .HasComment("Admin's questions or comments on the observation");
    entity.Property(e => e.WorkerResponse)
          .HasMaxLength(1000)
          .HasComment("Worker's response to admin questions");
    entity.Property(e => e.FlagReason)
          .HasMaxLength(50)
          .HasComment("outlier, inconsistent_data, missing_info, duplicate");
    entity.Property(e => e.ValidatedAt)
          .HasColumnType("timestamp with time zone")
          .HasComment("When the observation was validated");
          
    // NEW: Image fields
    entity.Property(e => e.ImagePath).HasMaxLength(500);
    entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
    entity.Property(e => e.ImageCaption).HasMaxLength(500);
    entity.Property(e => e.AdditionalImagePaths)
          .HasColumnType("jsonb")
          .HasConversion(
              v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
              v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(v, (System.Text.Json.JsonSerializerOptions)null)
          );
    entity.Property(e => e.ImageMetadata).HasColumnType("jsonb");
    entity.Property(e => e.IsImageVerified).HasDefaultValue(false);
    entity.Property(e => e.ImageVerificationNotes).HasMaxLength(1000);
    
    // Relationships
    entity.HasOne(e => e.Farm)
          .WithMany(f => f.Observations)
          .HasForeignKey(e => e.FarmId)
          .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Admin)
          .WithMany()  // Admin doesn't have an Observations collection
          .HasForeignKey(e => e.AdminId)
          .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.Field)
          .WithMany(f => f.Observations)
          .HasForeignKey(e => e.FieldId)
          .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.CropCycle)
          .WithMany(cc => cc.Observations)
          .HasForeignKey(e => e.CropCycleId)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne(e => e.Worker)
          .WithMany(w => w.Observations)
          .HasForeignKey(e => e.WorkerId)
          .OnDelete(DeleteBehavior.SetNull);
    
    // NEW: Validator relationship
    entity.HasOne(e => e.Validator)
          .WithMany()  // Admin doesn't have a ValidatedObservations collection
          .HasForeignKey(e => e.ValidatedBy)
          .OnDelete(DeleteBehavior.Restrict);
    
    // Indexes for performance
    entity.HasIndex(e => new { e.FieldId, e.ObservationDate })
          .HasDatabaseName("IX_Observations_Field_Date");
    entity.HasIndex(e => e.WorkerId)
          .HasDatabaseName("IX_Observations_WorkerId");
    entity.HasIndex(e => e.ValidationStatus)
          .HasDatabaseName("IX_Observations_ValidationStatus");
    entity.HasIndex(e => new { e.FarmId, e.ValidationStatus })
          .HasDatabaseName("IX_Observations_Farm_ValidationStatus");
    entity.HasIndex(e => e.ValidatedBy)
          .HasDatabaseName("IX_Observations_ValidatedBy");
    entity.HasIndex(e => e.ObservationDate)
          .HasDatabaseName("IX_Observations_Date");
}); 

        // =============================================
        // WEATHER DATA CONFIGURATION
        // =============================================
        modelBuilder.Entity<WeatherData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Temperature).HasPrecision(5, 2);
            entity.Property(e => e.Humidity).HasPrecision(5, 2);
            entity.Property(e => e.RainfallMm).HasPrecision(6, 2);
            entity.Property(e => e.WindSpeed).HasPrecision(5, 2);
            entity.Property(e => e.Condition).HasConversion<string>().HasMaxLength(20);
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.WeatherData)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FieldId, e.RecordedAt });
        });
        
        // =============================================
        // WEATHER ALERT CONFIGURATION
        // =============================================
        modelBuilder.Entity<WeatherAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Severity).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Temperature).HasPrecision(5, 2);
            entity.Property(e => e.WindSpeed).HasPrecision(5, 2);
            entity.Property(e => e.RainfallMm).HasPrecision(6, 2);
            
            entity.HasOne(e => e.Farm)
                  .WithMany()
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany()
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Field)
                  .WithMany()
                  .HasForeignKey(e => e.FieldId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Acknowledger)
                  .WithMany()
                  .HasForeignKey(e => e.AcknowledgedBy)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => new { e.FarmId, e.FieldId });
            entity.HasIndex(e => e.AlertType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.AlertTime);
            entity.HasIndex(e => e.IsAcknowledged);
            entity.HasIndex(e => e.ExpiresAt);
        });
        
        // =============================================
        // WORKER CONFIGURATION
        // =============================================
        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255); 
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.LastLoginAt).HasColumnType("timestamp with time zone");
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.Workers)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Workers)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FarmId, e.Role });
            entity.HasIndex(e => e.IsActive);
        });
        
        // =============================================
        // WORKER TASK CONFIGURATION
        // =============================================
        modelBuilder.Entity<WorkerTask>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskName).HasConversion<string>().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.Tasks)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Tasks)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.WorkerId, e.Status });
            entity.HasIndex(e => new { e.FarmId, e.Status });
            entity.HasIndex(e => new { e.Status, e.DueDate });
        });
        
        // =============================================
        // WORKER FIELD ASSIGNMENT CONFIGURATION
        // =============================================
        modelBuilder.Entity<WorkerFieldAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(e => e.Farm)
                  .WithMany()
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany()
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Worker)
                  .WithMany()
                  .HasForeignKey(e => e.WorkerId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Field)
                  .WithMany()
                  .HasForeignKey(e => e.FieldId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FarmId, e.WorkerId, e.FieldId });
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.AssignedDate);
        });
        
// =============================================
// HARVEST CONFIGURATION - UPDATED WITH ALL FIELDS
// =============================================
// Infrastructure/Context/AppDbContext.cs - Fix the Harvest configuration

// =============================================
// HARVEST CONFIGURATION - FIXED
// =============================================
modelBuilder.Entity<Harvest>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // Basic properties
    entity.Property(e => e.QuantityKg)
          .HasPrecision(12, 2)
          .IsRequired();
    entity.Property(e => e.QualityGrade)
          .HasConversion<string>()
          .HasMaxLength(10);
    entity.Property(e => e.HarvestMethod)
          .HasConversion<string>()
          .HasMaxLength(20);
    entity.Property(e => e.HarvestDate)
          .HasColumnType("timestamp with time zone")
          .IsRequired();
    
    // Approval Workflow properties
    entity.Property(e => e.ApprovalStatus)
          .HasMaxLength(20)
          .HasDefaultValue("PENDING");
    entity.Property(e => e.RejectionReason)
          .HasMaxLength(500);
    entity.Property(e => e.AdminNotes)
          .HasMaxLength(1000);
    entity.Property(e => e.WorkerResponse)
          .HasMaxLength(1000);
    entity.Property(e => e.ApprovedAt)
          .HasColumnType("timestamp with time zone");
    
    // Financial and tracking properties
    entity.Property(e => e.PricePerKg)
          .HasPrecision(10, 2);
    entity.Property(e => e.BatchNumber)
          .HasMaxLength(50);
    entity.Property(e => e.Notes)
          .HasMaxLength(1000);
    
    // Computed property - ignore for database
    entity.Ignore(e => e.TotalValue);

    // ✅ FIXED: Image properties with simple conversion
    entity.Property(e => e.ImagePath).HasMaxLength(500);
    entity.Property(e => e.ThumbnailPath).HasMaxLength(500);
    entity.Property(e => e.ImageCaption).HasMaxLength(500);
    
    // ✅ FIXED: Use simple expression without statement body
    entity.Property(e => e.AdditionalImagePaths)
        .HasColumnType("jsonb")
        .IsRequired(false)
        .HasConversion(
            v => v == null || !v.Any() ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
        );
    
    entity.Property(e => e.ImageMetadata).HasColumnType("jsonb");
    
    // Relationships
    entity.HasOne(e => e.Farm)
          .WithMany(f => f.Harvests)
          .HasForeignKey(e => e.FarmId)
          .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Admin)
          .WithMany()
          .HasForeignKey(e => e.AdminId)
          .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.Field)
          .WithMany()
          .HasForeignKey(e => e.FieldId)
          .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.CropCycle)
          .WithMany(cc => cc.Harvests)
          .HasForeignKey(e => e.CropCycleId)
          .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.Harvester)
          .WithMany(w => w.Harvests)
          .HasForeignKey(e => e.HarvestedBy)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne(e => e.Submitter)
          .WithMany()
          .HasForeignKey(e => e.SubmittedBy)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne(e => e.Approver)
          .WithMany()
          .HasForeignKey(e => e.ApprovedBy)
          .OnDelete(DeleteBehavior.SetNull);
    
    // Indexes for performance
    entity.HasIndex(e => new { e.FarmId, e.HarvestDate })
          .HasDatabaseName("IX_Harvests_Farm_Date");
    entity.HasIndex(e => e.CropCycleId)
          .HasDatabaseName("IX_Harvests_CropCycleId");
    entity.HasIndex(e => e.ApprovalStatus)
          .HasDatabaseName("IX_Harvests_ApprovalStatus");
    entity.HasIndex(e => e.SubmittedBy)
          .HasDatabaseName("IX_Harvests_SubmittedBy");
    entity.HasIndex(e => e.HarvestedBy)
          .HasDatabaseName("IX_Harvests_HarvestedBy");
    entity.HasIndex(e => e.BatchNumber)
          .HasDatabaseName("IX_Harvests_BatchNumber");
});

// =============================================
// QUALITY CHECK CONFIGURATION
// =============================================
modelBuilder.Entity<QualityCheck>(entity =>
{
    entity.HasKey(e => e.Id);
    
    entity.Property(e => e.MoisturePct).HasPrecision(5, 2);
    entity.Property(e => e.DefectPct).HasPrecision(5, 2);
    entity.Property(e => e.FinalGrade).HasConversion<string>().HasMaxLength(10);
    entity.Property(e => e.ApprovalStatus).HasMaxLength(20).HasDefaultValue("PENDING");
    entity.Property(e => e.RejectionReason).HasMaxLength(500);
    entity.Property(e => e.AdminNotes).HasMaxLength(1000);
    entity.Property(e => e.WorkerResponse).HasMaxLength(1000);
    entity.Property(e => e.CheckDate).HasColumnType("timestamp with time zone");
    entity.Property(e => e.ApprovedAt).HasColumnType("timestamp with time zone");
    
    entity.HasOne(e => e.Farm)
          .WithMany(f => f.QualityChecks)
          .HasForeignKey(e => e.FarmId)
          .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Harvest)
          .WithMany(h => h.QualityChecks)
          .HasForeignKey(e => e.HarvestId)
          .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Checker)
          .WithMany(w => w.QualityChecks)
          .HasForeignKey(e => e.CheckedBy)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne(e => e.Approver)
          .WithMany()
          .HasForeignKey(e => e.ApprovedBy)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasIndex(e => e.HarvestId);
    entity.HasIndex(e => e.CheckedBy);
    entity.HasIndex(e => e.ApprovalStatus);
    entity.HasIndex(e => e.CheckDate);
});


// =============================================
// YIELD REPORT CONFIGURATION - CORRECTED
// =============================================
modelBuilder.Entity<YieldReport>(entity =>
{
    entity.HasKey(e => e.Id);
    
    // Basic properties
    entity.Property(e => e.ReportName)
          .IsRequired()
          .HasMaxLength(200);
    entity.Property(e => e.ReportType)
          .HasMaxLength(20);
    entity.Property(e => e.StartDate)
          .HasColumnType("timestamp with time zone")
          .IsRequired();
    entity.Property(e => e.EndDate)
          .HasColumnType("timestamp with time zone")
          .IsRequired();
    
    // Yield statistics
    entity.Property(e => e.TotalYieldKg)
          .HasPrecision(12, 2);
    entity.Property(e => e.AverageYieldPerHectare)  // ✅ Use correct property name
          .HasPrecision(10, 2);
    entity.Property(e => e.TotalHarvests);
    entity.Property(e => e.AveragePricePerKg)
          .HasPrecision(10, 2);
    entity.Property(e => e.TotalValue)
          .HasPrecision(12, 2);
    
    // Quality statistics
    entity.Property(e => e.AverageQualityGrade)  // ✅ Use correct property name
          .HasMaxLength(10);
    entity.Property(e => e.PassRate)
          .HasPrecision(5, 2);
    entity.Property(e => e.RejectionRate)
          .HasPrecision(5, 2);
    
    // JSON fields
    entity.Property(e => e.FieldBreakdownJson)
          .HasColumnType("jsonb");
    entity.Property(e => e.CropTypeBreakdownJson)
          .HasColumnType("jsonb");
    entity.Property(e => e.MonthlyTrendJson)
          .HasColumnType("jsonb");
    entity.Property(e => e.QualityDistributionJson)
          .HasColumnType("jsonb");
    
    // Export tracking
    entity.Property(e => e.FilePath)
          .HasMaxLength(500);
    entity.Property(e => e.FileFormat)
          .HasMaxLength(20);
    entity.Property(e => e.ExportedAt)
          .HasColumnType("timestamp with time zone");
       
    // Scheduling
    entity.Property(e => e.ScheduleCron)
          .HasMaxLength(100);
    entity.Property(e => e.LastGeneratedAt)
          .HasColumnType("timestamp with time zone");
    entity.Property(e => e.NextScheduledRun)
          .HasColumnType("timestamp with time zone");
    
    // Relationships
    entity.HasOne(e => e.Farm)
          .WithMany(f => f.YieldReports)
          .HasForeignKey(e => e.FarmId)
          .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(e => e.Admin)
          .WithMany()
          .HasForeignKey(e => e.AdminId)
          .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(e => e.CropCycle)
          .WithMany(cc => cc.YieldReports)
          .HasForeignKey(e => e.CropCycleId)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne(e => e.Field)
          .WithMany()
          .HasForeignKey(e => e.FieldId)
          .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasOne(e => e.Exporter)
          .WithMany()
          .HasForeignKey(e => e.ExportedBy)
          .OnDelete(DeleteBehavior.SetNull);
    
    // Indexes for performance
    entity.HasIndex(e => new { e.FarmId, e.StartDate, e.EndDate })
          .HasDatabaseName("IX_YieldReports_Farm_DateRange");
    entity.HasIndex(e => e.CropCycleId)
          .HasDatabaseName("IX_YieldReports_CropCycleId");
    entity.HasIndex(e => e.FieldId)
          .HasDatabaseName("IX_YieldReports_FieldId");
    entity.HasIndex(e => e.ReportType)
          .HasDatabaseName("IX_YieldReports_ReportType");
    entity.HasIndex(e => e.IsScheduled)
          .HasDatabaseName("IX_YieldReports_IsScheduled");
    entity.HasIndex(e => e.NextScheduledRun)
          .HasDatabaseName("IX_YieldReports_NextScheduledRun");
    entity.HasIndex(e => e.CreatedAt)
          .HasDatabaseName("IX_YieldReports_CreatedAt");
});

        // =============================================
        // AUDIT LOG CONFIGURATION
        // =============================================
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.OldValue).HasColumnType("jsonb");
            entity.Property(e => e.NewValue).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.AuditLogs)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasIndex(e => new { e.FarmId, e.CreatedAt });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Action);
        });
        
        // =============================================
        // NOTIFICATION CONFIGURATION
        // =============================================
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Farm)
                  .WithMany(f => f.Notifications)
                  .HasForeignKey(e => e.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.WorkerId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.AdminId);
        });
    }
    
 // AgriculturePlatform.Infrastructure/Context/AppDbContext.cs

private JsonDocument? SerializeToJsonDocument(object? obj)
{
    if (obj == null) return null;
    
    var options = new JsonSerializerOptions
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        // ✅ Add this to convert enums to strings
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    
    var json = JsonSerializer.Serialize(obj, options);
    return JsonDocument.Parse(json);
}   
    
   

    // =============================================
    // GLOBAL AUDIT LOGGING - SaveChangesAsync
    // =============================================

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted);
        
        var auditLogs = new List<AuditLog>();
        var now = DateTime.UtcNow;
        
        // Get current user info from HttpContext
        var userInfo = GetCurrentUserInfo();
        var adminId = userInfo.adminId;
        var workerId = userInfo.workerId;
        var farmId = userInfo.farmId;
        var ipAddress = userInfo.ipAddress;
        var userAgent = userInfo.userAgent;
        
        // JSON options with enum converter
        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        
        foreach (var entityEntry in entries)
        {
            // Skip AuditLog entity itself
            if (entityEntry.Entity is AuditLog)
                continue;
            
            // Skip SensorReadings
            if (entityEntry.Entity is SensorReading)
                continue;
            
            // Skip WeatherData
            if (entityEntry.Entity is WeatherData)
                continue;
            
            var entityType = entityEntry.Entity.GetType().Name;
            var entityId = GetEntityId(entityEntry.Entity);
            var entityFarmId = farmId ?? GetFarmIdFromEntity(entityEntry.Entity);
            
            // Handle DateTime conversions to UTC
            ConvertDateTimesToUtc(entityEntry);
            
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    // Set audit fields
                    HandleAuditFields(entityEntry, adminId, workerId, now);
                    
                    // Serialize entity with enum converter
                    var addedJson = JsonSerializer.Serialize(entityEntry.Entity, jsonOptions);
                    
                    auditLogs.Add(new AuditLog
                    {
                        FarmId = entityFarmId,
                        AdminId = adminId,
                        WorkerId = workerId,
                        Action = "CREATE",
                        EntityType = entityType,
                        EntityId = entityId,
                        OldValue = null,
                        NewValue = JsonDocument.Parse(addedJson),
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        CreatedAt = now
                    });
                    break;
                    
                case EntityState.Modified:
                    // Get changed properties
                    var changedProperties = entityEntry.Properties
                        .Where(p => p.IsModified && !AreValuesEqual(p.OriginalValue, p.CurrentValue))
                        .ToList();
                    
                    if (changedProperties.Any())
                    {
                        // Set audit fields
                        HandleAuditFields(entityEntry, adminId, workerId, now);
                        
                        // Get OLD state
                        var oldDict = GetEntityState(entityEntry.Entity, useOriginalValues: true);
                        var oldJson = JsonSerializer.Serialize(oldDict, jsonOptions);
                        
                        // Get NEW state
                        var newDict = GetEntityState(entityEntry.Entity, useOriginalValues: false);
                        var newJson = JsonSerializer.Serialize(newDict, jsonOptions);
                        
                        auditLogs.Add(new AuditLog
                        {
                            FarmId = entityFarmId,
                            AdminId = adminId,
                            WorkerId = workerId,
                            Action = "UPDATE",
                            EntityType = entityType,
                            EntityId = entityId,
                            OldValue = JsonDocument.Parse(oldJson),
                            NewValue = JsonDocument.Parse(newJson),
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            CreatedAt = now
                        });
                    }
                    break;
                    
                case EntityState.Deleted:
                    var deletedDict = GetEntityState(entityEntry.Entity, useOriginalValues: true);
                    var deletedJson = JsonSerializer.Serialize(deletedDict, jsonOptions);
                    
                    auditLogs.Add(new AuditLog
                    {
                        FarmId = entityFarmId,
                        AdminId = adminId,
                        WorkerId = workerId,
                        Action = "DELETE",
                        EntityType = entityType,
                        EntityId = entityId,
                        OldValue = JsonDocument.Parse(deletedJson),
                        NewValue = null,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        CreatedAt = now
                    });
                    break;
            }
        }
        
        // Add all audit logs
        if (auditLogs.Any())
        {
            await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    // =============================================
    // HELPER METHODS
    // =============================================
private Dictionary<string, object?> GetEntityState(object entity, bool useOriginalValues = false)
{
    var state = new Dictionary<string, object?>();
    var entry = Entry(entity);

    foreach (var property in entity.GetType().GetProperties())
    {
        try
        {
            // Skip navigation properties
            if (property.PropertyType.IsClass &&
                property.PropertyType != typeof(string) &&
                property.PropertyType.Namespace?.StartsWith("AgriculturePlatform.Domain.Entities") == true)
            {
                continue;
            }

            object? value;

            if (useOriginalValues)
            {
                var propertyEntry = entry.Properties
                    .FirstOrDefault(p => p.Metadata.Name == property.Name);

                value = propertyEntry?.OriginalValue;
            }
            else
            {
                value = property.GetValue(entity);
            }

            if (value == null)
            {
                state[property.Name] = null;
                continue;
            }

            // Convert enums to string
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (propertyType.IsEnum)
            {
                state[property.Name] = value.ToString();
                continue;
            }

            // Format DateTime
            if (value is DateTime dt)
            {
                state[property.Name] = dt.ToUniversalTime()
                    .ToString("yyyy-MM-dd HH:mm:ss.fffZ");
                continue;
            }

            state[property.Name] = value;
        }
        catch
        {
            // Ignore inaccessible properties
        }
    }

    return state;
}

    private (int? adminId, int? workerId, int? farmId, string ipAddress, string? userAgent) GetCurrentUserInfo()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null)
            return (null, null, null, "127.0.0.1", null);
        
        // Get Admin ID (from token)
        var adminIdClaim = httpContext.User.FindFirst("adminId")?.Value ?? 
                          httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? adminId = null;
        if (!string.IsNullOrEmpty(adminIdClaim) && int.TryParse(adminIdClaim, out var aId))
        {
            var userType = httpContext.User.FindFirst("userType")?.Value;
            if (userType == "Admin")
                adminId = aId;
        }
        
        // Get Worker ID (from token)
        var workerIdClaim = httpContext.User.FindFirst("workerId")?.Value;
        int? workerId = null;
        if (!string.IsNullOrEmpty(workerIdClaim) && int.TryParse(workerIdClaim, out var wId))
        {
            var userType = httpContext.User.FindFirst("userType")?.Value;
            if (userType == "Worker")
                workerId = wId;
        }
        
        // Get Farm ID (from token)
        var farmIdClaim = httpContext.User.FindFirst("farmId")?.Value;
        int? farmId = null;
        if (!string.IsNullOrEmpty(farmIdClaim) && int.TryParse(farmIdClaim, out var fId))
            farmId = fId;
        
        // Get IP Address
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = httpContext.Connection.RemoteIpAddress?.ToString();
        var ipAddress = ip ?? "127.0.0.1";
        
        // Get User Agent
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (string.IsNullOrEmpty(userAgent))
            userAgent = null;
        
        return (adminId, workerId, farmId, ipAddress, userAgent);
    }

    private void ConvertDateTimesToUtc(EntityEntry entityEntry)
    {
        var properties = entityEntry.Entity.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));
        
        foreach (var property in properties)
        {
            var currentValue = property.GetValue(entityEntry.Entity);
            if (currentValue != null)
            {
                var dateTime = (DateTime)currentValue;
                if (dateTime.Kind != DateTimeKind.Utc)
                {
                    var utcValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    property.SetValue(entityEntry.Entity, utcValue);
                }
            }
        }
    }

    private void HandleAuditFields(EntityEntry entityEntry, int? adminId, int? workerId, DateTime now)
    {
        if (entityEntry.Entity is BaseEntity baseEntity)
        {
            if (entityEntry.State == EntityState.Added)
            {
                baseEntity.CreatedAt = now;
                baseEntity.CreatedBy = adminId ?? workerId;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                baseEntity.UpdatedAt = now;
                baseEntity.UpdatedBy = adminId ?? workerId;
            }
        }
    }

    private int? GetEntityId(object entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty == null) return null;
        
        var value = idProperty.GetValue(entity);
        if (value == null) return null;
        
        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            short shortValue => (int)shortValue,
            _ => null
        };
    }

    private int? GetFarmIdFromEntity(object entity)
    {
        var farmIdProp = entity.GetType().GetProperty("FarmId");
        if (farmIdProp != null)
        {
            var value = farmIdProp.GetValue(entity);
            return value as int?;
        }
        return null;
    }

    private bool AreValuesEqual(object? original, object? current)
    {
        if (original == null && current == null) return true;
        if (original == null || current == null) return false;
        return original.Equals(current);
    }


}

