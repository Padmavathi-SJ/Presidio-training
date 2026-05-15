using Microsoft.EntityFrameworkCore;

namespace DbFirst
{
    internal class Program
    {
        readonly TestDbContext _context;  // Changed to TestDbContext
        
        Program()
        {
            _context = new TestDbContext();
        }
        
        void AddAccountUsingSP()
        {
            Account account = new Account() { Aacno = 5, Balance = 1233.3f };
            // call add_account(4,3243);
            _context.Database.ExecuteSqlInterpolated($"call add_account({account.Aacno},{account.Balance});");
            Console.WriteLine("Account Created");
        }
        
        static void Main(string[] args)
        {
            new Program().AddAccountUsingSP();
        }
    }
}