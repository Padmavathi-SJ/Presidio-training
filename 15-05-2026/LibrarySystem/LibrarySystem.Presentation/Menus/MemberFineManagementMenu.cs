using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.Presentation.Helpers;

namespace LibrarySystem.Presentation.Menus
{
    public static class MemberFineManagementMenu
    {
        
        public static async Task Show(IFineService fineService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("              FINE MANAGEMENT           ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. View My Unpaid Fines");
                Console.WriteLine("2. View My Paid Fines");
                Console.WriteLine("3. View All My Fines");
                Console.WriteLine("4. Pay a Fine");
                Console.WriteLine("5. Check Total Unpaid Amount");
                Console.WriteLine("0. Back to Main Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (0-5): ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await ViewUnpaidFines(fineService);
                        break;
                    case 2:
                        await ViewPaidFines(fineService);
                        break;
                    case 3:
                        await ViewAllFines(fineService);
                        break;
                    case 4:
                        await PayFine(fineService);
                        break;
                    case 5:
                        await CheckTotalUnpaidAmount(fineService);
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select 0-5.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task ViewUnpaidFines(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("MY UNPAID FINES");
            Console.WriteLine("==============");
            
            try
            {
                var fines = await fineService.GetUnpaidFinesByMemberIdAsync(UserSession.CurrentMemberId);
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine("You have no unpaid fines!");
                }
                else
                {
                    Console.WriteLine($"You have {fines.Count} unpaid fine(s):");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Amount",-12} {"Date",-15} {"Reason"}");
                    Console.WriteLine(new string('-', 60));
                    
                    foreach (var fine in fines)
                    {
                        Console.WriteLine($"{fine.Id,-5} Rs.{fine.FineAmount,-10} {fine.CreatedAt:yyyy-MM-dd,-15} {fine.FineReason}");
                    }
                    
                    var totalUnpaid = fines.Sum(f => f.FineAmount);
                    Console.WriteLine();
                    Console.WriteLine($"Total Unpaid Amount: Rs.{totalUnpaid}");
                    
                    if (totalUnpaid > 500)
                    {
                        Console.WriteLine("You cannot borrow new books until you clear fines above Rs.500!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewPaidFines(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("MY PAID FINES");
            Console.WriteLine("============");
            
            try
            {
                var fines = await fineService.GetPaidFinesByMemberIdAsync(UserSession.CurrentMemberId);
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine("You have no paid fines.");
                }
                else
                {
                    Console.WriteLine($"You have {fines.Count} paid fine(s):");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Amount",-12} {"Paid Date",-15} {"Reason"}");
                    Console.WriteLine(new string('-', 60));
                    
                    foreach (var fine in fines)
                    {
                        Console.WriteLine($"{fine.Id,-5} Rs.{fine.FineAmount,-10} {fine.PaymentDate:yyyy-MM-dd,-15} {fine.FineReason}");
                    }
                    
                    var totalPaid = fines.Sum(f => f.FineAmount);
                    Console.WriteLine();
                    Console.WriteLine($"Total Paid Amount: Rs.{totalPaid}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAllFines(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("ALL MY FINES");
            Console.WriteLine("===========");
            
            try
            {
                var fines = await fineService.GetFinesByMemberIdAsync(UserSession.CurrentMemberId);
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine("You have no fines.");
                }
                else
                {
                    Console.WriteLine($"Total Fines: {fines.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Amount",-12} {"Status",-12} {"Date",-12} {"Reason"}");
                    Console.WriteLine(new string('-', 70));
                    
                    foreach (var fine in fines)
                    {
                        Console.WriteLine($"{fine.Id,-5} Rs.{fine.FineAmount,-10} {fine.PaymentStatus,-12} {fine.CreatedAt:yyyy-MM-dd,-12} {fine.FineReason}");
                    }
                    
                    var totalAmount = fines.Sum(f => f.FineAmount);
                    var unpaidAmount = fines.Where(f => f.PaymentStatus == FinePaymentStatus.Pending).Sum(f => f.FineAmount);
                    var paidAmount = fines.Where(f => f.PaymentStatus == FinePaymentStatus.Paid).Sum(f => f.FineAmount);
                    
                    Console.WriteLine();
                    Console.WriteLine("Summary:");
                    Console.WriteLine($"  Total: Rs.{totalAmount}");
                    Console.WriteLine($"  Unpaid: Rs.{unpaidAmount}");
                    Console.WriteLine($"  Paid: Rs.{paidAmount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task PayFine(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("PAY A FINE");
            Console.WriteLine("=========");
            
            try
            {
                var unpaidFines = await fineService.GetUnpaidFinesByMemberIdAsync(UserSession.CurrentMemberId);
                
                if (unpaidFines == null || !unpaidFines.Any())
                {
                    Console.WriteLine("You have no unpaid fines to pay!");
                    await WaitForUserInput();
                    return;
                }
                
                Console.WriteLine("Your Unpaid Fines:");
                Console.WriteLine();
                Console.WriteLine($"{"ID",-5} {"Amount",-12} {"Reason"}");
                Console.WriteLine(new string('-', 50));
                
                foreach (var fine in unpaidFines)
                {
                    Console.WriteLine($"{fine.Id,-5} Rs.{fine.FineAmount,-10} {fine.FineReason}");
                }
                
                var totalUnpaid = unpaidFines.Sum(f => f.FineAmount);
                Console.WriteLine();
                Console.WriteLine($"Total Unpaid Amount: Rs.{totalUnpaid}");
                
                Console.Write("Enter Fine ID to pay: ");
                if (!int.TryParse(Console.ReadLine(), out int fineId) || fineId <= 0)
                {
                    Console.WriteLine("Invalid Fine ID.");
                    await WaitForUserInput();
                    return;
                }
                
                var selectedFine = unpaidFines.FirstOrDefault(f => f.Id == fineId);
                if (selectedFine == null)
                {
                    Console.WriteLine("Fine not found or already paid.");
                    await WaitForUserInput();
                    return;
                }
                
                Console.WriteLine();
                Console.WriteLine("Fine Details:");
                Console.WriteLine($"  Fine ID: {selectedFine.Id}");
                Console.WriteLine($"  Amount: Rs.{selectedFine.FineAmount}");
                Console.WriteLine($"  Reason: {selectedFine.FineReason}");
                
                Console.Write($"Confirm payment of Rs.{selectedFine.FineAmount}? (y/n): ");
                string confirm = Console.ReadLine()?.ToLower() ?? "";
                
                if (confirm != "y" && confirm != "yes")
                {
                    Console.WriteLine("Payment cancelled.");
                    await WaitForUserInput();
                    return;
                }
                
                var result = await fineService.ProcessFinePaymentAsync(UserSession.CurrentMemberId, fineId, selectedFine.FineAmount);
                
                if (result.Success)
                {
                    Console.WriteLine(result.Message);
                }
                else
                {
                    Console.WriteLine(result.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task CheckTotalUnpaidAmount(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("TOTAL UNPAID FINES");
            Console.WriteLine("=================");
            
            try
            {
                var totalUnpaid = await fineService.GetTotalUnpaidFineAmountByMemberAsync(UserSession.CurrentMemberId);
                
                if (totalUnpaid == 0)
                {
                    Console.WriteLine("You have no unpaid fines!");
                }
                else
                {
                    Console.WriteLine($"Total Unpaid Fines: Rs.{totalUnpaid}");
                    
                    if (totalUnpaid > 500)
                    {
                        Console.WriteLine("You cannot borrow new books until you clear fines above Rs.500!");
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