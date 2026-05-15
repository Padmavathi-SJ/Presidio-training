using Microsoft.EntityFrameworkCore;
using NotificationSystem.DataAccess.Entities;
using NotificationSystem.DataAccess.Config;

namespace NotificationSystem.DataAccess.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
        {
            
        }
        //DbSets
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNum).HasColumnName("phone_num").IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
                entity.Property(e => e.ReceiveEmailNotification).HasColumnName("receiveemailnotification").HasDefaultValue(true);
                entity.Property(e => e.ReceiveSmsNotification).HasColumnName("receivesmsnotification").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.Email).IsUnique();
            });

            //notification entity
            modelBuilder.Entity<NotificationEntity>(entity =>
            {
               entity.ToTable("notifications");

               entity.HasKey(e => e.Id);
               entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn();
               entity.Property(e => e.UserId).HasColumnName("user_id");
               entity.Property(e => e.UserName).HasColumnName("user_name").IsRequired().HasMaxLength(50);
               entity.Property(e => e.Type).HasColumnName("type");
               entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(100);
               entity.Property(e => e.Message).HasColumnName("message").IsRequired().HasMaxLength(200);
               entity.Property(e => e.Recipient).HasColumnName("recipient").IsRequired().HasMaxLength(100);
               entity.Property(e => e.IsSent).HasColumnName("is_sent").HasDefaultValue(true);
               entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
               entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
               entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

               entity.HasOne<UserEntity>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);

               // configuring indexes
               entity.HasIndex(e => e.UserId).HasDatabaseName("idx_notification_user_id");
               entity.HasIndex(e => e.Type).HasDatabaseName("idx_notification_type");
               entity.HasIndex(e => e.IsSent).HasDatabaseName("idx_notification_is_sent");
               entity.HasIndex(e => e.SentAt).HasDatabaseName("idx_notification_sent_at");
            });
        }
    }
}