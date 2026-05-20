using LibrarySystem.Business.Services;

namespace LibrarySystem.Presentation.Screens
{
    public static class LoginScreen
    {
        public static async Task<bool> ShowAndValidate(IAdminService adminService)
        {
            Console.WriteLine("=== LIBRARY SYSTEM ADMIN LOGIN ===\n");

                    Console.Write("Phone Number: ");
                string phoneNum = Console.ReadLine() ?? "";

                Console.Write("Password: ");
                string password = Console.ReadLine() ?? "";

                bool loginSuccess = await adminService.LoginAsync(phoneNum, password);

                if (loginSuccess)
                {
                     Console.WriteLine("\nLogin Successful!");
                     return true;
                }
                 else
                {
                    Console.WriteLine("\nLogin Failed!");
                    return false;
                }

        }
    }
}