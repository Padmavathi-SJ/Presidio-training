using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnderstandingOOPSApp.Models;
using UnderstandingOOPSApp.Repositories;
using UnderstandingOOPSApp.Interfaces;

namespace UnderstandingOOPSApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            

            // Create repository instance
            AccountRepository accountRepo = new AccountRepository();

            // Sample Account 1 - Saving Account
            Account account1 = new Account
            {
                NameOnAccount = "John Doe",
                DateOfBirth = new DateTime(1990, 5, 15),
                Email = "john.doe@email.com",
                Phone = "9876543210",
                Balance = 5000.50f,
                AccountType = AccType.SavingAccount
            };

            // Sample Account 2 - Current Account
            Account account2 = new Account
            {
                NameOnAccount = "Jane Smith",
                DateOfBirth = new DateTime(1985, 8, 22),
                Email = "jane.smith@email.com",
                Phone = "9876543211",
                Balance = 15000.75f,
                AccountType = AccType.CurrentAccount
            };

            // Sample Account 3 - Saving Account
            Account account3 = new Account
            {
                NameOnAccount = "Bob Johnson",
                DateOfBirth = new DateTime(1995, 12, 10),
                Email = "bob.johnson@email.com",
                Phone = "9876543212",
                Balance = 7500.00f,
                AccountType = AccType.SavingAccount
            };

            // CREATE Accounts
            Console.WriteLine("--- Creating Accounts ---");
            var createdAccount1 = accountRepo.Create(account1);
            var createdAccount2 = accountRepo.Create(account2);
            var createdAccount3 = accountRepo.Create(account3);

            Console.WriteLine($"✓ Account created for: {createdAccount1.NameOnAccount}");
            Console.WriteLine($"✓ Account created for: {createdAccount2.NameOnAccount}");
            Console.WriteLine($"✓ Account created for: {createdAccount3.NameOnAccount}");
            Console.WriteLine();

            // GET all Accounts
            Console.WriteLine("--- All Accounts ---");
            var allAccounts = accountRepo.GetAccounts();
            if (allAccounts != null)
            {
                foreach (var account in allAccounts)
                {
                    Console.WriteLine(account);
                    Console.WriteLine("------------------------");
                }
            }
            Console.WriteLine();

            // GET specific Account by Account Number
            Console.WriteLine("--- Get Specific Account ---");
            string searchAccountNumber = createdAccount1.AccountNumber;
            var retrievedAccount = accountRepo.GetAccount(searchAccountNumber);
            if (retrievedAccount != null)
            {
                Console.WriteLine($"Found account: {retrievedAccount.NameOnAccount}");
                Console.WriteLine(retrievedAccount);
            }
            Console.WriteLine();

            // UPDATE an Account
            Console.WriteLine("--- Update Account ---");
            if (retrievedAccount != null)
            {
                retrievedAccount.Balance = 8500.75f;
                retrievedAccount.Phone = "9999999999";
                var updatedAccount = accountRepo.Update(searchAccountNumber, retrievedAccount);
                Console.WriteLine($"✓ Account updated for: {updatedAccount.NameOnAccount}");
                Console.WriteLine($"New Balance: ${updatedAccount.Balance}");
                Console.WriteLine($"New Phone: {updatedAccount.Phone}");
            }
            Console.WriteLine();

            // DELETE an Account
            Console.WriteLine("--- Delete Account ---");
            string accountToDelete = createdAccount3.AccountNumber;
            var deletedAccount = accountRepo.Delete(accountToDelete);
            if (deletedAccount != null)
            {
                Console.WriteLine($"✓ Account deleted for: {deletedAccount.NameOnAccount}");
            }
            Console.WriteLine();

            // Show remaining accounts after deletion
            Console.WriteLine("--- Remaining Accounts After Deletion ---");
            var remainingAccounts = accountRepo.GetAccounts();
            if (remainingAccounts != null)
            {
                foreach (var account in remainingAccounts)
                {
                    Console.WriteLine($"Account: {account.AccountNumber} - {account.NameOnAccount} - Balance: ${account.Balance}");
                }
            }
            Console.WriteLine();

            // Try to get a deleted account (should return null)
            Console.WriteLine("--- Try to Get Deleted Account ---");
            var deletedAccountCheck = accountRepo.GetAccount(accountToDelete);
            if (deletedAccountCheck == null)
            {
                Console.WriteLine($"✓ Account {accountToDelete} no longer exists in the system");
            }

            Console.WriteLine("\n======= Press any key to exit =======");
            Console.ReadKey();
        }
    }
}