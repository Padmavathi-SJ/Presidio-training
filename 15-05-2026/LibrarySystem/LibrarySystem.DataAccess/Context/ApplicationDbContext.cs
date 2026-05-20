
using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<BorrowingRules> BorrowingRules { get; set; }
        public DbSet<Fine> Fines { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Admin Entity
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.PhoneNum)
                    .IsRequired()
                    .HasMaxLength(20);
                
                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50);
                    
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
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
                    .HasMaxLength(50);
                
                // Enum conversions
                entity.Property(e => e.MembershipType)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue(MembershipType.Basic);
                    
                entity.Property(e => e.MembershipStatus)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue(MembershipStatus.Active);
                    
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.AllowedBorrowingCount)
                    .HasDefaultValue(2);

                entity.Property(e => e.CurrentBorrowedCount)
                    .HasDefaultValue(0);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            // BookCategory Entity
            modelBuilder.Entity<BookCategory>(entity =>
            {
                entity.ToTable("book_categories");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.CategoryName).IsUnique();
                
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            //  Book Entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CategoryId)
                    .IsRequired();
                    
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
                    
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            //  BookCopy Entity
            modelBuilder.Entity<BookCopy>(entity =>
            {
                entity.ToTable("book_copies");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.BookId)
                    .IsRequired();
                    
                entity.Property(e => e.BookCopyId)
                    .IsRequired();
                    
                entity.HasIndex(e => e.BookCopyId).IsUnique();
                
                entity.Property(e => e.IsAvailable)
                    .HasDefaultValue(true);
                    
                entity.Property(e => e.IsBorrowed)
                    .HasDefaultValue(false);
                    
                entity.Property(e => e.IsDamaged)
                    .HasDefaultValue(false);
                    
                entity.Property(e => e.ConditionNotes)
                    .HasMaxLength(500);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            // CBorrowing Entity
            modelBuilder.Entity<Borrowing>(entity =>
            {
                entity.ToTable("borrowings");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.MemberId)
                    .IsRequired();
                    
                entity.Property(e => e.BookCopyId)
                    .IsRequired();
                    
                entity.Property(e => e.BookId)
                    .IsRequired();
                    
                entity.Property(e => e.BorrowedDate)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.DueDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();
                    
                entity.Property(e => e.MemberReturnedDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);
                    
                entity.Property(e => e.FineAmount)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0);
                    
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue(BookBorrowStatus.Borrowed);
                    
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            modelBuilder.Entity<BorrowingRules>(entity =>
            {
                entity.ToTable("borrowing_rules");
                entity.HasKey(e => e.Id);

                 entity.Property(e => e.MembershipType)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                
                entity.Property(e => e.MaxActiveBorrowings)
                    .IsRequired();
                
                entity.Property(e => e.MaxBorrowDays)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                 
            });

            //  Fine Entity
            modelBuilder.Entity<Fine>(entity =>
            {
                entity.ToTable("fines");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.BorrowingId)
                    .IsRequired();
                    
                entity.Property(e => e.MemberId)
                    .IsRequired();
                    
                entity.Property(e => e.FineAmount)
                    .HasPrecision(10, 2)
                    .IsRequired();
                    
                entity.Property(e => e.FineReason)
                    .HasMaxLength(200);
                    
                entity.Property(e => e.PaymentStatus)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue(FinePaymentStatus.Pending);
                    
                entity.Property(e => e.PaymentDate)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired(false);
                    
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            //  Relationships
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCopies)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.Member)
                .WithMany(m => m.Borrowings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.BookCopy)
                .WithMany(bc => bc.Borrowings)
                .HasForeignKey(b => b.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.Book)
                .WithMany(bk => bk.Borrowings)
                .HasForeignKey(b => b.BookId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.Borrowing)
                .WithMany(b => b.Fines)
                .HasForeignKey(f => f.BorrowingId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.Member)
                .WithMany(m => m.Fines)
                .HasForeignKey(f => f.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}