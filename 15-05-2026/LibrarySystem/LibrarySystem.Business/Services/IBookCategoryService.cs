using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Business.Services
{
    public interface IBookCategoryService
    {
        Task<BookCategory> AddCategoryAsync(BookCategory bookCategory);

        Task<List<BookCategory>> GetAllCategoriesAsync();

        Task<BookCategory?> GetCategoryByIdAsync(int id);
    }
}