using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.DTOs;

namespace LibrarySystem.Interfaces
{
    public interface IBookFilter
    {
        Task<List<Book>> GetBooksByPublicationYearRange(BookFilterDTO request);
    }
}