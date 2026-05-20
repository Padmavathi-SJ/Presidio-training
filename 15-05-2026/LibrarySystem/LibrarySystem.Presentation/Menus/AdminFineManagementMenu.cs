using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Presentation.Menus
{
    public static class AdminFineManagementMenu
    {
        public static async Task Show(IFineService fineService, IMemberService memberService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("              FINE MANAGEMENT           ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. View All Fines");
                Console.WriteLine("2. View Unpaid Fines");
                Console.WriteLine("3. View Fines by Member");
                Console.WriteLine("4. View Member's Unpaid Fines");
                Console.WriteLine("5. Generate Fine Report");
                Console.WriteLine("6. Back to Admin Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter your choice (1-6): ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await ViewAllFines(fineService);
                        break;
                    case 2:
                        await ViewUnpaidFines(fineService);
                        break;
                    case 3:
                        await ViewFinesByMember(fineService, memberService);
                        break;
                    case 4:
                        await ViewMemberUnpaidFines(fineService, memberService);
                        break;
                    case 5:
                        await GenerateFineReport(fineService);
                        break;
                    case 6:
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please select 1-6.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task ViewAllFines(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("ALL FINES");
            Console.WriteLine("=========");
            
            try
            {
                var fines = await fineService.GetAllFinesAsync();
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine("No fines found in the system.");
                }
                else
                {
                    Console.WriteLine($"Total Fines: {fines.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Member ID",-10} {"Member Name",-25} {"Amount",-10} {"Status",-12} {"Date"}");
                    Console.WriteLine(new string('-', 85));
                    
                    foreach (var fine in fines)
                    {
                        string memberName = fine.Member?.Name?.Length > 22 ? fine.Member.Name.Substring(0, 19) + "..." : fine.Member?.Name ?? "N/A";
                        Console.WriteLine($"{fine.Id,-5} {fine.MemberId,-10} {memberName,-25} Rs.{fine.FineAmount,-8} {fine.PaymentStatus,-12} {fine.CreatedAt:yyyy-MM-dd}");
                    }
                    
                    var totalAmount = fines.Sum(f => f.FineAmount);
                    var unpaidAmount = fines.Where(f => f.PaymentStatus == FinePaymentStatus.Pending).Sum(f => f.FineAmount);
                    var paidAmount = fines.Where(f => f.PaymentStatus == FinePaymentStatus.Paid).Sum(f => f.FineAmount);
                    
                    Console.WriteLine();
                    Console.WriteLine("SUMMARY");
                    Console.WriteLine($"Total Fine Amount: Rs.{totalAmount}");
                    Console.WriteLine($"Unpaid Amount: Rs.{unpaidAmount}");
                    Console.WriteLine($"Paid Amount: Rs.{paidAmount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewUnpaidFines(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("UNPAID FINES");
            Console.WriteLine("============");
            
            try
            {
                var fines = await fineService.GetAllUnpaidFinesAsync();
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine("No unpaid fines found.");
                }
                else
                {
                    Console.WriteLine($"Unpaid Fines: {fines.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Member ID",-10} {"Member Name",-25} {"Amount",-10} {"Reason"}");
                    Console.WriteLine(new string('-', 75));
                    
                    foreach (var fine in fines)
                    {
                        string memberName = fine.Member?.Name?.Length > 22 ? fine.Member.Name.Substring(0, 19) + "..." : fine.Member?.Name ?? "N/A";
                        string reason = fine.FineReason.Length > 30 ? fine.FineReason.Substring(0, 27) + "..." : fine.FineReason;
                        Console.WriteLine($"{fine.Id,-5} {fine.MemberId,-10} {memberName,-25} Rs.{fine.FineAmount,-8} {reason}");
                    }
                    
                    var totalUnpaid = fines.Sum(f => f.FineAmount);
                    Console.WriteLine();
                    Console.WriteLine($"Total Unpaid Amount: Rs.{totalUnpaid}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewFinesByMember(IFineService fineService, IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("FINES BY MEMBER");
            Console.WriteLine("===============");
            
            Console.Write("Enter Member ID: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid Member ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var fines = await fineService.GetFinesByMemberIdAsync(memberId);
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine($"No fines found for Member ID {memberId}.");
                }
                else
                {
                    Console.WriteLine($"Fines for Member ID {memberId}: {fines.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Amount",-10} {"Status",-12} {"Created",-15} {"Reason"}");
                    Console.WriteLine(new string('-', 65));
                    
                    foreach (var fine in fines)
                    {
                        string reason = fine.FineReason.Length > 25 ? fine.FineReason.Substring(0, 22) + "..." : fine.FineReason;
                        Console.WriteLine($"{fine.Id,-5} Rs.{fine.FineAmount,-8} {fine.PaymentStatus,-12} {fine.CreatedAt:yyyy-MM-dd,-15} {reason}");
                    }
                    
                    var totalAmount = fines.Sum(f => f.FineAmount);
                    var unpaidAmount = fines.Where(f => f.PaymentStatus == FinePaymentStatus.Pending).Sum(f => f.FineAmount);
                    
                    Console.WriteLine();
                    Console.WriteLine($"Total: Rs.{totalAmount} (Unpaid: Rs.{unpaidAmount})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewMemberUnpaidFines(IFineService fineService, IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("MEMBER UNPAID FINES");
            Console.WriteLine("==================");
            
            Console.Write("Enter Member ID: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid Member ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var fines = await fineService.GetUnpaidFinesByMemberIdAsync(memberId);
                
                if (fines == null || !fines.Any())
                {
                    Console.WriteLine($"No unpaid fines found for Member ID {memberId}.");
                }
                else
                {
                    Console.WriteLine($"Unpaid Fines for Member ID {memberId}: {fines.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Amount",-10} {"Created",-15} {"Reason"}");
                    Console.WriteLine(new string('-', 55));
                    
                    foreach (var fine in fines)
                    {
                        Console.WriteLine($"{fine.Id,-5} Rs.{fine.FineAmount,-8} {fine.CreatedAt:yyyy-MM-dd,-15} {fine.FineReason}");
                    }
                    
                    var totalUnpaid = fines.Sum(f => f.FineAmount);
                    Console.WriteLine();
                    Console.WriteLine($"Total Unpaid Amount: Rs.{totalUnpaid}");
                    
                    if (totalUnpaid > 500)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Member cannot borrow new books until fines are cleared!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task GenerateFineReport(IFineService fineService)
        {
            Console.Clear();
            Console.WriteLine("FINE REPORT");
            Console.WriteLine("===========");
            
            try
            {
                var allFines = await fineService.GetAllFinesAsync();
                var unpaidFines = await fineService.GetAllUnpaidFinesAsync();
                
                Console.WriteLine();
                Console.WriteLine("FINE REPORT SUMMARY");
                Console.WriteLine("===================");
                Console.WriteLine($"Total Fines Issued: {allFines.Count}");
                Console.WriteLine($"Total Fine Amount: Rs.{allFines.Sum(f => f.FineAmount)}");
                Console.WriteLine($"Unpaid Fines Count: {unpaidFines.Count}");
                Console.WriteLine($"Unpaid Fine Amount: Rs.{unpaidFines.Sum(f => f.FineAmount)}");
                Console.WriteLine($"Paid Fines Count: {allFines.Count - unpaidFines.Count}");
                Console.WriteLine($"Paid Fine Amount: Rs.{allFines.Sum(f => f.FineAmount) - unpaidFines.Sum(f => f.FineAmount)}");
                
                if (unpaidFines.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Members with Unpaid Fines:");
                    var membersWithFines = unpaidFines.GroupBy(f => f.MemberId)
                        .Select(g => new { MemberId = g.Key, Total = g.Sum(f => f.FineAmount), Count = g.Count() });
                    
                    foreach (var member in membersWithFines)
                    {
                        Console.WriteLine($"  Member ID {member.MemberId}: {member.Count} fine(s), Total: Rs.{member.Total}");
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