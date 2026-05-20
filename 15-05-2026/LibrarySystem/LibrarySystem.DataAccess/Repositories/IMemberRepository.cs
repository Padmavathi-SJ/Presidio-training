using System;
using System.Collections.Generic;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IMemberRepository
    {
        Task<Member> AddMember(Member member);
        Task<List<Member>> GetAll();
        Task<Member?> GetById(int id);
        Task<Member> UpdateMember(Member member);
        Task<Member?> GetByEmail(string email);
        Task<bool> ExistsByEmail(string email);
        Task<List<Member>> GetByMembershipType(MembershipType type);

//Get member borrowing summary using stored procedure
Task<(int Active, int Returned, int Overdue, decimal Fine)>
    GetMemberBorrowingSummaryWithSPAsync(int memberId);
    
    }
    
    public class MemberBorrowingSummary
    {
        public int ActiveBorrowings { get; set; }
        public int ReturnedBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public decimal TotalFine { get; set; }
    }
    }
