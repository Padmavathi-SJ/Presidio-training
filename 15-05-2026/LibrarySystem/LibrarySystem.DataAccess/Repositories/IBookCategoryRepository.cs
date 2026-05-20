using System;
using System.Collections.Generic;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IBookCategoryRepository
    {
        Task<BookCategory> AddBookCategory(BookCategory bookCategory);
        Task<List<BookCategory>> GetAllCategories();
        Task<BookCategory?> GetById(int id);
        Task<bool> ExistsByName(string categoryName);
        
    }
}