using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;

        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Book> AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            return book;
        }

        public async Task<Book?> GetByIdAsync(int id)
{
    return await _context.Books
        .Include(b => b.Category)
        .Include(b => b.BookCopies)
        .FirstOrDefaultAsync(b => b.Id == id);
}

public async Task<BookCopy> AddBookCopies(BookCopy bookCopy)
{
    try
    {
        // Check if book exists
        var book = await _context.Books.FindAsync(bookCopy.BookId);
        if (book == null)
            throw new Exception($"Book with ID {bookCopy.BookId} not found.");
        
        // Check if BookCopyId already exists
        var existingCopy = await _context.BookCopies
            .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopy.BookCopyId);
        
        if (existingCopy != null)
            throw new Exception($"Book copy with ID {bookCopy.BookCopyId} already exists.");
        
        // Set default values
        bookCopy.IsAvailable = true;
        bookCopy.IsBorrowed = false;
        bookCopy.IsDamaged = false;
        bookCopy.CreatedAt = DateTime.UtcNow;
        bookCopy.UpdatedAt = DateTime.UtcNow;
        
        await _context.BookCopies.AddAsync(bookCopy);
        await _context.SaveChangesAsync();
        
        // Update the book's NoOfCopies count
        book.NoOfCopies = await _context.BookCopies.CountAsync(bc => bc.BookId == bookCopy.BookId);
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
        
        return bookCopy;
    }
    catch (Exception ex)
    {
        throw new Exception($"Error adding book copy: {ex.Message}", ex);
    }
}

public async Task<BookCopy?> GetBookCopyByCopyIdAsync(int bookCopyId)
{
    return await _context.BookCopies
        .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);
}
        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .ToListAsync();
        }

        public async Task<List<Book>> GetByCategoryAsync(int category_id)
        {
            return await _context.Books
                .Where(b => b.CategoryId == category_id)
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .ToListAsync();
        }

        public async Task<List<Book>> GetByTitle(string title)
        {
            return await _context.Books
                .Where(b => b.Title.ToLower().Contains(title.ToLower()))
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .ToListAsync();
        }

        public async Task<List<Book>> GetByAuthor(string author)
        {
            return await _context.Books
                .Where(b => b.Author.ToLower().Contains(author.ToLower()))
                .Include(b => b.Category)
                .Include(b => b.BookCopies)
                .ToListAsync();
        }

        public async Task MarkAsDamaged(int book_copy_id)
        {
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(b => b.Id == book_copy_id);

            if (bookCopy != null)
            {
                bookCopy.IsDamaged = true;
                bookCopy.IsAvailable = false;
                bookCopy.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsUnAvailable(int book_copy_id)
        {
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(b => b.Id == book_copy_id);

            if (bookCopy != null)
            {
                bookCopy.IsAvailable = false;
                bookCopy.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }
    }
}