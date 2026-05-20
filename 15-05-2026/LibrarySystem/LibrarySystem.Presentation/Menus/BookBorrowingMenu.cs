using LibrarySystem.Business.Services;  
using Microsoft.EntityFrameworkCore; 
using LibrarySystem.DataAccess.Context;
using LibrarySystem.Presentation.Helpers;

namespace LibrarySystem.Presentation.Menus
{
    public static class BorrowingMenu
    {
      
        
      

        public static async Task Show(IBorrowingService borrowingService, IBookService bookService, ApplicationDbContext context)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("           BOOK BORROWING SYSTEM        ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Borrow a Book");
                Console.WriteLine("2. Return a Book");
                Console.WriteLine("3. View My Borrowings");
                Console.WriteLine("4. View Active Borrowings");
                Console.WriteLine("5. Check Borrowing Limit");
                Console.WriteLine("6. Check Unpaid Fines");
                Console.WriteLine("0. Back to Main Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter Choice: ");
                
                string input = Console.ReadLine() ?? "0";
                if (!int.TryParse(input, out int choice))
                {
                    Console.WriteLine("Invalid input!");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await BorrowBook(borrowingService, bookService, context);
                        break;
                    case 2:
                        await ReturnBook(borrowingService);
                        break;
                    case 3:
                        await ViewAllBorrowings(borrowingService);
                        break;
                    case 4:
                        await ViewActiveBorrowings(borrowingService);
                        break;
                    case 5:
                        await CheckBorrowingLimit(borrowingService);
                        break;
                    case 6:
                        await CheckUnpaidFines(borrowingService);
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice!");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task BorrowBook(IBorrowingService borrowingService, IBookService bookService, ApplicationDbContext context)
        {
            Console.Clear();
            Console.WriteLine("BORROW A BOOK");
            Console.WriteLine("=============\n");

            // Check eligibility first
            var eligibility = await borrowingService.CheckBorrowingEligibilityAsync(UserSession.CurrentMemberId);
            if (!eligibility.CanBorrow)
            {
                Console.WriteLine($"Cannot borrow: {eligibility.Message}");
                await WaitForUserInput();
                return;
            }

            // Get available books with their copy IDs
            var availableBooks = await context.BookCopies
                .Include(bc => bc.Book)
                .Where(bc => bc.IsAvailable && !bc.IsBorrowed && !bc.IsDamaged)
                .GroupBy(bc => new { bc.BookId, bc.Book.Title, bc.Book.Author })
                .Select(g => new
                {
                    g.Key.BookId,
                    g.Key.Title,
                    g.Key.Author,
                    AvailableCopies = g.Count(),
                    CopyIds = g.Select(x => x.BookCopyId).ToList()
                })
                .ToListAsync();

            if (!availableBooks.Any())
            {
                Console.WriteLine("No books available for borrowing.");
                await WaitForUserInput();
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Title",-40} {"Author",-25} {"Copies"}");
            Console.WriteLine(new string('-', 80));

            foreach (var book in availableBooks)
            {
                Console.WriteLine($"{book.BookId,-5} {book.Title,-40} {book.Author,-25} {book.AvailableCopies}");
            }

            Console.Write("\nEnter Book ID: ");
            if (!int.TryParse(Console.ReadLine(), out int bookId))
            {
                Console.WriteLine("Invalid Book ID.");
                await WaitForUserInput();
                return;
            }

            var selectedBook = availableBooks.FirstOrDefault(b => b.BookId == bookId);
            if (selectedBook == null)
            {
                Console.WriteLine("Book not found.");
                await WaitForUserInput();
                return;
            }

            Console.WriteLine($"\nSelected Book: {selectedBook.Title}");
            Console.WriteLine($"Available Copy IDs: {string.Join(", ", selectedBook.CopyIds)}");
            
            Console.Write("Enter Book Copy ID (number): ");
            if (!int.TryParse(Console.ReadLine(), out int copyId))  // ✅ Parse as int
            {
                Console.WriteLine("Invalid Copy ID. Please enter a number.");
                await WaitForUserInput();
                return;
            }

            // ✅ Validate borrowing with int
            var validation = await borrowingService.ValidateBorrowingAsync(UserSession.CurrentMemberId, copyId);
            
            if (!validation.IsValid)
            {
                Console.WriteLine($"\nCannot borrow: {validation.ErrorMessage}");
                await WaitForUserInput();
                return;
            }

            Console.WriteLine("\nBorrowing Status:");
            Console.WriteLine($"  Current Borrowings: {validation.CurrentBorrowings}");
            Console.WriteLine($"  Allowed Borrowings: {validation.AllowedBorrowings}");
            Console.WriteLine($"  Available Slots: {validation.AllowedBorrowings - validation.CurrentBorrowings}");
            Console.WriteLine($"  Unpaid Fines: Rs.{validation.UnpaidFines}");

            Console.Write("\nConfirm borrowing? (y/n): ");
            string confirm = Console.ReadLine()?.ToLower() ?? "";

            if (confirm != "y")
            {
                Console.WriteLine("Borrowing cancelled.");
                await WaitForUserInput();
                return;
            }

            try
            {
                //  Pass int, not string
                var borrowing = await borrowingService.BorrowBookAsync(UserSession.CurrentMemberId, copyId);
                
                Console.WriteLine("\n✓ Book borrowed successfully!");
                Console.WriteLine($"  Borrowing ID: {borrowing.Id}");
                Console.WriteLine($"  Borrow Date: {borrowing.BorrowedDate:yyyy-MM-dd}");
                Console.WriteLine($"  Due Date: {borrowing.DueDate:yyyy-MM-dd}");
                Console.WriteLine($"  Please return by {borrowing.DueDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
            }

            await WaitForUserInput();
        }

        private static async Task ReturnBook(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("RETURN A BOOK");
            Console.WriteLine("=============\n");

            var activeBorrowings = await borrowingService.GetActiveBorrowingsAsync(UserSession.CurrentMemberId);

            if (!activeBorrowings.Any())
            {
                Console.WriteLine("No active borrowings found.");
                await WaitForUserInput();
                return;
            }

            Console.WriteLine("Your Active Borrowings:");
            Console.WriteLine($"{"ID",-5} {"Book Title",-40} {"Due Date",-15} {"Status"}");
            Console.WriteLine(new string('-', 70));

            foreach (var borrowing in activeBorrowings)
            {
                string status = borrowing.DueDate < DateTime.UtcNow ? "OVERDUE" : "Active";
                string title = borrowing.Book?.Title ?? "Unknown";
                Console.WriteLine($"{borrowing.Id,-5} {title,-40} {borrowing.DueDate:yyyy-MM-dd,-15} {status}");
            }

            Console.Write("\nEnter Borrowing ID to return: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowingId))
            {
                Console.WriteLine("Invalid ID.");
                await WaitForUserInput();
                return;
            }

            try
            {
                var returned = await borrowingService.ReturnBookAsync(borrowingId);
                
                Console.WriteLine("\n✓ Book returned successfully!");
                
                if (returned.FineAmount > 0)
                {
                    int daysLate = (int)(returned.FineAmount / 10);
                    Console.WriteLine($"\n⚠️ Book was returned {daysLate} days late.");
                    Console.WriteLine($"   Fine amount: Rs.{returned.FineAmount}");
                    Console.WriteLine($"   Please pay the fine at the library counter.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
            }

            await WaitForUserInput();
        }

        private static async Task ViewAllBorrowings(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("MY BORROWING HISTORY");
            Console.WriteLine("===================\n");

            var borrowings = await borrowingService.GetMemberBorrowingsAsync(UserSession.CurrentMemberId);

            if (!borrowings.Any())
            {
                Console.WriteLine("No borrowing history found.");
            }
            else
            {
                foreach (var borrowing in borrowings)
                {
                    Console.WriteLine($"ID: {borrowing.Id}");
                    Console.WriteLine($"Book: {borrowing.Book?.Title ?? "Unknown"}");
                    Console.WriteLine($"Copy ID: {borrowing.BookCopy?.BookCopyId}");
                    Console.WriteLine($"Borrowed: {borrowing.BorrowedDate:yyyy-MM-dd}");
                    Console.WriteLine($"Due Date: {borrowing.DueDate:yyyy-MM-dd}");
                    Console.WriteLine($"Returned: {(borrowing.MemberReturnedDate?.ToString("yyyy-MM-dd") ?? "Not returned")}");
                    Console.WriteLine($"Status: {borrowing.Status}");
                    if (borrowing.FineAmount > 0)
                        Console.WriteLine($"Fine: Rs.{borrowing.FineAmount}");
                    Console.WriteLine(new string('-', 40));
                }
            }

            await WaitForUserInput();
        }

        private static async Task ViewActiveBorrowings(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("ACTIVE BORROWINGS");
            Console.WriteLine("================\n");

            var activeBorrowings = await borrowingService.GetActiveBorrowingsAsync(UserSession.CurrentMemberId);

            if (!activeBorrowings.Any())
            {
                Console.WriteLine("No active borrowings found.");
            }
            else
            {
                foreach (var borrowing in activeBorrowings)
                {
                    Console.WriteLine($"ID: {borrowing.Id}");
                    Console.WriteLine($"Book: {borrowing.Book?.Title ?? "Unknown"}");
                    Console.WriteLine($"Due Date: {borrowing.DueDate:yyyy-MM-dd}");
                    
                    if (borrowing.DueDate < DateTime.UtcNow)
                    {
                        int daysOverdue = (DateTime.UtcNow - borrowing.DueDate).Days;
                        Console.WriteLine($"⚠️ OVERDUE! ({daysOverdue} days)");
                        Console.WriteLine($"   Late fine: Rs.{daysOverdue * 10}");
                    }
                    Console.WriteLine(new string('-', 30));
                }
            }

            await WaitForUserInput();
        }

        private static async Task CheckBorrowingLimit(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("BORROWING LIMIT INFORMATION");
            Console.WriteLine("==========================\n");

            var eligibility = await borrowingService.CheckBorrowingEligibilityAsync(UserSession.CurrentMemberId);
            
            Console.WriteLine($"Current Borrowings: {eligibility.CurrentCount}");
            Console.WriteLine($"Maximum Allowed: {eligibility.MaxAllowed}");
            Console.WriteLine($"Available Slots: {eligibility.MaxAllowed - eligibility.CurrentCount}");
            Console.WriteLine($"Unpaid Fines: Rs.{eligibility.UnpaidFines}");
            Console.WriteLine($"\nStatus: {eligibility.Message}");

            await WaitForUserInput();
        }

        private static async Task CheckUnpaidFines(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("UNPAID FINES");
            Console.WriteLine("===========\n");

            var unpaidFines = await borrowingService.GetUnpaidFineAmountAsync(UserSession.CurrentMemberId);

            if (unpaidFines == 0)
            {
                Console.WriteLine("✓ You have no unpaid fines.");
            }
            else
            {
                Console.WriteLine($"Total Unpaid Fines: Rs.{unpaidFines}");
                
                if (unpaidFines > 500)
                {
                    Console.WriteLine("\n⚠️ You cannot borrow new books until you clear fines above Rs.500.");
                }
            }

            await WaitForUserInput();
        }

        private static async Task WaitForUserInput()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}