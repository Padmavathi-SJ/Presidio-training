using System.Linq;
using LibrarySystem.Interfaces;
using LibrarySystem.DTOs;
using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services
{
    public class BookFilterWithPaginationService : IBookFilterWithPagination
    {
        private readonly LibraryDbContext _context;
        
        public BookFilterWithPaginationService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponseDTO<Book>> GetBooksWithPaginationAsync(BookFilterWithPaginationDTO filter)
        {
            var query = _context.Books.AsQueryable();
            
            if (filter.FromPublicationYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear >= filter.FromPublicationYear.Value);
            }

            if (filter.ToPublicationYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear <= filter.ToPublicationYear.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                query = query.Where(b => b.Title.Contains(filter.Title));
            }

             if (!string.IsNullOrWhiteSpace(filter.Author))
            {
                query = query.Where(b => b.Author.Contains(filter.Author));
            }
            
            var totalCount = await query.CountAsync();

            var items = await query
            .OrderBy(b => b.PublicationYear) 
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

            return new PaginatedResponseDTO<Book>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }
}