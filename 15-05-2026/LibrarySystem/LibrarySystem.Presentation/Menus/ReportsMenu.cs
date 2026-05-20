using LibrarySystem.Business.Services;

namespace LibrarySystem.Presentation.Menus
{
    public static class ReportsMenu
    {
        public static async Task Show(IReportService reportService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("              REPORTS MENU              ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Dashboard Summary");
                Console.WriteLine("2. Book Reports");
                Console.WriteLine("3. Member Reports");
                Console.WriteLine("4. Borrowing Reports");
                Console.WriteLine("5. Fine Reports");
                Console.WriteLine("6. Category Reports");
                Console.WriteLine("0. Back to Admin Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (0-6): ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await ShowDashboardSummary(reportService);
                        break;
                    case 2:
                        await ShowBookReports(reportService);
                        break;
                    case 3:
                        await ShowMemberReports(reportService);
                        break;
                    case 4:
                        await ShowBorrowingReports(reportService);
                        break;
                    case 5:
                        await ShowFineReports(reportService);
                        break;
                    case 6:
                        await ShowCategoryReports(reportService);
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select 0-6.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task ShowDashboardSummary(IReportService reportService)
        {
            Console.Clear();
            Console.WriteLine("DASHBOARD SUMMARY");
            Console.WriteLine("================");
            
            try
            {
                var summary = await reportService.GetDashboardSummaryAsync();
                
                Console.WriteLine();
                Console.WriteLine("BOOK STATISTICS");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine($"Total Books: {summary.TotalBooks}");
                Console.WriteLine($"Available Books: {summary.AvailableBooks}");
                Console.WriteLine($"Borrowed Books: {summary.BorrowedBooks}");
                Console.WriteLine($"Damaged Books: {summary.DamagedBooks}");
                
                Console.WriteLine();
                Console.WriteLine("MEMBER STATISTICS");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine($"Total Members: {summary.TotalMembers}");
                Console.WriteLine($"Active Members: {summary.ActiveMembers}");
                Console.WriteLine($"Inactive Members: {summary.TotalMembers - summary.ActiveMembers}");
                
                Console.WriteLine();
                Console.WriteLine("BORROWING STATISTICS");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine($"Active Borrowings: {summary.ActiveBorrowings}");
                Console.WriteLine($"Overdue Borrowings: {summary.OverdueBorrowings}");
                
                Console.WriteLine();
                Console.WriteLine("FINE STATISTICS");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine($"Total Fines Collected: Rs.{summary.TotalFinesCollected}");
                Console.WriteLine($"Pending Fines: Rs.{summary.PendingFines}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ShowBookReports(IReportService reportService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("BOOK REPORTS");
                Console.WriteLine("===========");
                Console.WriteLine("1. Total Books Count");
                Console.WriteLine("2. Available Books Count");
                Console.WriteLine("3. Borrowed Books Count");
                Console.WriteLine("4. Damaged Books Count");
                Console.WriteLine("0. Back");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (0-4): ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input!");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        var total = await reportService.GetTotalBooksCountAsync();
                        Console.WriteLine($"Total Books: {total}");
                        break;
                    case 2:
                        var available = await reportService.GetTotalAvailableBooksCountAsync();
                        Console.WriteLine($"Available Books: {available}");
                        break;
                    case 3:
                        var borrowed = await reportService.GetTotalBorrowedBooksCountAsync();
                        Console.WriteLine($"Borrowed Books: {borrowed}");
                        break;
                    case 4:
                        var damaged = await reportService.GetTotalDamagedBooksCountAsync();
                        Console.WriteLine($"Damaged Books: {damaged}");
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice!");
                        break;
                }
                
                if (choice >= 1 && choice <= 4)
                    await WaitForUserInput();
            }
        }

        private static async Task ShowMemberReports(IReportService reportService)
        {
            Console.Clear();
            Console.WriteLine("MEMBER REPORTS");
            Console.WriteLine("=============");
            
            try
            {
                var total = await reportService.GetTotalMembersCountAsync();
                var active = await reportService.GetActiveMembersCountAsync();
                var inactive = await reportService.GetInactiveMembersCountAsync();
                var byType = await reportService.GetMembersByMembershipTypeAsync();
                
                Console.WriteLine();
                Console.WriteLine($"Total Members: {total}");
                Console.WriteLine($"Active Members: {active}");
                Console.WriteLine($"Inactive Members: {inactive}");
                
                Console.WriteLine();
                Console.WriteLine("Members by Membership Type:");
                Console.WriteLine("-------------------------------------");
                foreach (var type in byType)
                {
                    Console.WriteLine($"{type.Key}: {type.Value} members");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ShowBorrowingReports(IReportService reportService)
        {
            Console.Clear();
            Console.WriteLine("BORROWING REPORTS");
            Console.WriteLine("================");
            
            try
            {
                var total = await reportService.GetTotalBorrowingsCountAsync();
                var active = await reportService.GetActiveBorrowingsCountAsync();
                var completed = await reportService.GetCompletedBorrowingsCountAsync();
                var overdue = await reportService.GetOverdueBorrowingsCountAsync();
                var overdueList = await reportService.GetOverdueBorrowingsWithDetailsAsync();
                
                Console.WriteLine();
                Console.WriteLine($"Total Borrowings: {total}");
                Console.WriteLine($"Active Borrowings: {active}");
                Console.WriteLine($"Completed Borrowings: {completed}");
                Console.WriteLine($"Overdue Borrowings: {overdue}");
                
                if (overdueList.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("OVERDUE BORROWINGS DETAILS:");
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine($"{"Member",-25} {"Book",-30} {"Due Date",-15} {"Days Overdue"}");
                    Console.WriteLine(new string('-', 85));
                    
                    foreach (var borrowing in overdueList)
                    {
                        int daysOverdue = (DateTime.UtcNow - borrowing.DueDate).Days;
                        string memberName = borrowing.Member?.Name ?? "N/A";
                        string bookTitle = borrowing.Book?.Title ?? "N/A";
                        Console.WriteLine($"{memberName,-25} {bookTitle,-30} {borrowing.DueDate:yyyy-MM-dd,-15} {daysOverdue}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ShowFineReports(IReportService reportService)
        {
            Console.Clear();
            Console.WriteLine("FINE REPORTS");
            Console.WriteLine("===========");
            
            try
            {
                var collected = await reportService.GetTotalFineCollectedAsync();
                var pending = await reportService.GetTotalPendingFineAmountAsync();
                
                Console.WriteLine();
                Console.WriteLine($"Total Fines Collected: Rs.{collected}");
                Console.WriteLine($"Pending Fines: Rs.{pending}");
                
                if (pending > 500)
                {
                    Console.WriteLine("Warning: High amount of pending fines!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ShowCategoryReports(IReportService reportService)
        {
            Console.Clear();
            Console.WriteLine("CATEGORY REPORTS");
            Console.WriteLine("===============");
            
            try
            {
                var categories = await reportService.GetCategoriesWithBookCountAsync();
                
                if (categories == null || !categories.Any())
                {
                    Console.WriteLine("No categories found.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Books by Category:");
                    Console.WriteLine($"{"Category",-30} {"Number of Books"}");
                    Console.WriteLine(new string('-', 50));
                    
                    foreach (var category in categories)
                    {
                        int bookCount = category.Books?.Count ?? 0;
                        Console.WriteLine($"{category.CategoryName,-30} {bookCount}");
                    }
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