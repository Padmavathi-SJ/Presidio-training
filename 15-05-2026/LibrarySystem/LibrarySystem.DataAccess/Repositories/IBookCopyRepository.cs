using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IBookCopyRepository
    {
        Task<BookCopy> GetByIdAsync(int id);
        Task<BookCopy> GetByBookCopyIdAsync(int bookCopyId);
        Task<BookCopy> UpdateAsync(BookCopy bookCopy);
        Task<List<BookCopy>> GetAvailableCopiesByBookIdAsync(int bookId);
        Task<bool> IsBookCopyAvailableAsync(int bookCopyId);
    }
}