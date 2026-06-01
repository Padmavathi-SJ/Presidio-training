using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;

namespace LibrarySystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
            
        }
        
        public DbSet<Member> Members { get; set; }
       
        public DbSet<Book> Books { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

                        //  Member Entity
            modelBuilder.Entity<Member>(entity =>
            {
                entity.ToTable("members");
                entity.HasKey(e => e.Id);
                
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
                
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
        
              modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");
                entity.HasKey(e => e.Id);
                    
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.ISBN)
                    .HasMaxLength(20);
                entity.HasIndex(e => e.ISBN).IsUnique();
                
                entity.Property(e => e.PublicationYear)
                    .IsRequired();
                    
                entity.Property(e => e.NoOfCopies)
                    .HasDefaultValue(1);
                  
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
           
        }
    }
}

