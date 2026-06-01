using System;
using LibrarySystem.Repositories;
using LibrarySystem.Interfaces;
using LibrarySystem.Models;
using LibrarySystem.Data;
using LibrarySystem.Exceptions;  

namespace LibrarySystem.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
            {
                throw new ValidationException("Book title is required.");
            }

            if (string.IsNullOrWhiteSpace(book.Author))
            {
                throw new ValidationException("Author name is required.");
            }

            if (book.NoOfCopies <= 0)
            {
                throw new ValidationException("Number of copies must be greater than zero.");
            }

            return await _bookRepository.AddBook(book);
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllBooks();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            if (id <= 0)   
            {
                throw new ValidationException("Invalid id, enter a valid book id.");
            }
            return await _bookRepository.GetById(id);
        }
    }
}