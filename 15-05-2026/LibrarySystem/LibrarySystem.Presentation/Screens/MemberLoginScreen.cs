using LibrarySystem.Business.Services;
using LibrarySystem.Presentation.Helpers;

namespace LibrarySystem.Presentation.Screens
{
    public static class MemberLoginScreen
    {
        public static async Task<(bool Success, int MemberId, string Email)> ShowAndValidate(IMemberService memberService)
        {
            Console.Clear();  
            Console.WriteLine("=== LIBRARY SYSTEM MEMBER LOGIN ===\n");

            Console.Write("Email Id: ");
            string email = Console.ReadLine() ?? "";

            Console.Write("Password: ");
            string password = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("\n Email and password are required!");
                await Task.Delay(1500);
                return (false, 0, "");
            }

            try
            {
                //  LoginWithDetailsAsync returns (bool Success, int MemberId)
                var result = await memberService.LoginWithDetailsAsync(email, password);

                //  Check result.Success directly (result is not nullable)
                if (result.Success)
                {
                    //  Get member name separately if needed
                    var member = await memberService.GetById(result.MemberId);
                    string memberName = member?.Name ?? "Member";
                    
                    // Store session
UserSession.Login(result.MemberId, email);

Console.WriteLine($"\n Login Successful! Welcome {memberName}!");
await Task.Delay(1500);

return (true, result.MemberId, email);
                }
                else
                {
                    Console.WriteLine("\n Login Failed! Invalid email or password.");
                    await Task.Delay(1500);
                    return (false, 0, "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Error during login: {ex.Message}");
                await Task.Delay(1500);
                return (false, 0, "");
            }
        }
    }
}