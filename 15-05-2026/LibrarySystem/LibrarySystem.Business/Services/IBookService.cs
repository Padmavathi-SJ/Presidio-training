using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Business.Services
{
    public interface IBookService
    {
        Task<Book> AddBookAsync(Book book);

        Task<BookCopy> AddBookCopyAsync(BookCopy bookCopy);

        Task<List<Book>> GetAllBooksAsync();

        Task<List<Book>> GetBooksByCategoryAsync(int category_id);

        Task<List<Book>> SearchByTitleAsync(string title);

        Task<List<Book>> SearchByAuthorAsync(string author);

        Task MarkBookCopyAsDamagedAsync(int book_copy_id);

        Task MarkBookCopyAsUnavailableAsync(int book_copy_id);
    }
}