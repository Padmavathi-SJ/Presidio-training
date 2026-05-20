using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Presentation.Menus
{
    public static class AdminBorrowingMenu
    {
        public static async Task Show(IBorrowingService borrowingService, IMemberService memberService, IBookService bookService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("      ADMIN - BORROWING MANAGEMENT       ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. View All Borrowings");
                Console.WriteLine("2. View All Active Borrowings");
                Console.WriteLine("3. View All Overdue Borrowings");
                Console.WriteLine("4. View Borrowings by Member");
                Console.WriteLine("5. View Member's Active Borrowings");
                Console.WriteLine("6. Force Return Book (Admin)");
                Console.WriteLine("7. View Borrowing Statistics");
                Console.WriteLine("0. Back to Admin Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (0-7): ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await ViewAllBorrowings(borrowingService);
                        break;
                    case 2:
                        await ViewAllActiveBorrowings(borrowingService);
                        break;
                    case 3:
                        await ViewAllOverdueBorrowings(borrowingService);
                        break;
                    case 4:
                        await ViewBorrowingsByMember(borrowingService, memberService);
                        break;
                    case 5:
                        await ViewMemberActiveBorrowings(borrowingService, memberService);
                        break;
                    case 6:
                        await ForceReturnBook(borrowingService);
                        break;
                    case 7:
                        await ViewBorrowingStatistics(borrowingService);
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select 0-7.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task ViewAllBorrowings(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("ALL BORROWINGS");
            Console.WriteLine("==============");
            
            try
            {
                var allBorrowings = await borrowingService.GetAllBorrowingsAsync();
                
                if (allBorrowings == null || !allBorrowings.Any())
                {
                    Console.WriteLine("No borrowings found in the system.");
                }
                else
                {
                    Console.WriteLine($"Total Borrowings: {allBorrowings.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Member",-25} {"Book",-35} {"Borrowed",-12} {"Due",-12} {"Returned",-12} {"Status"}");
                    Console.WriteLine(new string('-', 120));
                    
                    foreach (var borrowing in allBorrowings)
                    {
                        string returnedDate = borrowing.MemberReturnedDate?.ToString("yyyy-MM-dd") ?? "-";
                        string memberName = borrowing.Member?.Name?.Length > 22 ? borrowing.Member.Name.Substring(0, 19) + "..." : borrowing.Member?.Name ?? "N/A";
                        string bookTitle = borrowing.Book?.Title?.Length > 32 ? borrowing.Book.Title.Substring(0, 29) + "..." : borrowing.Book?.Title ?? "N/A";
                        
                        Console.WriteLine($"{borrowing.Id,-5} {memberName,-25} {bookTitle,-35} {borrowing.BorrowedDate.ToString("yyyy-MM-dd"),-12} {borrowing.DueDate.ToString("yyyy-MM-dd"),-12} {returnedDate,-12} {borrowing.Status}");
                        
                        if (borrowing.FineAmount > 0)
                        {
                            Console.WriteLine($"  Fine: Rs.{borrowing.FineAmount}");
                        }
                    }
                    
                    var activeCount = allBorrowings.Count(b => b.Status == BookBorrowStatus.Borrowed);
                    var returnedCount = allBorrowings.Count(b => b.Status == BookBorrowStatus.Returned);
                    var overdueCount = allBorrowings.Count(b => b.Status == BookBorrowStatus.Overdue);
                    var totalFines = allBorrowings.Sum(b => b.FineAmount);
                    
                    Console.WriteLine();
                    Console.WriteLine("SYSTEM SUMMARY");
                    Console.WriteLine($"Active Borrowings: {activeCount}");
                    Console.WriteLine($"Returned Borrowings: {returnedCount}");
                    Console.WriteLine($"Overdue Borrowings: {overdueCount}");
                    Console.WriteLine($"Total Fines: Rs.{totalFines}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAllActiveBorrowings(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("ALL ACTIVE BORROWINGS");
            Console.WriteLine("====================");
            
            try
            {
                var activeBorrowings = await borrowingService.GetAllActiveBorrowingsAsync();
                
                if (activeBorrowings == null || !activeBorrowings.Any())
                {
                    Console.WriteLine("No active borrowings found.");
                }
                else
                {
                    Console.WriteLine($"Active Borrowings: {activeBorrowings.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Member",-25} {"Book",-35} {"Borrowed",-12} {"Due Date",-12} {"Days Left"}");
                    Console.WriteLine(new string('-', 110));
                    
                    foreach (var borrowing in activeBorrowings)
                    {
                        int daysLeft = (borrowing.DueDate - DateTime.UtcNow).Days;
                        string daysLeftText = daysLeft < 0 ? $"Overdue by {-daysLeft}" : $"{daysLeft} days";
                        string memberName = borrowing.Member?.Name?.Length > 22 ? borrowing.Member.Name.Substring(0, 19) + "..." : borrowing.Member?.Name ?? "N/A";
                        string bookTitle = borrowing.Book?.Title?.Length > 32 ? borrowing.Book.Title.Substring(0, 29) + "..." : borrowing.Book?.Title ?? "N/A";
                        
                        Console.WriteLine($"{borrowing.Id,-5} {memberName,-25} {bookTitle,-35} {borrowing.BorrowedDate.ToString("yyyy-MM-dd"),-12} {borrowing.DueDate.ToString("yyyy-MM-dd"),-12} {daysLeftText}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAllOverdueBorrowings(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("ALL OVERDUE BORROWINGS");
            Console.WriteLine("=====================");
            
            try
            {
                var overdueBorrowings = await borrowingService.GetOverdueBorrowingsAsync();
                
                if (overdueBorrowings == null || !overdueBorrowings.Any())
                {
                    Console.WriteLine("No overdue borrowings found.");
                }
                else
                {
                    Console.WriteLine($"Overdue Borrowings: {overdueBorrowings.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Member",-25} {"Book",-35} {"Due Date",-12} {"Days Overdue",-15} {"Fine"}");
                    Console.WriteLine(new string('-', 110));
                    
                    decimal totalOverdueFines = 0;
                    
                    foreach (var borrowing in overdueBorrowings)
                    {
                        int daysOverdue = (DateTime.UtcNow - borrowing.DueDate).Days;
                        decimal fineAmount = daysOverdue * 10;
                        totalOverdueFines += fineAmount;
                        string memberName = borrowing.Member?.Name?.Length > 22 ? borrowing.Member.Name.Substring(0, 19) + "..." : borrowing.Member?.Name ?? "N/A";
                        string bookTitle = borrowing.Book?.Title?.Length > 32 ? borrowing.Book.Title.Substring(0, 29) + "..." : borrowing.Book?.Title ?? "N/A";
                        
                        Console.WriteLine($"{borrowing.Id,-5} {memberName,-25} {bookTitle,-35} {borrowing.DueDate.ToString("yyyy-MM-dd"),-12} {daysOverdue,-15} Rs.{fineAmount}");
                    }
                    
                    Console.WriteLine();
                    Console.WriteLine(new string('-', 110));
                    Console.WriteLine($"Total Potential Fines: Rs.{totalOverdueFines}");
                    Console.WriteLine();
                    Console.WriteLine("ACTION REQUIRED: Contact these members to return overdue books.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewBorrowingsByMember(IBorrowingService borrowingService, IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("BORROWINGS BY MEMBER");
            Console.WriteLine("===================");
            
            Console.Write("Enter Member ID: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid Member ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var member = await memberService.GetById(memberId);
                if (member == null)
                {
                    Console.WriteLine($"Member with ID {memberId} not found.");
                    await WaitForUserInput();
                    return;
                }
                
                var summary = await memberService.GetMemberBorrowingSummaryAsync(memberId);
                
                Console.WriteLine();
                Console.WriteLine("MEMBER INFORMATION");
                Console.WriteLine($"Name: {member.Name}");
                Console.WriteLine($"Email: {member.Email}");
                Console.WriteLine($"Membership Type: {member.MembershipType}");
                Console.WriteLine($"Allowed Borrowings: {member.AllowedBorrowingCount}");
                Console.WriteLine($"Current Borrowed: {member.CurrentBorrowedCount}");
                
                Console.WriteLine();
                Console.WriteLine("BORROWING SUMMARY");
                Console.WriteLine($"Active Borrowings: {summary.Active}");
                Console.WriteLine($"Returned Borrowings: {summary.Returned}");
                Console.WriteLine($"Overdue Borrowings: {summary.Overdue}");
                Console.WriteLine($"Total Unpaid Fine: Rs.{summary.Fine}");
                
                var borrowings = await borrowingService.GetMemberBorrowingsAsync(memberId);
                
                if (borrowings != null && borrowings.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("DETAILED BORROWING HISTORY");
                    Console.WriteLine($"{"ID",-5} {"Book",-35} {"Borrowed",-12} {"Due Date",-12} {"Returned",-12} {"Status"}");
                    Console.WriteLine(new string('-', 95));
                    
                    foreach (var borrowing in borrowings)
                    {
                        string returnedDate = borrowing.MemberReturnedDate?.ToString("yyyy-MM-dd") ?? "-";
                        string bookTitle = borrowing.Book?.Title?.Length > 32 ? borrowing.Book.Title.Substring(0, 29) + "..." : borrowing.Book?.Title ?? "N/A";
                        
                        Console.WriteLine($"{borrowing.Id,-5} {bookTitle,-35} {borrowing.BorrowedDate.ToString("yyyy-MM-dd"),-12} {borrowing.DueDate.ToString("yyyy-MM-dd"),-12} {returnedDate,-12} {borrowing.Status}");
                        
                        if (borrowing.FineAmount > 0)
                        {
                            Console.WriteLine($"  Fine: Rs.{borrowing.FineAmount}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewMemberActiveBorrowings(IBorrowingService borrowingService, IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("MEMBER'S ACTIVE BORROWINGS");
            Console.WriteLine("=========================");
            
            Console.Write("Enter Member ID: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid Member ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var member = await memberService.GetById(memberId);
                if (member == null)
                {
                    Console.WriteLine($"Member with ID {memberId} not found.");
                    await WaitForUserInput();
                    return;
                }
                
                Console.WriteLine();
                Console.WriteLine($"Active Borrowings for: {member.Name}");
                Console.WriteLine($"Membership Type: {member.MembershipType}");
                Console.WriteLine($"Allowed: {member.AllowedBorrowingCount} | Current: {member.CurrentBorrowedCount}");
                
                var activeBorrowings = await borrowingService.GetActiveBorrowingsAsync(memberId);
                
                if (activeBorrowings == null || !activeBorrowings.Any())
                {
                    Console.WriteLine("No active borrowings for this member.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"Active Borrowings: {activeBorrowings.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Book",-40} {"Borrowed",-12} {"Due Date",-12} {"Days Left"}");
                    Console.WriteLine(new string('-', 90));
                    
                    foreach (var borrowing in activeBorrowings)
                    {
                        int daysLeft = (borrowing.DueDate - DateTime.UtcNow).Days;
                        string bookTitle = borrowing.Book?.Title?.Length > 37 ? borrowing.Book.Title.Substring(0, 34) + "..." : borrowing.Book?.Title ?? "N/A";
                        
                        string daysText = daysLeft < 0 ? $"Overdue by {-daysLeft}" : $"{daysLeft} days";
                        Console.WriteLine($"{borrowing.Id,-5} {bookTitle,-40} {borrowing.BorrowedDate.ToString("yyyy-MM-dd"),-12} {borrowing.DueDate.ToString("yyyy-MM-dd"),-12} {daysText}");
                    }
                    
                    int remainingSlots = member.AllowedBorrowingCount - member.CurrentBorrowedCount;
                    Console.WriteLine();
                    Console.WriteLine($"Remaining Capacity: {remainingSlots}/{member.AllowedBorrowingCount}");
                    
                    if (remainingSlots == 0)
                    {
                        Console.WriteLine("Member has reached maximum borrowing limit!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ForceReturnBook(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("FORCE RETURN BOOK (ADMIN)");
            Console.WriteLine("========================");
            
            Console.Write("Enter Borrowing ID to force return: ");
            if (!int.TryParse(Console.ReadLine(), out int borrowingId) || borrowingId <= 0)
            {
                Console.WriteLine("Invalid Borrowing ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                Console.Write("Are you sure you want to force return this book? (y/n): ");
                string confirm = Console.ReadLine()?.ToLower() ?? "";
                
                if (confirm != "y" && confirm != "yes")
                {
                    Console.WriteLine("Operation cancelled.");
                    await WaitForUserInput();
                    return;
                }
                
                var returned = await borrowingService.ReturnBookAsync(borrowingId);
                
                Console.WriteLine($"Book force returned successfully!");
                Console.WriteLine($"Borrowing ID: {returned.Id}");
                if (returned.FineAmount > 0)
                {
                    Console.WriteLine($"Fine Amount: Rs.{returned.FineAmount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewBorrowingStatistics(IBorrowingService borrowingService)
        {
            Console.Clear();
            Console.WriteLine("BORROWING STATISTICS");
            Console.WriteLine("===================");
 
            try
            {
                var allBorrowings = await borrowingService.GetAllBorrowingsAsync();
                var activeBorrowings = await borrowingService.GetAllActiveBorrowingsAsync();
                var overdueBorrowings = await borrowingService.GetOverdueBorrowingsAsync();
                
                var returnedBorrowings = allBorrowings.Where(b => b.Status == BookBorrowStatus.Returned).ToList();
                
                double avgDuration = 0;
                if (returnedBorrowings.Any())
                {
                    avgDuration = returnedBorrowings
                        .Where(b => b.MemberReturnedDate.HasValue)
                        .Average(b => (b.MemberReturnedDate.Value - b.BorrowedDate).TotalDays);
                }
                
                var mostBorrowedBooks = allBorrowings
                    .GroupBy(b => b.BookId)
                    .Select(g => new { BookId = g.Key, Count = g.Count(), Book = g.First().Book })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();
                
                var mostActiveMembers = allBorrowings
                    .GroupBy(b => b.MemberId)
                    .Select(g => new { MemberId = g.Key, Count = g.Count(), Member = g.First().Member })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();
                
                var monthlyData = new Dictionary<string, int>();
                for (int i = 5; i >= 0; i--)
                {
                    var month = DateTime.UtcNow.AddMonths(-i);
                    var monthName = month.ToString("MMM yyyy");
                    var count = allBorrowings.Count(b => b.BorrowedDate.Year == month.Year && b.BorrowedDate.Month == month.Month);
                    monthlyData[monthName] = count;
                }
                
                var totalFines = allBorrowings.Sum(b => b.FineAmount);
                
                Console.WriteLine();
                Console.WriteLine("OVERALL STATISTICS");
                Console.WriteLine($"Total Borrowings: {allBorrowings.Count}");
                Console.WriteLine($"Active Borrowings: {activeBorrowings.Count}");
                Console.WriteLine($"Completed Borrowings: {returnedBorrowings.Count}");
                Console.WriteLine($"Overdue Borrowings: {overdueBorrowings.Count}");
                Console.WriteLine($"Average Borrowing Duration: {avgDuration:F1} days");
                Console.WriteLine($"Total Fines Collected: Rs.{totalFines}");
                
                Console.WriteLine();
                Console.WriteLine("TOP 5 MOST BORROWED BOOKS");
                if (mostBorrowedBooks.Any())
                {
                    int rank = 1;
                    foreach (var book in mostBorrowedBooks)
                    {
                        Console.WriteLine($"{rank}. {book.Book?.Title} - {book.Count} times");
                        rank++;
                    }
                }
                else
                {
                    Console.WriteLine("No data available");
                }
                
                Console.WriteLine();
                Console.WriteLine("TOP 5 MOST ACTIVE MEMBERS");
                if (mostActiveMembers.Any())
                {
                    int rank = 1;
                    foreach (var member in mostActiveMembers)
                    {
                        Console.WriteLine($"{rank}. {member.Member?.Name} - {member.Count} borrowings");
                        rank++;
                    }
                }
                else
                {
                    Console.WriteLine("No data available");
                }
                
                Console.WriteLine();
                Console.WriteLine("MONTHLY BORROWING TREND (Last 6 Months)");
                foreach (var month in monthlyData)
                {
                    int barLength = month.Value / 2;
                    string bar = new string('=', Math.Min(barLength, 50));
                    Console.WriteLine($"{month.Key,-12} : {bar} ({month.Value})");
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