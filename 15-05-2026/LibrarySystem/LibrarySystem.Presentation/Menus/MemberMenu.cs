using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.Presentation.Helpers;

namespace LibrarySystem.Presentation.Menus
{
    public static class MemberMenu
    {
        

        public static async Task Show(
            IBookService bookService, 
            IMemberService memberService, 
            IBorrowingService borrowingService,
            IFineService fineService,
            ApplicationDbContext context)  
        {
            
            // The _currentMemberId is set by SetCurrentMember before Show is called
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("               MEMBER PANEL             ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. View All Books");
                Console.WriteLine("2. Search Books by Title");
                Console.WriteLine("3. Search Books by Author");
                Console.WriteLine("4. Search Books by Category");
                Console.WriteLine("5. Borrow/Return a Book");
                Console.WriteLine("6. View My Library Summary");
                Console.WriteLine("7. View My Borrowing History");
                Console.WriteLine("8. View Fines");
                Console.WriteLine("9. Logout");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (1-9): ");
                
                string choice = Console.ReadLine() ?? "0";

                switch (choice)
                {
                    case "1":
                        await ViewAllBooks(bookService);
                        break;
                    case "2":
                        await SearchBooksByTitle(bookService);
                        break;
                    case "3":
                        await SearchBooksByAuthor(bookService);
                        break;
                    case "4":
                        await SearchBooksByCategory(bookService);
                        break;
                    case "5":
                        await BorrowingMenu.Show(borrowingService, bookService, context);
                        break;
                    case "6":
                        await ViewMySummary(memberService, fineService);
                        break;
                    case "7":
                        await ViewBorrowingHistory(borrowingService);
                        break;
                    case "8":
                      //  MemberFineManagementMenu.SetCurrentMember(UserSession.CurrentMemberId);
                        await MemberFineManagementMenu.Show(fineService);
                        break;
                    case "9":
                    UserSession.Logout();
                        Console.WriteLine("Logging out...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select 1-9.");
                        await WaitForUserInput();
                        break;
                }
            }
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
                    Console.WriteLine($"Total Books Available: {books.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Title",-40} {"Author",-25} {"Copies",-10} {"Year"}");
                    Console.WriteLine(new string('-', 90));
                    
                    foreach (var book in books)
                    {
                        Console.WriteLine($"{book.Id,-5} {book.Title,-40} {book.Author,-25} {book.NoOfCopies,-10} {book.PublicationYear}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchBooksByTitle(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH BOOKS BY TITLE");
            Console.WriteLine("====================");
            
            Console.Write("Enter title (or part of it): ");
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
                    foreach (var book in books)
                    {
                        Console.WriteLine($"  {book.Title} by {book.Author} (ISBN: {book.ISBN}, Copies: {book.NoOfCopies})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchBooksByAuthor(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH BOOKS BY AUTHOR");
            Console.WriteLine("=====================");
            
            Console.Write("Enter author name: ");
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
                    foreach (var book in books)
                    {
                        Console.WriteLine($"  {book.Title} (ISBN: {book.ISBN}, Copies: {book.NoOfCopies})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchBooksByCategory(IBookService bookService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH BOOKS BY CATEGORY");
            Console.WriteLine("=======================");
            
            Console.Write("Enter Category ID: ");
            string input = Console.ReadLine() ?? "";
            
            if (!int.TryParse(input, out int categoryId) || categoryId <= 0)
            {
                Console.WriteLine("Invalid category ID.");
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
                    foreach (var book in books)
                    {
                        Console.WriteLine($"  {book.Title} by {book.Author} (ISBN: {book.ISBN})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewBorrowingHistory(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("MY BORROWING HISTORY");
            Console.WriteLine("===================");
            
            try
            {
                var borrowings = await borrowingService.GetMemberBorrowingsAsync(UserSession.CurrentMemberId);
                
                if (borrowings == null || !borrowings.Any())
                {
                    Console.WriteLine("No borrowing history found.");
                }
                else
                {
                    Console.WriteLine($"Total Borrowings: {borrowings.Count}");
                    Console.WriteLine();
                    
                    foreach (var borrowing in borrowings)
                    {
                        Console.WriteLine($"ID: {borrowing.Id}");
                        Console.WriteLine($"Book Copy ID: {borrowing.BookCopyId}");
                        Console.WriteLine($"Borrowed: {borrowing.BorrowedDate:yyyy-MM-dd}");
                        Console.WriteLine($"Due Date: {borrowing.DueDate:yyyy-MM-dd}");
                        Console.WriteLine($"Returned: {(borrowing.MemberReturnedDate?.ToString("yyyy-MM-dd") ?? "Not returned")}");
                        Console.WriteLine($"Status: {borrowing.Status}");
                        if (borrowing.FineAmount > 0)
                            Console.WriteLine($"Fine: Rs.{borrowing.FineAmount}");
                        Console.WriteLine(new string('-', 40));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewMySummary(IMemberService memberService, IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("MY LIBRARY SUMMARY");
            Console.WriteLine("=================");
            
            try
            {
                var summary = await memberService.GetMemberBorrowingSummaryAsync(UserSession.CurrentMemberId);
                var member = await memberService.GetById(UserSession.CurrentMemberId);
                
                Console.WriteLine();
                Console.WriteLine($"Member: {member?.Name}");
                Console.WriteLine($"Membership Type: {member?.MembershipType}");
                Console.WriteLine($"Allowed Borrowings: {member?.AllowedBorrowingCount ?? 0}");
                Console.WriteLine($"Current Borrowed: {member?.CurrentBorrowedCount ?? 0}");
                
                Console.WriteLine();
                Console.WriteLine("BORROWING SUMMARY");
                Console.WriteLine("=================");
                Console.WriteLine($"Active Borrowings: {summary.Active}");
                Console.WriteLine($"Returned Borrowings: {summary.Returned}");
                Console.WriteLine($"Overdue Borrowings: {summary.Overdue}");
                Console.WriteLine($"Total Unpaid Fine: Rs.{summary.Fine}");
                
                if (summary.Overdue > 0)
                {
                    Console.WriteLine("\n⚠️ Warning: You have overdue books! Please return them soon to avoid additional fines.");
                }
                
                if (summary.Fine > 500)
                {
                    Console.WriteLine("\n⚠️ Warning: Your unpaid fines exceed Rs.500. You cannot borrow new books until you clear them.");
                }
                else if (summary.Fine > 0)
                {
                    Console.WriteLine($"\nYou have unpaid fines of Rs.{summary.Fine}. Please pay them at the library counter.");
                }
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