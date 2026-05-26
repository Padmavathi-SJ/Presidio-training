using System.Linq;
using LibrarySystem.Interfaces;
using LibrarySystem.DTOs;
using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services
{
    public class BookFilterService : IBookFilter
    {
        private readonly LibraryDbContext _context;
        
        public BookFilterService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetBooksByPublicationYearRange(BookFilterDTO request)
        {
            var query = _context.Books.AsQueryable();
            
            if (request.FromPublicationYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear >= request.FromPublicationYear.Value);
            }

            if (request.ToPublicationYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear <= request.ToPublicationYear.Value);
            }

            query = query.OrderBy(b => b.PublicationYear);

            return await query.ToListAsync();  
        }
    }
}