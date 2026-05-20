using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public class BookCopyRepository : IBookCopyRepository
    {
        private readonly ApplicationDbContext _context;

        public BookCopyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookCopy> GetByIdAsync(int id)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.Id == id);
        }

        public async Task<BookCopy> GetByBookCopyIdAsync(int bookCopyId)
        {
            return await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);
        }

        public async Task<BookCopy> UpdateAsync(BookCopy bookCopy)
        {
            _context.BookCopies.Update(bookCopy);
            await _context.SaveChangesAsync();
            return bookCopy;
        }

        public async Task<List<BookCopy>> GetAvailableCopiesByBookIdAsync(int bookId)
        {
            return await _context.BookCopies
                .Where(bc => bc.BookId == bookId && bc.IsAvailable && !bc.IsBorrowed && !bc.IsDamaged)
                .ToListAsync();
        }

        public async Task<bool> IsBookCopyAvailableAsync(int bookCopyId)
        {
            return await _context.BookCopies
                .AnyAsync(bc => bc.BookCopyId == bookCopyId && 
                               bc.IsAvailable && 
                               !bc.IsBorrowed && 
                               !bc.IsDamaged);
        }
    }
}