using LibrarySystem.Business.Services;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Presentation.Menus
{
    public static class MemberManagementMenu
    {
        public static async Task Show(IMemberService memberService, IBorrowingRulesService borrowingRulesService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("            MEMBER MANAGEMENT           ");
                Console.WriteLine("=========================================");
                Console.WriteLine("1. Add New Member");
                Console.WriteLine("2. View All Members");
                Console.WriteLine("3. Get Members by Membership Type");
                Console.WriteLine("4. Update Member");
                Console.WriteLine("5. Search Member by Email");
                Console.WriteLine("6. Activate/Deactivate Member");
                Console.WriteLine("0. Back to Main Menu");
                Console.WriteLine("=========================================");
                Console.Write("Enter Choice: ");
                string input = Console.ReadLine() ?? "0";
                
                if (!int.TryParse(input, out int choice))
                {
                    Console.WriteLine("Invalid input! Please enter a number.");
                    await WaitForUserInput();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await AddNewMember(memberService, borrowingRulesService);
                        break;
                    case 2:
                        await ViewAllMembers(memberService);
                        break;
                    case 3:
                        await GetMembersByType(memberService);
                        break;
                    case 4:
                        await UpdateMember(memberService, borrowingRulesService);
                        break;
                    case 5:
                        await SearchMemberByEmail(memberService);
                        break;
                    case 6:
                        await ToggleMemberStatus(memberService);
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please try again.");
                        await WaitForUserInput();
                        break;
                }
            }
        }

        private static async Task AddNewMember(IMemberService memberService, IBorrowingRulesService borrowingRulesService)
        {
            Console.Clear();
            Console.WriteLine("ADD NEW MEMBER");
            Console.WriteLine("=============");
            
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
            
            if (password != confirmPassword)
            {
                Console.WriteLine("Passwords do not match.");
                await WaitForUserInput();
                return;
            }
            
            Console.WriteLine();
            Console.WriteLine("Membership Types and limits:");

            var basicLimit = await borrowingRulesService.GetMaxBorrowingsAsync(MembershipType.Basic);
            var studentLimit = await borrowingRulesService.GetMaxBorrowingsAsync(MembershipType.Student);
            var premiumLimit = await borrowingRulesService.GetMaxBorrowingsAsync(MembershipType.Premium);
            
            Console.WriteLine($"1. Basic    - Can borrow up to {basicLimit} books");
            Console.WriteLine($"2. Premium  - Can borrow up to {premiumLimit} books");
            Console.WriteLine($"3. Student  - Can borrow up to {studentLimit} books");
            
            Console.Write("Select Membership Type (1-3): ");
            string typeInput = Console.ReadLine() ?? "1";
            
            MembershipType membershipType = MembershipType.Basic;
            switch (typeInput)
            {
                case "1": membershipType = MembershipType.Basic; break;
                case "2": membershipType = MembershipType.Premium; break;
                case "3": membershipType = MembershipType.Student; break;
                default: membershipType = MembershipType.Basic; break;
            }

            int allowedBorrowings = membershipType switch
            {
                MembershipType.Basic => basicLimit,
                MembershipType.Premium => premiumLimit,
                MembershipType.Student => studentLimit,
                _ => basicLimit
            };
            
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
            
            try
            {
                var result = await memberService.AddMemberAsync(member);
                
                Console.WriteLine($"Member added successfully!");
                Console.WriteLine($"  Member ID: {result.Id}");
                Console.WriteLine($"  Name: {result.Name}");
                Console.WriteLine($"  Email: {result.Email}");
                Console.WriteLine($"  Membership Type: {result.MembershipType}");
                Console.WriteLine($"  Allowed Borrowings: {result.AllowedBorrowingCount} books");
                Console.WriteLine($"  Current Borrowed: {result.CurrentBorrowedCount} books");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ViewAllMembers(IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("ALL MEMBERS");
            Console.WriteLine("===========");
            
            try
            {
                var members = await memberService.GetAllMembersAsync();
                
                if (members == null || !members.Any())
                {
                    Console.WriteLine("No members found in the system.");
                }
                else
                {
                    Console.WriteLine($"Total Members: {members.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Name",-25} {"Email",-25} {"Phone",-15} {"Type",-12} {"Allowed",-8} {"Borrowed",-8} {"Status",-10} {"Active"}");
                    Console.WriteLine(new string('-', 110));
                    
                    foreach (var member in members)
                    {
                        string activeStatus = member.IsActive ? "Yes" : "No";
                        Console.WriteLine($"{member.Id,-5} {member.Name,-25} {member.Email,-25} {member.PhoneNum,-15} {member.MembershipType,-12} {member.AllowedBorrowingCount,-8} {member.CurrentBorrowedCount,-8} {member.MembershipStatus,-10} {activeStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task GetMembersByType(IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("MEMBERS BY MEMBERSHIP TYPE");
            Console.WriteLine("=========================");
            
            Console.WriteLine("Membership Types:");
            Console.WriteLine("1. Basic");
            Console.WriteLine("2. Premium");
            Console.WriteLine("3. Student");
            
            Console.Write("Select Membership Type (1-3): ");
            string input = Console.ReadLine() ?? "1";
            
            MembershipType selectedType = input switch
            {
                "1" => MembershipType.Basic,
                "2" => MembershipType.Premium,
                "3" => MembershipType.Student,
                _ => MembershipType.Basic
            };
            
            try
            {
                var members = await memberService.GetByMembershipTypeAsync(selectedType);
                
                if (members == null || !members.Any())
                {
                    Console.WriteLine($"No members found with {selectedType} membership.");
                }
                else
                {
                    Console.WriteLine($"Members with {selectedType} Membership: {members.Count}");
                    Console.WriteLine();
                    Console.WriteLine($"{"ID",-5} {"Name",-30} {"Email",-30} {"Phone",-15} {"Status"}");
                    Console.WriteLine(new string('-', 85));
                    
                    foreach (var member in members)
                    {
                        Console.WriteLine($"{member.Id,-5} {member.Name,-30} {member.Email,-30} {member.PhoneNum,-15} {member.MembershipStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task UpdateMember(IMemberService memberService, IBorrowingRulesService borrowingRulesService)
        {
            Console.Clear();
            Console.WriteLine("UPDATE MEMBER");
            Console.WriteLine("============");
            
            Console.Write("Enter Member ID to update: ");
            string idInput = Console.ReadLine() ?? "";
            
            if (!int.TryParse(idInput, out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid member ID.");
                await WaitForUserInput();
                return;
            }
            
            Console.WriteLine();
            Console.WriteLine("Membership Types:");
            Console.WriteLine("1. Basic");
            Console.WriteLine("2. Premium");
            Console.WriteLine("3. Student");
            
            Console.Write("Select Membership Type (1-3): ");
            string typeInput = Console.ReadLine() ?? "1";
            
            MembershipType newType = typeInput switch
            {
                "1" => MembershipType.Basic,
                "2" => MembershipType.Premium,
                "3" => MembershipType.Student,
                _ => MembershipType.Basic
            };
            
            Console.WriteLine();
            Console.WriteLine("Membership Status:");
            Console.WriteLine("1. Active");
            Console.WriteLine("2. Inactive");
            
            Console.Write("Select Membership Status (1-2): ");
            string statusInput = Console.ReadLine() ?? "1";
            
            MembershipStatus newStatus = statusInput switch
            {
                "1" => MembershipStatus.Active,
                "2" => MembershipStatus.Inactive,
                _ => MembershipStatus.Active
            };
            
            Console.Write("Is Active? (y/n): ");
            string activeInput = Console.ReadLine()?.ToLower() ?? "";
            bool isActive = activeInput == "y" || activeInput == "yes";
            
            try
            {
                var updatedMember = await memberService.UpdateMemberAsync(memberId, newType, newStatus, isActive);
                
                Console.WriteLine($"Member updated successfully!");
                Console.WriteLine($"  Member ID: {updatedMember.Id}");
                Console.WriteLine($"  Name: {updatedMember.Name}");
                Console.WriteLine($"  New Type: {updatedMember.MembershipType}");
                Console.WriteLine($"  New Status: {updatedMember.MembershipStatus}");
                Console.WriteLine($"  Active: {(updatedMember.IsActive ? "Yes" : "No")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task SearchMemberByEmail(IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("SEARCH MEMBER BY EMAIL");
            Console.WriteLine("=====================");
            
            Console.Write("Enter Email Address: ");
            string email = Console.ReadLine() ?? "";
            
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email cannot be empty.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var allMembers = await memberService.GetAllMembersAsync();
                var member = allMembers.FirstOrDefault(m => m.Email?.ToLower() == email.ToLower());
                
                if (member == null)
                {
                    Console.WriteLine($"No member found with email: {email}");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Member Details:");
                    Console.WriteLine($"ID: {member.Id}");
                    Console.WriteLine($"Name: {member.Name}");
                    Console.WriteLine($"Email: {member.Email}");
                    Console.WriteLine($"Phone: {member.PhoneNum}");
                    Console.WriteLine($"Membership Type: {member.MembershipType}");
                    Console.WriteLine($"Allowed Borrowings: {member.AllowedBorrowingCount}");
                    Console.WriteLine($"Current Borrowed: {member.CurrentBorrowedCount}");
                    Console.WriteLine($"Membership Status: {member.MembershipStatus}");
                    Console.WriteLine($"Active: {(member.IsActive ? "Yes" : "No")}");
                    Console.WriteLine($"Created: {member.CreatedAt:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"Last Updated: {member.UpdatedAt:yyyy-MM-dd HH:mm}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            await WaitForUserInput();
        }

        private static async Task ToggleMemberStatus(IMemberService memberService)
        {
            Console.Clear();
            Console.WriteLine("ACTIVATE/DEACTIVATE MEMBER");
            Console.WriteLine("========================");
            
            Console.Write("Enter Member ID: ");
            string idInput = Console.ReadLine() ?? "";
            
            if (!int.TryParse(idInput, out int memberId) || memberId <= 0)
            {
                Console.WriteLine("Invalid member ID.");
                await WaitForUserInput();
                return;
            }
            
            try
            {
                var allMembers = await memberService.GetAllMembersAsync();
                var member = allMembers.FirstOrDefault(m => m.Id == memberId);
                
                if (member == null)
                {
                    Console.WriteLine($"Member with ID {memberId} not found.");
                }
                else
                {
                    Console.WriteLine($"Current Status: {(member.IsActive ? "Active" : "Inactive")}");
                    Console.Write($"Do you want to {(member.IsActive ? "deactivate" : "activate")} this member? (y/n): ");
                    string confirm = Console.ReadLine()?.ToLower() ?? "";
                    
                    if (confirm == "y" || confirm == "yes")
                    {
                        bool newStatus = !member.IsActive;
                        var updatedMember = await memberService.UpdateMemberAsync(
                            memberId, 
                            member.MembershipType, 
                            member.MembershipStatus, 
                            newStatus);
                        
                        Console.WriteLine($"Member {(newStatus ? "activated" : "deactivated")} successfully!");
                        Console.WriteLine($"  Member: {updatedMember.Name}");
                        Console.WriteLine($"  New Status: {(updatedMember.IsActive ? "Active" : "Inactive")}");
                    }
                    else
                    {
                        Console.WriteLine("Operation cancelled.");
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