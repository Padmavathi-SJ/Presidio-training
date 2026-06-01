using Microsoft.EntityFrameworkCore;
using UsersXL.Models;

namespace UsersXL.Context
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> dbContextOptions) 
            : base(dbContextOptions)
        {
        }
        
        public DbSet<User> Users { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.PhoneNum)
                    .IsRequired()
                    .HasMaxLength(20);
                
                entity.Property(e => e.Age).IsRequired();
            });
        }
    }
}