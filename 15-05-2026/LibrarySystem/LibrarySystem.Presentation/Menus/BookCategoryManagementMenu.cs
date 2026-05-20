using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Presentation.Menus
{
    public static class BookCategoryManagementMenu
    {
        public static async Task Show(IBookCategoryService bookCategoryService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("        BOOK CATEGORY MANAGEMENT        ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Add Book Category");
                Console.WriteLine("2. Get All Active Categories");
                Console.WriteLine("3. Get Category By Id");
                Console.WriteLine("4. Back");
                Console.WriteLine("=========================================");
                Console.Write("Enter Choice: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        var category = new BookCategory();
                        Console.Write("Enter Category Name: ");
                        category.CategoryName = Console.ReadLine() ?? "";
                        await bookCategoryService.AddCategoryAsync(category);
                        Console.WriteLine("Book Category Added Successfully!");
                        break;

                    case 2:
                        var categories = await bookCategoryService.GetAllCategoriesAsync();
                        if (categories == null || !categories.Any())
                        {
                            Console.WriteLine("No categories found.");
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine($"{"Id",-5} {"Category Name",-30} {"Active"}");
                            Console.WriteLine(new string('-', 50));
                            foreach (var c in categories)
                            {
                                Console.WriteLine($"{c.Id,-5} {c.CategoryName,-30} {(c.IsActive ? "Yes" : "No")}");
                            }
                        }
                        break;

                    case 3:
                        Console.Write("Enter Category Id: ");
                        if (!int.TryParse(Console.ReadLine(), out int categoryId))
                        {
                            Console.WriteLine("Invalid Category ID.");
                            await WaitForUserInput();
                            continue;
                        }
                        var categoryById = await bookCategoryService.GetCategoryByIdAsync(categoryId);
                        if (categoryById == null)
                        {
                            Console.WriteLine("Category Not Found!");
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine($"Id: {categoryById.Id}");
                            Console.WriteLine($"Category: {categoryById.CategoryName}");
                            Console.WriteLine($"Active: {(categoryById.IsActive ? "Yes" : "No")}");
                        }
                        break;

                    case 4:
                        return;

                    default:
                        Console.WriteLine("Invalid Choice!");
                        break;
                }
                
                await WaitForUserInput();
            }
        }

        private static async Task WaitForUserInput()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}