using System;
using LibrarySystem.Models;
using LibrarySystem.Data;

namespace LibrarySystem.Interfaces
{
    public interface IBookRepository
    {
        Task<Book> AddBook(Book book);
        Task<List<Book>> GetAllBooks();
        Task<Book?> GetById(int id);

    }
}