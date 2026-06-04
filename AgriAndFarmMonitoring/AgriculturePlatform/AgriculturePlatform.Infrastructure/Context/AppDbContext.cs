using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.YieldReports;

namespace AgriculturePlatform.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // Admin DbSets
    public DbSet<Company> Companies { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    
    // Crop Monitoring DbSets
    public DbSet<Field> Fields { get; set; }
    public DbSet<CropCycle> CropCycles { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Observation> Observations { get; set; }
    public DbSet<WeatherData> WeatherData { get; set; }
    
    // Worker Management DbSets
    public DbSet<Worker> Workers { get; set; }
    public DbSet<WorkerTask> Tasks { get; set; }
    
    // Yield Reports DbSets
    public DbSet<Harvest> Harvests { get; set; }
    public DbSet<QualityCheck> QualityChecks { get; set; }
    public DbSet<YieldReport> YieldReports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // =============================================
        // COMPANY CONFIGURATION
        // =============================================
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            
            entity.HasMany(e => e.Admins)
                  .WithOne(a => a.Company)
                  .HasForeignKey(a => a.CompanyId)
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
                  
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.IsActive);
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Fields)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Fields)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.CompanyId, e.Status });
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.CropCycles)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.CropCycles)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.CompanyId, e.Status });
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.SensorReadings)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.SensorReadings)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FieldId, e.RecordedAt });
            entity.HasIndex(e => new { e.CompanyId, e.RecordedAt });
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Alerts)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.CompanyId, e.IsResolved });
            entity.HasIndex(e => new { e.FieldId, e.IsResolved });
            entity.HasIndex(e => e.Severity);
        });
        
        // =============================================
        // OBSERVATION CONFIGURATION
        // =============================================
        modelBuilder.Entity<Observation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CropHealth).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PestType).HasMaxLength(100);
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Observations)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FieldId, e.ObservationDate });
            entity.HasIndex(e => e.WorkerId);
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.WeatherData)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.FieldId, e.RecordedAt });
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
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Workers)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Workers)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.CompanyId, e.Role });
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Tasks)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Admin)
                  .WithMany(a => a.Tasks)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.WorkerId, e.Status });
            entity.HasIndex(e => new { e.CompanyId, e.Status });
            entity.HasIndex(e => new { e.Status, e.DueDate });
        });
        
        // =============================================
        // HARVEST CONFIGURATION
        // =============================================
        modelBuilder.Entity<Harvest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuantityKg).HasPrecision(12, 2);
            entity.Property(e => e.QualityGrade).HasConversion<string>().HasMaxLength(10);
            entity.Property(e => e.ApprovalStatus).HasMaxLength(20);
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Harvests)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Add explicit relationship for Harvester (Worker)
    entity.HasOne(e => e.Harvester)
          .WithMany(w => w.Harvests)
          .HasForeignKey(e => e.HarvestedBy)
          .OnDelete(DeleteBehavior.Restrict);

                  
            entity.HasOne(e => e.Approver)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => new { e.CompanyId, e.HarvestDate });
            entity.HasIndex(e => e.CropCycleId);
            entity.HasIndex(e => e.ApprovalStatus);
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
            entity.Property(e => e.ApprovalStatus).HasMaxLength(20);
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.QualityChecks)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Add explicit relationship for Checker (Worker)
    entity.HasOne(e => e.Checker)
          .WithMany(w => w.QualityChecks)
          .HasForeignKey(e => e.CheckedBy)
          .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Approver)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => e.HarvestId);
            entity.HasIndex(e => e.ApprovalStatus);
        });
        
        // =============================================
        // YIELD REPORT CONFIGURATION
        // =============================================
        modelBuilder.Entity<YieldReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalYieldKg).HasPrecision(12, 2);
            entity.Property(e => e.YieldPerHectareKg).HasPrecision(10, 2);
            entity.Property(e => e.ReportType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.AvgQualityGrade).HasConversion<string>().HasMaxLength(10);
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.YieldReports)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.CompanyId, e.ReportDate });
            entity.HasIndex(e => e.CropCycleId);
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.AuditLogs)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasIndex(e => new { e.CompanyId, e.CreatedAt });
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
            
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Notifications)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.WorkerId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.AdminId);
        });
    }
    
    // Auto-update timestamps
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
        
        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Modified)
            {
                var updatedAtProperty = entityEntry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                {
                    updatedAtProperty.SetValue(entityEntry.Entity, DateTime.UtcNow);
                }
            }
            
            if (entityEntry.State == EntityState.Added)
            {
                var createdAtProperty = entityEntry.Entity.GetType().GetProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.CanWrite)
                {
                    var currentValue = createdAtProperty.GetValue(entityEntry.Entity);
                    if (currentValue == null || (DateTime)currentValue == default)
                    {
                        createdAtProperty.SetValue(entityEntry.Entity, DateTime.UtcNow);
                    }
                }
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}