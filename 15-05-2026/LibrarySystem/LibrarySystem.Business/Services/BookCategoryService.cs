using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Repositories;
using LibrarySystem.Business.Exceptions;


namespace LibrarySystem.Business.Services
{
    public class BookCategoryService : IBookCategoryService
    {
        private readonly IBookCategoryRepository _bookCategoryRepository;

        public BookCategoryService(IBookCategoryRepository bookCategoryRepository)
        {
            _bookCategoryRepository = bookCategoryRepository;
        }

        public async Task<BookCategory> AddCategoryAsync(BookCategory bookCategory)
        {
            if (bookCategory == null)
            {
                throw new ValidationException("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(bookCategory.CategoryName))
            {
                throw new ValidationException("Category name is required.");
            }

            if (bookCategory.CategoryName.Length > 100)
            {
                throw new ValidationException("Category name cannot exceed 100 characters.");
            }

            bool exists = await _bookCategoryRepository.ExistsByName(bookCategory.CategoryName);
            if (exists)
            {
                throw new DuplicateException("BookCategory", "CategoryName", bookCategory.CategoryName);
            }

            return await _bookCategoryRepository.AddBookCategory(bookCategory);
        }

        public async Task<List<BookCategory>> GetAllCategoriesAsync()
        {
            var categories = await _bookCategoryRepository.GetAllCategories();
            
            if (categories == null || !categories.Any())
            {
                throw new NotFoundException("No book categories found in the system.");
            }
            
            return categories;
        }

        public async Task<BookCategory?> GetCategoryByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("Category ID must be greater than zero.");
            }

            var category = await _bookCategoryRepository.GetById(id);
            
            if (category == null)
            {
                throw new NotFoundException("BookCategory", id);
            }
            
            return category;
        }
    }
}