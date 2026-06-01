using Microsoft.EntityFrameworkCore;
using System;
using LibrarySystem.Interfaces;
using LibrarySystem.Data;
using LibrarySystem.Models;


namespace LibrarySystem.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _context;

        public BookRepository(LibraryDbContext context){
            _context = context;
        }

        public async Task<Book> AddBook(Book book)
        {
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<List<Book>> GetAllBooks()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book?> GetById(int id)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
        } 


    }
}