using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Presentation.Menus
{
    public static class AdminMenu
    {
        public static async Task Show(
            IBookService bookService, 
            IBookCategoryService bookCategoryService, 
            IMemberService memberService, 
            IBorrowingRulesService borrowingRulesService,
            IBookCopyService bookCopyService,
            IFineService fineService,
            IReportService reportService,
            IBorrowingService borrowingService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("              ADMIN MENU               ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Book Category Management");
                Console.WriteLine("2. Book Management");
                Console.WriteLine("3. Member Management");
                Console.WriteLine("4. Book Borrowings");
                Console.WriteLine("5. Borrowing Rules Management");
                Console.WriteLine("6. Fine Management");
                Console.WriteLine("7. Reports");
                Console.WriteLine("8. Exit");
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
                        await BookCategoryManagementMenu.Show(bookCategoryService);
                        break;
                    case 2:
                        await BookManagementMenu.Show(bookCopyService, bookService);
                        break;
                    case 3:
                        await MemberManagementMenu.Show(memberService, borrowingRulesService);
                        break;
                    case 4:
                        await AdminBorrowingMenu.Show(borrowingService, memberService, bookService);
                        break;
                    case 5:
                        await BorrowingRulesManagementMenu.Show(borrowingRulesService);
                        break;
                    case 6:
                        await AdminFineManagementMenu.Show(fineService, memberService);
                        break;
                    case 7:
                        await ReportsMenu.Show(reportService);
                        break;
                    case 8:
                        Console.WriteLine("Returning to Main Menu...");
                        await Task.Delay(1500);
                        return;
                    default:
                        Console.WriteLine("Invalid Choice!");
                        await WaitForUserInput();
                        break;
                }
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