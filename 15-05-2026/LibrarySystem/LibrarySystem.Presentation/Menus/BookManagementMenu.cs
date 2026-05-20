using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Presentation.Menus
{
    public static class BookManagementMenu
    {
        public static async Task Show(IBookCopyService bookCopyService, IBookService bookService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("              BOOK MANAGEMENT           ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Add New Book");
                Console.WriteLine("2. Add Book Copy");
                Console.WriteLine("3. View All Books");
                Console.WriteLine("4. Search By Title");
                Console.WriteLine("5. Search By Author");
                Console.WriteLine("6. Search By Category");
                Console.WriteLine("7. Mark Book Copy As Damaged");
                Console.WriteLine("8. Mark Book Copy As Unavailable");
                Console.WriteLine("9. Back to Main Menu");
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
                        await AddNewBook(bookService);
                        break;
                    case 2:
                        await BookCopyManagementMenu.Show(bookCopyService, bookService);
                        break;
                    case 3:
                        await ViewAllBooks(bookService);
                        break;
                    case 4:
                        await SearchByTitle(bookService);
                        break;
                    case 5:
                        await SearchByAuthor(bookService);
                        break;
                    case 6:
                        await SearchByCategory(bookService);
                        break;
                    case 7:
                        await MarkBookAsDamaged(bookService);
                        break;
                    case 8:
                        await MarkBookAsUnavailable(bookService);
                        break;
                    case 9:
                        return;
                    default:
                        Console.WriteLine("Invalid Choice! Please select 1-9.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task AddNewBook(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("ADD NEW BOOK");
            Console.WriteLine("===========");
            
            try
            {
                var book = new Book();

                Console.Write("Category Id: ");
                if (!int.TryParse(Console.ReadLine(), out int categoryId) || categoryId <= 0)
                {
                    Console.WriteLine("Invalid Category ID.");
                    await WaitForUserInput();
                    return;
                }
                book.CategoryId = categoryId;

                Console.Write("Title: ");
                string title = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine("Title is required.");
                    await WaitForUserInput();
                    return;
                }
                book.Title = title;

                Console.Write("Author: ");
                string author = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(author))
                {
                    Console.WriteLine("Author is required.");
                    await WaitForUserInput();
                    return;
                }
                book.Author = author;

                Console.Write("ISBN: ");
                book.ISBN = Console.ReadLine() ?? "";

                Console.Write("Publication Year: ");
                if (!int.TryParse(Console.ReadLine(), out int year) || year < 1800 || year > DateTime.Now.Year)
                {
                    Console.WriteLine($"Invalid Publication Year. Must be between 1800 and {DateTime.Now.Year}.");
                    await WaitForUserInput();
                    return;
                }
                book.PublicationYear = year;

                Console.Write("Number Of Copies: ");
                if (!int.TryParse(Console.ReadLine(), out int copies) || copies <= 0)
                {
                    Console.WriteLine("Number of copies must be greater than 0.");
                    await WaitForUserInput();
                    return;
                }
                book.NoOfCopies = copies;

                await bookService.AddBookAsync(book);
                Console.WriteLine("Book Added Successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAllBooks(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("ALL BOOKS");
            Console.WriteLine("=========");
            
            try
            {
                var books = await bookService.GetAllBooksAsync();
                
                if (books == null || !books.Any())
                {
                    Console.WriteLine("No books found in the library.");
                }
                else
                {
                    Console.WriteLine($"Total Books: {books.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Title",-40} {"Author",-25} {"ISBN",-15} {"Year",-8} {"Copies"}");
                    Console.WriteLine(new string('-', 98));
                    
                    foreach (var b in books)
                    {
                        Console.WriteLine($"{b.Id,-5} {b.Title,-40} {b.Author,-25} {b.ISBN,-15} {b.PublicationYear,-8} {b.NoOfCopies}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchByTitle(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH BOOKS BY TITLE");
            Console.WriteLine("====================");
            
            Console.Write("Enter Title (or part of it): ");
            string title = Console.ReadLine() ?? "";
            
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Title cannot be empty.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var books = await bookService.SearchByTitleAsync(title);
                
                if (books == null || !books.Any())
                {
                    Console.WriteLine($"No books found with title containing '{title}'.");
                }
                else
                {
                    Console.WriteLine($"Found {books.Count} book(s):");
                    Console.WriteLine();
                    foreach (var b in books)
                    {
                        Console.WriteLine($"  {b.Title} by {b.Author} (ISBN: {b.ISBN}, Copies: {b.NoOfCopies})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchByAuthor(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH BOOKS BY AUTHOR");
            Console.WriteLine("=====================");
            
            Console.Write("Enter Author Name: ");
            string author = Console.ReadLine() ?? "";
            
            if (string.IsNullOrWhiteSpace(author))
            {
                Console.WriteLine("Author name cannot be empty.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var books = await bookService.SearchByAuthorAsync(author);
                
                if (books == null || !books.Any())
                {
                    Console.WriteLine($"No books found by author '{author}'.");
                }
                else
                {
                    Console.WriteLine($"Found {books.Count} book(s) by {author}:");
                    Console.WriteLine();
                    foreach (var b in books)
                    {
                        Console.WriteLine($"  {b.Title} (ISBN: {b.ISBN}, Copies: {b.NoOfCopies})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchByCategory(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH BOOKS BY CATEGORY");
            Console.WriteLine("=======================");
            
            Console.Write("Enter Category ID: ");
            if (!int.TryParse(Console.ReadLine(), out int categoryId) || categoryId <= 0)
            {
                Console.WriteLine("Invalid Category ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var books = await bookService.GetBooksByCategoryAsync(categoryId);
                
                if (books == null || !books.Any())
                {
                    Console.WriteLine($"No books found in category ID {categoryId}.");
                }
                else
                {
                    Console.WriteLine($"Found {books.Count} book(s):");
                    Console.WriteLine();
                    foreach (var b in books)
                    {
                        Console.WriteLine($"  {b.Title} by {b.Author} (ISBN: {b.ISBN})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task MarkBookAsDamaged(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("MARK BOOK COPY AS DAMAGED");
            Console.WriteLine("=======================");
            
            Console.Write("Enter Book Copy ID: ");
            if (!int.TryParse(Console.ReadLine(), out int damagedId) || damagedId <= 0)
            {
                Console.WriteLine("Invalid Book Copy ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                await bookService.MarkBookCopyAsDamagedAsync(damagedId);
                Console.WriteLine("Book Copy Marked As Damaged!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task MarkBookAsUnavailable(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("MARK BOOK COPY AS UNAVAILABLE");
            Console.WriteLine("============================");
            
            Console.Write("Enter Book Copy ID: ");
            if (!int.TryParse(Console.ReadLine(), out int unavailableId) || unavailableId <= 0)
            {
                Console.WriteLine("Invalid Book Copy ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                await bookService.MarkBookCopyAsUnavailableAsync(unavailableId);
                Console.WriteLine("Book Copy Marked As Unavailable!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task WaitForUserInput()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}