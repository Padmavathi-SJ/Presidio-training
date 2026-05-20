using Microsoft.EntityFrameworkCore;

using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public class BookCategoryRepository : IBookCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public BookCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookCategory> AddBookCategory(BookCategory bookCategory)
        {
            await _context.BookCategories.AddAsync(bookCategory);

            await _context.SaveChangesAsync();

            return bookCategory;
        }

        public async Task<List<BookCategory>> GetAllCategories()
        {
            return await _context.BookCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<BookCategory?> GetById(int id)
        {
            return await _context.BookCategories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> ExistsByName(string categoryName)
        {
            return await _context.BookCategories
                .AnyAsync(c =>
                    c.CategoryName.ToLower() ==
                    categoryName.ToLower());
        }
    }
}