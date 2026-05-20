using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Presentation.Menus
{
    public static class BorrowingRulesManagementMenu
    {
        public static async Task Show(IBorrowingRulesService rulesService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("        BORROWING RULES MANAGEMENT      ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. View All Borrowing Rules");
                Console.WriteLine("2. View Rules by Membership Type");
                Console.WriteLine("3. Add New Borrowing Rules");
                Console.WriteLine("4. Update Borrowing Rules");
                Console.WriteLine("5. Delete Borrowing Rules");
                Console.WriteLine("0. Back to Admin Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (0-5): ");
                
                string choice = Console.ReadLine() ?? "0";

                switch (choice)
                {
                    case "1":
                        await ViewAllRules(rulesService);
                        break;
                    case "2":
                        await ViewRulesByType(rulesService);
                        break;
                    case "3":
                        await AddNewRules(rulesService);
                        break;
                    case "4":
                        await UpdateRules(rulesService);
                        break;
                    case "5":
                        await DeleteRules(rulesService);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select 0-5.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task ViewAllRules(IBorrowingRulesService rulesService)
        {
            Console.Clear();
            Console.WriteLine("ALL BORROWING RULES");
            Console.WriteLine("===================");
            
            try
            {
                var rules = await rulesService.GetAllRulesAsync();
                
                if (rules == null || !rules.Any())
                {
                    Console.WriteLine("No borrowing rules found in the system.");
                    Console.WriteLine("You can add new rules using option 3.");
                }
                else
                {
                    Console.WriteLine($"Total Rules Configured: {rules.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Membership Type",-20} {"Max Books",-15} {"Max Days",-12} {"Created Date"}");
                    Console.WriteLine(new string('-', 75));
                    
                    foreach (var rule in rules)
                    {
                        Console.WriteLine($"{rule.Id,-5} {rule.MembershipType,-20} {rule.MaxActiveBorrowings,-15} {rule.MaxBorrowDays,-12} {rule.CreatedAt:yyyy-MM-dd}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    Console.WriteLine("No borrowing rules found in the system.");
                    Console.WriteLine("You can add new rules using option 3.");
                }
                else
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewRulesByType(IBorrowingRulesService rulesService)
        {
            Console.Clear();
            Console.WriteLine("RULES BY MEMBERSHIP TYPE");
            Console.WriteLine("=======================");
            
            Console.WriteLine("Select Membership Type:");
            Console.WriteLine("1. Basic");
            Console.WriteLine("2. Student");
            Console.WriteLine("3. Premium");
            Console.WriteLine("0. Cancel");
            
            Console.Write("Enter choice (1-3): ");
            string input = Console.ReadLine() ?? "0";
            
            MembershipType type;
            switch (input)
            {
                case "1":
                    type = MembershipType.Basic;
                    break;
                case "2":
                    type = MembershipType.Student;
                    break;
                case "3":
                    type = MembershipType.Premium;
                    break;
                default:
                    Console.WriteLine("Operation cancelled.");
                    await WaitForUserInput();
                    return;
            }
            
            try
            {
                var rule = await rulesService.GetRulesByMembershipTypeAsync(type);
                
                Console.WriteLine($"Borrowing Rules for {type} Members:");
                Console.WriteLine($"Rule ID: {rule.Id}");
                Console.WriteLine($"Maximum Books: {rule.MaxActiveBorrowings}");
                Console.WriteLine($"Maximum Borrow Days: {rule.MaxBorrowDays}");
                Console.WriteLine($"Created: {rule.CreatedAt:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"Last Updated: {rule.UpdatedAt:yyyy-MM-dd HH:mm}");
                
                Console.WriteLine("Policy Details:");
                Console.WriteLine($"  Can borrow up to {rule.MaxActiveBorrowings} book(s) simultaneously");
                Console.WriteLine($"  Must return within {rule.MaxBorrowDays} days");
                Console.WriteLine($"  Late fee: Rs.10 per day after due date");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task AddNewRules(IBorrowingRulesService rulesService)
        {
            Console.Clear();
            Console.WriteLine("ADD NEW BORROWING RULES");
            Console.WriteLine("======================");
            
            try
            {
                Console.WriteLine("Select Membership Type for New Rules:");
                Console.WriteLine("1. Basic");
                Console.WriteLine("2. Student");
                Console.WriteLine("3. Premium");
                
                Console.Write("Enter choice (1-3): ");
                string input = Console.ReadLine() ?? "1";
                
                MembershipType type;
                switch (input)
                {
                    case "1":
                        type = MembershipType.Basic;
                        break;
                    case "2":
                        type = MembershipType.Student;
                        break;
                    case "3":
                        type = MembershipType.Premium;
                        break;
                    default:
                        Console.WriteLine("Invalid choice!");
                        await WaitForUserInput();
                        return;
                }
                
                try
                {
                    var existing = await rulesService.GetRulesByMembershipTypeAsync(type);
                    if (existing != null)
                    {
                        Console.WriteLine($"Rules for {type} already exist!");
                        Console.WriteLine($"  ID: {existing.Id}");
                        Console.WriteLine($"  Max Books: {existing.MaxActiveBorrowings}");
                        Console.WriteLine($"  Max Days: {existing.MaxBorrowDays}");
                        Console.WriteLine("Please use Update option instead.");
                        await WaitForUserInput();
                        return;
                    }
                }
                catch
                {
                    // Rules don't exist, continue with creation
                }
                
                Console.Write("Enter Maximum Books Allowed: ");
                if (!int.TryParse(Console.ReadLine(), out int maxBooks) || maxBooks <= 0)
                {
                    Console.WriteLine("Invalid input. Max books must be greater than 0.");
                    await WaitForUserInput();
                    return;
                }
                
                Console.Write("Enter Maximum Borrow Days: ");
                if (!int.TryParse(Console.ReadLine(), out int maxDays) || maxDays <= 0)
                {
                    Console.WriteLine("Invalid input. Max days must be greater than 0.");
                    await WaitForUserInput();
                    return;
                }
                
                var newRules = new BorrowingRules
                {
                    MembershipType = type,
                    MaxActiveBorrowings = maxBooks,
                    MaxBorrowDays = maxDays,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                var result = await rulesService.AddRulesAsync(newRules);
                
                Console.WriteLine($"Borrowing rules for {type} added successfully!");
                Console.WriteLine($"  Rule ID: {result.Id}");
                Console.WriteLine($"  Max Books: {result.MaxActiveBorrowings}");
                Console.WriteLine($"  Max Days: {result.MaxBorrowDays}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task UpdateRules(IBorrowingRulesService rulesService)
        {
            Console.Clear();
            Console.WriteLine("UPDATE BORROWING RULES");
            Console.WriteLine("=====================");
            
            try
            {
                var allRules = await rulesService.GetAllRulesAsync();
                
                Console.WriteLine("Existing Borrowing Rules:");
                Console.WriteLine();
                Console.WriteLine($"{"ID",-5} {"Membership Type",-20} {"Max Books",-15} {"Max Days"}");
                Console.WriteLine(new string('-', 55));
                
                foreach (var r in allRules)
                {
                    Console.WriteLine($"{r.Id,-5} {r.MembershipType,-20} {r.MaxActiveBorrowings,-15} {r.MaxBorrowDays}");
                }
                
                Console.Write("Enter Rule ID to update: ");
                if (!int.TryParse(Console.ReadLine(), out int ruleId) || ruleId <= 0)
                {
                    Console.WriteLine("Invalid Rule ID.");
                    await WaitForUserInput();
                    return;
                }
                
                var existingRule = await rulesService.GetRulesByIdAsync(ruleId);
                
                Console.WriteLine($"Current Rules for {existingRule.MembershipType}:");
                Console.WriteLine($"  Current Max Books: {existingRule.MaxActiveBorrowings}");
                Console.WriteLine($"  Current Max Days: {existingRule.MaxBorrowDays}");
                
                Console.Write("Enter New Max Books (press Enter to keep current): ");
                string booksInput = Console.ReadLine();
                int newMaxBooks = string.IsNullOrWhiteSpace(booksInput) 
                    ? existingRule.MaxActiveBorrowings 
                    : int.Parse(booksInput);
                
                Console.Write("Enter New Max Days (press Enter to keep current): ");
                string daysInput = Console.ReadLine();
                int newMaxDays = string.IsNullOrWhiteSpace(daysInput) 
                    ? existingRule.MaxBorrowDays 
                    : int.Parse(daysInput);
                
                if (newMaxBooks <= 0 || newMaxDays <= 0)
                {
                    Console.WriteLine("Values must be greater than 0.");
                    await WaitForUserInput();
                    return;
                }
                
                var updatedRule = new BorrowingRules
                {
                    MembershipType = existingRule.MembershipType,
                    MaxActiveBorrowings = newMaxBooks,
                    MaxBorrowDays = newMaxDays
                };
                
                var result = await rulesService.UpdateRulesAsync(ruleId, updatedRule);
                
                Console.WriteLine($"Borrowing rules for {result.MembershipType} updated successfully!");
                Console.WriteLine($"  New Max Books: {result.MaxActiveBorrowings}");
                Console.WriteLine($"  New Max Days: {result.MaxBorrowDays}");
                Console.WriteLine("Note: These changes will affect all new borrowings by existing members.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task DeleteRules(IBorrowingRulesService rulesService)
        {
            Console.Clear();
            Console.WriteLine("DELETE BORROWING RULES");
            Console.WriteLine("====================");
            
            try
            {
                var allRules = await rulesService.GetAllRulesAsync();
                
                Console.WriteLine("Existing Borrowing Rules:");
                Console.WriteLine();
                Console.WriteLine($"{"ID",-5} {"Membership Type",-20} {"Max Books",-15} {"Max Days"}");
                Console.WriteLine(new string('-', 55));
                
                foreach (var r in allRules)
                {
                    Console.WriteLine($"{r.Id,-5} {r.MembershipType,-20} {r.MaxActiveBorrowings,-15} {r.MaxBorrowDays}");
                }
                
                Console.Write("Enter Rule ID to delete: ");
                if (!int.TryParse(Console.ReadLine(), out int ruleId) || ruleId <= 0)
                {
                    Console.WriteLine("Invalid Rule ID.");
                    await WaitForUserInput();
                    return;
                }
                
                var ruleToDelete = await rulesService.GetRulesByIdAsync(ruleId);
                
                if (ruleToDelete == null)
                {
                    Console.WriteLine($"Rule with ID {ruleId} not found.");
                    await WaitForUserInput();
                    return;
                }
                
                Console.WriteLine($"Warning: You are about to delete rules for {ruleToDelete.MembershipType}");
                Console.WriteLine($"  Max Books: {ruleToDelete.MaxActiveBorrowings}");
                Console.WriteLine($"  Max Days: {ruleToDelete.MaxBorrowDays}");
                Console.Write("Are you sure you want to delete these rules? (y/n): ");
                
                string confirm = Console.ReadLine()?.ToLower() ?? "";
                
                if (confirm != "y" && confirm != "yes")
                {
                    Console.WriteLine("Operation cancelled.");
                    await WaitForUserInput();
                    return;
                }
                
                var result = await rulesService.DeleteRulesAsync(ruleId);
                
                if (result)
                {
                    Console.WriteLine($"Borrowing rules for {ruleToDelete.MembershipType} deleted successfully!");
                    Console.WriteLine("Note: System will use default values for this membership type:");
                    Console.WriteLine($"  Max Books: {(ruleToDelete.MembershipType == MembershipType.Basic ? 2 : ruleToDelete.MembershipType == MembershipType.Student ? 3 : 5)}");
                    Console.WriteLine($"  Max Days: {(ruleToDelete.MembershipType == MembershipType.Basic ? 7 : ruleToDelete.MembershipType == MembershipType.Student ? 10 : 15)}");
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