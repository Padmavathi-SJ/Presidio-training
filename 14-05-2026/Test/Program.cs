using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test
{
    internal class Program
    {
        readonly TestDbContext _context;
        
        Program()
        {
            _context = new TestDbContext();
        }

        void TransactWithTransactioninDatabase()
        {
            int fromAccountNo = 4;
            int toAccountNo = 5;
            float amount = 1000;
            int tran_id = 9;
            
            // Use nullable types
            Account? fc = _context.Accounts.FirstOrDefault(a => a.Aacno == fromAccountNo);
            Account? tc = _context.Accounts.FirstOrDefault(a => a.Aacno == toAccountNo);
            
            // Null checks
            if (fc == null)
            {
                Console.WriteLine($"Error: Account {fromAccountNo} does not exist!");
                return;
            }
            
            if (tc == null)
            {
                Console.WriteLine($"Error: Account {toAccountNo} does not exist!");
                return;
            }
            
            // Check balance (using null-forgiving operator since we already checked null)
            if (fc.Balance < amount)
            {
                Console.WriteLine($"Insufficient balance in account {fromAccountNo}. Available: {fc.Balance}, Required: {amount}");
                return;
            }
            
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Database.ExecuteSqlInterpolated($"CALL add_trans({tran_id}, {fc.Aacno}, {tc.Aacno}, {amount})");
                _context.Database.ExecuteSqlInterpolated($"CALL update_account({fc.Aacno}, {fc.Balance - amount})");
                _context.Database.ExecuteSqlInterpolated($"CALL update_account({tc.Aacno}, {tc.Balance + amount})");
                
                transaction.Commit();
                Console.WriteLine($"✓ Transaction successful!");
                Console.WriteLine($"  Transferred: {amount:C}");
                Console.WriteLine($"  From Account {fromAccountNo}: {fc.Balance:C} → {fc.Balance - amount:C}");
                Console.WriteLine($"  To Account {toAccountNo}: {tc.Balance:C} → {tc.Balance + amount:C}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"✗ Transaction failed: {ex.Message}");
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("=== Bank Transaction System ===\n");
            new Program().TransactWithTransactioninDatabase();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}