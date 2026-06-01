
using LibrarySystem.DTOs;
using LibrarySystem.Models;

namespace LibrarySystem.Interfaces
{
    public interface IBookFilterWithPagination
    {
        Task<PaginatedResponseDTO<Book>> GetBooksWithPaginationAsync(BookFilterWithPaginationDTO filter);
    }
}