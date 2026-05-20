using System;
using LibrarySystem.Interfaces;
using LibrarySystem.Repositories;
using LibrarySystem.Models;

namespace LibrarySystem.Interfaces
{
    public interface IBookService
    {
        Task<Book> AddBookAsync(Book book);
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetByIdAsync(int id);
    }
}