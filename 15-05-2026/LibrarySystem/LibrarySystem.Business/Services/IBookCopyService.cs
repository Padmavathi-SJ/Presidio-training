using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Business.Services
{
    public interface IBookCopyService
    {
        Task<BookCopy> AddBookCopyAsync(BookCopy bookCopy);
        Task<BookCopy> GetBookCopyByIdAsync(int id);
        Task<BookCopy> GetBookCopyByCopyIdAsync(int bookCopyId);
        Task<List<BookCopy>> GetAllBookCopiesAsync();
        Task<List<BookCopy>> GetAvailableBookCopiesAsync();
        Task<List<BookCopy>> GetBookCopiesByBookIdAsync(int bookId);
        Task<List<BookCopy>> GetDamagedBookCopiesAsync();
        Task<List<BookCopy>> GetBorrowedBookCopiesAsync();
        Task<BookCopy> UpdateBookCopyAsync(BookCopy bookCopy);
        Task MarkBookCopyAsDamagedAsync(int bookCopyId);
        Task MarkBookCopyAsAvailableAsync(int bookCopyId);
        Task MarkBookCopyAsUnavailableAsync(int bookCopyId);
        Task<bool> IsBookCopyAvailableAsync(int bookCopyId);
        Task<int> GetAvailableCopiesCountByBookIdAsync(int bookId);
    }
}