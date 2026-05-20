using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.Business.Exceptions;

namespace LibrarySystem.Business.Services
{
    public class BookCopyService : IBookCopyService
    {
        private readonly ApplicationDbContext _context;

        public BookCopyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookCopy> AddBookCopyAsync(BookCopy bookCopy)
        {
            if (bookCopy.BookId <= 0)
                throw new ValidationException("Invalid book ID.");

            // Check if book exists
            var book = await _context.Books.FindAsync(bookCopy.BookId);
            if (book == null)
                throw new NotFoundException($"Book with ID {bookCopy.BookId} not found.");
            
            // Check if copy ID already exists
            var existingCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopy.BookCopyId);
            
            if (existingCopy != null)
                throw new DuplicateException($"Book copy with ID {bookCopy.BookCopyId} already exists.");
            
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

        public async Task<BookCopy> GetBookCopyByIdAsync(int id)
        {
            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Id == id);
            
            if (bookCopy == null)
                throw new NotFoundException($"Book copy with ID {id} not found.");
            
            return bookCopy;
        }

        public async Task<BookCopy> GetBookCopyByCopyIdAsync(int bookCopyId)
        {
            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);
            
            if (bookCopy == null)
                throw new NotFoundException($"Book copy with Copy ID {bookCopyId} not found.");
            
            return bookCopy;
        }

        public async Task<List<BookCopy>> GetAllBookCopiesAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .OrderBy(bc => bc.BookCopyId)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetAvailableBookCopiesAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.IsAvailable && !bc.IsBorrowed && !bc.IsDamaged)
                .OrderBy(bc => bc.BookCopyId)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetBookCopiesByBookIdAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                throw new NotFoundException($"Book with ID {bookId} not found.");
            
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.BookId == bookId)
                .OrderBy(bc => bc.BookCopyId)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetDamagedBookCopiesAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.IsDamaged)
                .OrderBy(bc => bc.BookCopyId)
                .ToListAsync();
        }

        public async Task<List<BookCopy>> GetBorrowedBookCopiesAsync()
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.IsBorrowed)
                .OrderBy(bc => bc.BookCopyId)
                .ToListAsync();
        }

        public async Task<BookCopy> UpdateBookCopyAsync(BookCopy bookCopy)
        {
            var existingCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.Id == bookCopy.Id);
            
            if (existingCopy == null)
                throw new NotFoundException($"Book copy with ID {bookCopy.Id} not found.");
            
            existingCopy.IsAvailable = bookCopy.IsAvailable;
            existingCopy.IsBorrowed = bookCopy.IsBorrowed;
            existingCopy.IsDamaged = bookCopy.IsDamaged;
            existingCopy.ConditionNotes = bookCopy.ConditionNotes;
            existingCopy.UpdatedAt = DateTime.UtcNow;
            
            _context.BookCopies.Update(existingCopy);
            await _context.SaveChangesAsync();
            
            return existingCopy;
        }

        public async Task MarkBookCopyAsDamagedAsync(int bookCopyId)
        {
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);
            
            if (bookCopy == null)
                throw new NotFoundException($"Book copy with ID {bookCopyId} not found.");
            
            bookCopy.IsDamaged = true;
            bookCopy.IsAvailable = false;
            bookCopy.IsBorrowed = false;
            bookCopy.UpdatedAt = DateTime.UtcNow;
            
            _context.BookCopies.Update(bookCopy);
            await _context.SaveChangesAsync();
            
            // Update book's NoOfCopies count
            var book = await _context.Books.FindAsync(bookCopy.BookId);
            if (book != null)
            {
                book.NoOfCopies = await _context.BookCopies
                    .CountAsync(bc => bc.BookId == bookCopy.BookId && !bc.IsDamaged);
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkBookCopyAsAvailableAsync(int bookCopyId)
        {
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);
            
            if (bookCopy == null)
                throw new NotFoundException($"Book copy with ID {bookCopyId} not found.");
            
            bookCopy.IsAvailable = true;
            bookCopy.IsBorrowed = false;
            bookCopy.IsDamaged = false;
            bookCopy.UpdatedAt = DateTime.UtcNow;
            
            _context.BookCopies.Update(bookCopy);
            await _context.SaveChangesAsync();
        }

        public async Task MarkBookCopyAsUnavailableAsync(int bookCopyId)
        {
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);
            
            if (bookCopy == null)
                throw new NotFoundException($"Book copy with ID {bookCopyId} not found.");
            
            bookCopy.IsAvailable = false;
            bookCopy.IsBorrowed = false;
            bookCopy.UpdatedAt = DateTime.UtcNow;
            
            _context.BookCopies.Update(bookCopy);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBookCopyAvailableAsync(int bookCopyId)
        {
            return await _context.BookCopies
                .AnyAsync(bc => bc.BookCopyId == bookCopyId && 
                               bc.IsAvailable && 
                               !bc.IsBorrowed && 
                               !bc.IsDamaged);
        }

        public async Task<int> GetAvailableCopiesCountByBookIdAsync(int bookId)
        {
            return await _context.BookCopies
                .CountAsync(bc => bc.BookId == bookId && 
                                 bc.IsAvailable && 
                                 !bc.IsBorrowed && 
                                 !bc.IsDamaged);
        }
    }
}