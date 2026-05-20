using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IBookRepository
    {
        Task<Book> AddAsync(Book book);
        Task<BookCopy> AddBookCopies(BookCopy bookCopy);
        Task<BookCopy?> GetBookCopyByCopyIdAsync(int bookCopyId);
        Task<Book?> GetByIdAsync(int id);  
        Task<List<Book>> GetAllBooksAsync();
        Task<List<Book>> GetByCategoryAsync(int Category_id);
        Task<List<Book>> GetByTitle(string title);
        Task<List<Book>> GetByAuthor(string author);
        Task MarkAsDamaged(int book_copy_id);
        Task MarkAsUnAvailable(int book_copy_id);
    }
}