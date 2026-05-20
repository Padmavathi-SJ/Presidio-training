using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LibrarySystem.DataAccess.Config;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Database;
using LibrarySystem.Business.Services;
using LibrarySystem.Presentation.DependencyInjection;
using LibrarySystem.Presentation.Menus;
using LibrarySystem.Presentation.Screens;
using DotNetEnv;

namespace LibrarySystem.Presentation
{
    class Program
    {   
        static async Task Main(string[] args)
        {
            Env.Load();

            var dbConfig = new DatabaseConfig
            {
                Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"),
                DatabaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "notification_system_ef",
                UserName = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""
            };

            var dbConnection = new DatabaseConnection(dbConfig);

            try
            {
                using var connection = dbConnection.GetConnection();
                Console.WriteLine("database connected successfully!");
            } 
            catch(Exception ex)
            {
                Console.WriteLine($"failed: {ex.Message}");
                return;
            }
            
            var services = ServiceRegistration.ConfigureServices(dbConfig.GetConnectionString());
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                Console.WriteLine("dbcontext initialized successfully!");

                var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
                var bookService = scope.ServiceProvider.GetRequiredService<IBookService>();
                var bookCategoryService = scope.ServiceProvider.GetRequiredService<IBookCategoryService>();
                var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();
                var borrowingRulesService = scope.ServiceProvider.GetRequiredService<IBorrowingRulesService>();
                var borrowingService = scope.ServiceProvider.GetRequiredService<IBorrowingService>();
                var bookCopyService = scope.ServiceProvider.GetRequiredService<IBookCopyService>();
                var fineService = scope.ServiceProvider.GetRequiredService<IFineService>();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                //  Pass context to MainEntryScreen
                await MainEntryScreen(adminService, bookService, bookCategoryService, memberService, 
                    borrowingRulesService, borrowingService, bookCopyService, fineService, reportService, context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task MainEntryScreen(
            IAdminService adminService, 
            IBookService bookService, 
            IBookCategoryService bookCategoryService, 
            IMemberService memberService,
            IBorrowingRulesService borrowingRulesService,
            IBorrowingService borrowingService,
            IBookCopyService bookCopyService,
            IFineService fineService,
            IReportService reportService,
            ApplicationDbContext context)  
        {
            while (true)
            {
                Console.Clear();
             
                Console.WriteLine(" WELCOME TO LIBRARY SYSTEM ");
            
                Console.WriteLine("\nSelect Your Role:");
                Console.WriteLine("1. Administrator");
                Console.WriteLine("2. Member");
                Console.WriteLine("3. Exit");
                Console.Write("\nEnter your choice (1-3): ");
                
                string choice = Console.ReadLine() ?? "0";

                switch (choice)
                {
                    case "1":
                        await HandleAdminLogin(adminService, bookService, bookCategoryService, memberService, 
                            borrowingRulesService, borrowingService, bookCopyService, fineService, reportService);
                        break;
                    case "2":
                        await HandleMemberSection(memberService, bookService, borrowingService, borrowingRulesService, 
                            fineService, context);  
                        break;
                    case "3":
                        Console.WriteLine("\nThank you for using Library System. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("\n Invalid choice! Please select 1, 2, or 3.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        static async Task HandleAdminLogin(
            IAdminService adminService, 
            IBookService bookService, 
            IBookCategoryService bookCategoryService, 
            IMemberService memberService,
            IBorrowingRulesService borrowingRulesService,
            IBorrowingService borrowingService,
            IBookCopyService bookCopyService,
            IFineService fineService,
            IReportService reportService
            )
        {
            Console.Clear();
            bool loginSuccess = false;

            while (!loginSuccess)
            {
                loginSuccess = await LoginScreen.ShowAndValidate(adminService);
                if (!loginSuccess)
                {
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey();
                    Console.Clear(); 
                }
            }
            
            if (loginSuccess)
            {
                Console.Clear(); 
                Console.WriteLine("Login Successful! Redirecting to Admin Panel...\n");
                await Task.Delay(1500);
                await AdminMenu.Show(bookService, bookCategoryService, memberService, borrowingRulesService, 
                    bookCopyService, fineService, reportService, borrowingService);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n Failed login attempt. Returning to admin section...");
                Console.ResetColor();
            }
        }

        static async Task HandleMemberSection(
            IMemberService memberService, 
            IBookService bookService,
            IBorrowingService borrowingService,
            IBorrowingRulesService borrowingRulesService,
            IFineService fineService,
            ApplicationDbContext context) 
        {
            while (true)
            {
                Console.Clear();
              
                Console.WriteLine(" MEMBER SECTION ");
               
                Console.WriteLine("\n1. New Member Registration");
                Console.WriteLine("2. Existing Member Login");
                Console.WriteLine("3. Back to Main Menu");
                Console.Write("\nEnter your choice (1-3): ");
                
                string choice = Console.ReadLine() ?? "0";

                switch (choice)
                {
                    case "1":
                        await MemberRegistrationScreen.Show(memberService, borrowingRulesService);
                        break;
                    case "2":
                        await HandleMemberLogin(memberService, bookService, borrowingService, fineService, context);  // ✅ Pass context
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("\n Invalid choice! Please select 1, 2, or 3.");
                        await WaitForUserInput();
                        break;
                }
            }
        }
        
        static async Task HandleMemberLogin(
            IMemberService memberService, 
            IBookService bookService,
            IBorrowingService borrowingService,
            IFineService fineService,
            ApplicationDbContext context)  
        {
            Console.Clear();
            bool loginSuccess = false;
            int memberId = 0;

            while (!loginSuccess)
            {
                var result = await MemberLoginScreen.ShowAndValidate(memberService);
                loginSuccess = result.Success;
                memberId = result.MemberId;

                if (!loginSuccess)
                {
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey();
                    Console.Clear(); 
                }
            }
            
            if (loginSuccess)
            {
                Console.Clear(); 
                Console.WriteLine("Login Successful! Redirecting to Member Panel...\n");
                await Task.Delay(1500);
                
              //  BorrowingMenu.SetCurrentMember(memberId);
              //  MemberFineManagementMenu.SetCurrentMember(memberId);
                
                
                await MemberMenu.Show(bookService, memberService, borrowingService, fineService, context);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n Failed login attempt. Returning to member section...");
                Console.ResetColor();
            }
        }

        static async Task WaitForUserInput()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}