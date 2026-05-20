using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Presentation.Screens
{
    public static class MemberRegistrationScreen
    {
        public static async Task Show(IMemberService memberService, IBorrowingRulesService borrowingRulesService)
        {
            Console.Clear();
           
            Console.WriteLine(" NEW MEMBER REGISTRATION ");
            
            
            try
            {
                // Collect member information
                Console.Write("Full Name: ");
                string name = Console.ReadLine() ?? "";
                
                Console.Write("Email Address: ");
                string email = Console.ReadLine() ?? "";
                
                Console.Write("Phone Number: ");
                string phone = Console.ReadLine() ?? "";
                
                Console.Write("Password: ");
                string password = Console.ReadLine() ?? "";
                
                Console.Write("Confirm Password: ");
                string confirmPassword = Console.ReadLine() ?? "";
                
                // Check if passwords match
                if (password != confirmPassword)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n Passwords do not match. Registration failed.");
                    Console.ResetColor();
                    await WaitForUserInput();
                    return;
                }
                
                // Get membership limits
                var basicLimit = await borrowingRulesService.GetMaxBorrowingsAsync(MembershipType.Basic);
                var studentLimit = await borrowingRulesService.GetMaxBorrowingsAsync(MembershipType.Student);
                var premiumLimit = await borrowingRulesService.GetMaxBorrowingsAsync(MembershipType.Premium);
                
                // Membership type selection
                Console.WriteLine("\nSelect Membership Type:");
                Console.WriteLine($"1. Basic - Can borrow up to {basicLimit} books");
                Console.WriteLine($"2. Premium - Can borrow up to {premiumLimit} books");
                Console.WriteLine($"3. Student - Can borrow up to {studentLimit} books");
                Console.Write("\nEnter choice (1-3): ");
                
                string typeInput = Console.ReadLine() ?? "1";
                MembershipType membershipType = typeInput switch
                {
                    "1" => MembershipType.Basic,
                    "2" => MembershipType.Premium,
                    "3" => MembershipType.Student,
                    _ => MembershipType.Basic
                };
                
                int allowedBorrowings = membershipType switch
                {
                    MembershipType.Basic => basicLimit,
                    MembershipType.Premium => premiumLimit,
                    MembershipType.Student => studentLimit,
                    _ => basicLimit
                };
                
                // Create member object
                var member = new Member
                {
                    Name = name,
                    Email = email,
                    PhoneNum = phone,
                    Password = password,
                    MembershipType = membershipType,
                    MembershipStatus = MembershipStatus.Active,
                    IsActive = true,
                    AllowedBorrowingCount = allowedBorrowings,
                    CurrentBorrowedCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Register member
                var result = await memberService.AddMemberAsync(member);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n Registration Successful!");
                
                Console.WriteLine($"Welcome, {result.Name}!");
                Console.WriteLine($"Your Member ID is: {result.Id}");
                Console.WriteLine($"Membership Type: {result.MembershipType}");
                Console.WriteLine($"Allowed Borrowings: {result.AllowedBorrowingCount} books");
                Console.WriteLine($"Status: {result.MembershipStatus}");
                Console.WriteLine("\nYou can now login using your email and password.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n Registration Failed: {ex.Message}");
                Console.ResetColor();
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