using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Repositories;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.Business.Exceptions;


namespace LibrarySystem.Business.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ApplicationDbContext _context;

        public BookService(IBookRepository bookRepository, ApplicationDbContext context)
        {
            _bookRepository = bookRepository;
            _context = context;
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

            return await _bookRepository.AddAsync(book);
        }

        public async Task<BookCopy> AddBookCopyAsync(BookCopy bookCopy)
        {
            if (bookCopy.BookId <= 0)
                throw new ValidationException("Invalid book ID.");

            // Check if book exists
            var book = await _context.Books.FindAsync(bookCopy.BookId);
            if (book == null)
                throw new NotFoundException($"Book with ID {bookCopy.BookId} not found.");
            
            // Check if copy ID already exists
            var existingCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopy.BookCopyId);
            
            if (existingCopy != null)
throw new DuplicateException("Book", "ISBN", book.ISBN);
            
            // Set default values
            bookCopy.IsAvailable = true;
            bookCopy.IsBorrowed = false;
            bookCopy.IsDamaged = false;
            bookCopy.CreatedAt = DateTime.UtcNow;
            bookCopy.UpdatedAt = DateTime.UtcNow;
            
            await _context.BookCopies.AddAsync(bookCopy);
            await _context.SaveChangesAsync();
            
            // Update the book's NoOfCopies count
            book.NoOfCopies = await _context.BookCopies.CountAsync(bc => bc.BookId == bookCopy.BookId);
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            
            return bookCopy;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllBooksAsync();
        }

        public async Task<List<Book>> GetBooksByCategoryAsync(int category_id)
        {
            if (category_id <= 0)
            {
                throw new ValidationException("Invalid category id.");
            }

            return await _bookRepository.GetByCategoryAsync(category_id);
        }

        public async Task<List<Book>> SearchByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ValidationException("Title is required.");
            }

            return await _bookRepository.GetByTitle(title);
        }

        public async Task<List<Book>> SearchByAuthorAsync(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                throw new ValidationException("Author is required.");
            }

            return await _bookRepository.GetByAuthor(author);
        }

        public async Task MarkBookCopyAsDamagedAsync(int book_copy_id)
        {
            if (book_copy_id <= 0)
            {
                throw new ValidationException("Invalid book copy id.");
            }

            await _bookRepository.MarkAsDamaged(book_copy_id);
        }

        public async Task MarkBookCopyAsUnavailableAsync(int book_copy_id)
        {
            if (book_copy_id <= 0)
            {
                throw new ValidationException("Invalid book copy id.");
            }

            await _bookRepository.MarkAsUnAvailable(book_copy_id);
        }
    }
}