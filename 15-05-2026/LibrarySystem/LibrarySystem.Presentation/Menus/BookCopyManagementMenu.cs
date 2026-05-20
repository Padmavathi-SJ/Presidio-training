using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Presentation.Menus
{
    public static class BookCopyManagementMenu
    {
        public static async Task Show(IBookCopyService bookCopyService, IBookService bookService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("          BOOK COPY MANAGEMENT          ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Add New Book Copy");
                Console.WriteLine("2. View All Book Copies");
                Console.WriteLine("3. View Available Book Copies");
                Console.WriteLine("4. View Copies by Book ID");
                Console.WriteLine("5. View Damaged Copies");
                Console.WriteLine("6. View Borrowed Copies");
                Console.WriteLine("7. Mark Copy as Damaged");
                Console.WriteLine("8. Mark Copy as Available");
                Console.WriteLine("9. Mark Copy as Unavailable");
                Console.WriteLine("10. Back to Book Management");
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
                        await AddNewBookCopy(bookCopyService, bookService);
                        break;
                    case 2:
                        await ViewAllBookCopies(bookCopyService);
                        break;
                    case 3:
                        await ViewAvailableBookCopies(bookCopyService);
                        break;
                    case 4:
                        await ViewCopiesByBookId(bookCopyService);
                        break;
                    case 5:
                        await ViewDamagedCopies(bookCopyService);
                        break;
                    case 6:
                        await ViewBorrowedCopies(bookCopyService);
                        break;
                    case 7:
                        await MarkCopyAsDamaged(bookCopyService);
                        break;
                    case 8:
                        await MarkCopyAsAvailable(bookCopyService);
                        break;
                    case 9:
                        await MarkCopyAsUnavailable(bookCopyService);
                        break;
                    case 10:
                        return;
                    default:
                        Console.WriteLine("Invalid Choice! Please select 1-10.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task AddNewBookCopy(IBookCopyService bookCopyService, IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("ADD NEW BOOK COPY");
            Console.WriteLine("================");
            
            try
            {
                var books = await bookService.GetAllBooksAsync();
                if (books != null && books.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Available Books:");
                    Console.WriteLine($"{"ID",-5} {"Title",-40} {"Author",-25} {"Copies"}");
                    Console.WriteLine(new string('-', 75));
                    
                    foreach (var b in books)
                    {
                        Console.WriteLine($"{b.Id,-5} {b.Title,-40} {b.Author,-25} {b.NoOfCopies}");
                    }
                }
                
                Console.Write("Enter Book ID: ");
                if (!int.TryParse(Console.ReadLine(), out int bookId) || bookId <= 0)
                {
                    Console.WriteLine("Invalid Book ID.");
                    await WaitForUserInput();
                    return;
                }

                Console.Write("Enter Book Copy ID (unique number): ");
                if (!int.TryParse(Console.ReadLine(), out int bookCopyId) || bookCopyId <= 0)
                {
                    Console.WriteLine("Invalid Book Copy ID.");
                    await WaitForUserInput();
                    return;
                }

                Console.Write("Condition Notes (optional): ");
                string conditionNotes = Console.ReadLine() ?? "";

                var bookCopy = new BookCopy
                {
                    BookId = bookId,
                    BookCopyId = bookCopyId,
                    ConditionNotes = conditionNotes
                };

                var result = await bookCopyService.AddBookCopyAsync(bookCopy);
                
                Console.WriteLine($"Book Copy Added Successfully!");
                Console.WriteLine($"  Copy ID: {result.BookCopyId}");
                Console.WriteLine($"  Book: {result.Book?.Title ?? "N/A"}");
                Console.WriteLine($"  Status: Available");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Details: {ex.InnerException.Message}");
                }
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAllBookCopies(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("ALL BOOK COPIES");
            Console.WriteLine("==============");
            
            try
            {
                var copies = await bookCopyService.GetAllBookCopiesAsync();
                
                if (copies == null || !copies.Any())
                {
                    Console.WriteLine("No book copies found.");
                }
                else
                {
                    Console.WriteLine($"Total Copies: {copies.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"Copy ID",-10} {"Book ID",-10} {"Title",-30} {"Available",-12} {"Borrowed",-10} {"Damaged"}");
                    Console.WriteLine(new string('-', 85));
                    
                    foreach (var copy in copies)
                    {
                        string title = copy.Book?.Title?.Length > 27 ? copy.Book.Title.Substring(0, 24) + "..." : copy.Book?.Title ?? "N/A";
                        Console.WriteLine($"{copy.BookCopyId,-10} {copy.BookId,-10} {title,-30} {(copy.IsAvailable ? "Yes" : "No"),-12} {(copy.IsBorrowed ? "Yes" : "No"),-10} {(copy.IsDamaged ? "Yes" : "No")}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAvailableBookCopies(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("AVAILABLE BOOK COPIES");
            Console.WriteLine("====================");
            
            try
            {
                var copies = await bookCopyService.GetAvailableBookCopiesAsync();
                
                if (copies == null || !copies.Any())
                {
                    Console.WriteLine("No available book copies found.");
                }
                else
                {
                    Console.WriteLine($"Available Copies: {copies.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"Copy ID",-10} {"Book ID",-10} {"Title",-40} {"Condition"}");
                    Console.WriteLine(new string('-', 70));
                    
                    foreach (var copy in copies)
                    {
                        string title = copy.Book?.Title?.Length > 37 ? copy.Book.Title.Substring(0, 34) + "..." : copy.Book?.Title ?? "N/A";
                        Console.WriteLine($"{copy.BookCopyId,-10} {copy.BookId,-10} {title,-40} {(string.IsNullOrEmpty(copy.ConditionNotes) ? "Good" : copy.ConditionNotes)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewCopiesByBookId(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("BOOK COPIES BY BOOK ID");
            Console.WriteLine("=====================");
            
            Console.Write("Enter Book ID: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId) || bookId <= 0)
            {
                Console.WriteLine("Invalid Book ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var copies = await bookCopyService.GetBookCopiesByBookIdAsync(bookId);
                
                if (copies == null || !copies.Any())
                {
                    Console.WriteLine($"No copies found for Book ID {bookId}.");
                }
                else
                {
                    Console.WriteLine($"Copies for Book ID {bookId}: {copies.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"Copy ID",-10} {"Available",-12} {"Borrowed",-10} {"Damaged",-10} {"Condition"}");
                    Console.WriteLine(new string('-', 60));
                    
                    foreach (var copy in copies)
                    {
                        Console.WriteLine($"{copy.BookCopyId,-10} {(copy.IsAvailable ? "Yes" : "No"),-12} {(copy.IsBorrowed ? "Yes" : "No"),-10} {(copy.IsDamaged ? "Yes" : "No"),-10} {(string.IsNullOrEmpty(copy.ConditionNotes) ? "-" : copy.ConditionNotes)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewDamagedCopies(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("DAMAGED BOOK COPIES");
            Console.WriteLine("==================");
            
            try
            {
                var copies = await bookCopyService.GetDamagedBookCopiesAsync();
                
                if (copies == null || !copies.Any())
                {
                    Console.WriteLine("No damaged book copies found.");
                }
                else
                {
                    Console.WriteLine($"Damaged Copies: {copies.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"Copy ID",-10} {"Book ID",-10} {"Title",-40}");
                    Console.WriteLine(new string('-', 65));
                    
                    foreach (var copy in copies)
                    {
                        string title = copy.Book?.Title?.Length > 37 ? copy.Book.Title.Substring(0, 34) + "..." : copy.Book?.Title ?? "N/A";
                        Console.WriteLine($"{copy.BookCopyId,-10} {copy.BookId,-10} {title}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewBorrowedCopies(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("BORROWED BOOK COPIES");
            Console.WriteLine("===================");
            
            try
            {
                var copies = await bookCopyService.GetBorrowedBookCopiesAsync();
                
                if (copies == null || !copies.Any())
                {
                    Console.WriteLine("No borrowed book copies found.");
                }
                else
                {
                    Console.WriteLine($"Borrowed Copies: {copies.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"Copy ID",-10} {"Book ID",-10} {"Title",-40}");
                    Console.WriteLine(new string('-', 65));
                    
                    foreach (var copy in copies)
                    {
                        string title = copy.Book?.Title?.Length > 37 ? copy.Book.Title.Substring(0, 34) + "..." : copy.Book?.Title ?? "N/A";
                        Console.WriteLine($"{copy.BookCopyId,-10} {copy.BookId,-10} {title}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task MarkCopyAsDamaged(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("MARK BOOK COPY AS DAMAGED");
            Console.WriteLine("=======================");
            
            Console.Write("Enter Book Copy ID: ");
            if (!int.TryParse(Console.ReadLine(), out int copyId) || copyId <= 0)
            {
                Console.WriteLine("Invalid Book Copy ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                await bookCopyService.MarkBookCopyAsDamagedAsync(copyId);
                Console.WriteLine($"Book Copy {copyId} Marked as Damaged!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task MarkCopyAsAvailable(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("MARK BOOK COPY AS AVAILABLE");
            Console.WriteLine("==========================");
            
            Console.Write("Enter Book Copy ID: ");
            if (!int.TryParse(Console.ReadLine(), out int copyId) || copyId <= 0)
            {
                Console.WriteLine("Invalid Book Copy ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                await bookCopyService.MarkBookCopyAsAvailableAsync(copyId);
                Console.WriteLine($"Book Copy {copyId} Marked as Available!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task MarkCopyAsUnavailable(IBookCopyService bookCopyService)
        {
            Console.Clear();
            Console.WriteLine("MARK BOOK COPY AS UNAVAILABLE");
            Console.WriteLine("============================");
            
            Console.Write("Enter Book Copy ID: ");
            if (!int.TryParse(Console.ReadLine(), out int copyId) || copyId <= 0)
            {
                Console.WriteLine("Invalid Book Copy ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                await bookCopyService.MarkBookCopyAsUnavailableAsync(copyId);
                Console.WriteLine($"Book Copy {copyId} Marked as Unavailable!");
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