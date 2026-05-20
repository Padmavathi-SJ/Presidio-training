using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IReportRepository
    {
        // Book Reports
        Task<int> GetTotalBooksCountAsync();
        Task<int> GetTotalAvailableBooksCountAsync();
        Task<int> GetTotalBorrowedBooksCountAsync();
        Task<int> GetTotalDamagedBooksCountAsync();
      
        // Member Reports
        Task<int> GetTotalMembersCountAsync();
        Task<int> GetActiveMembersCountAsync();
        Task<int> GetInactiveMembersCountAsync();
        Task<Dictionary<MembershipType, int>> GetMembersByMembershipTypeAsync();
        
        // Borrowing Reports
        Task<int> GetTotalBorrowingsCountAsync();
        Task<int> GetActiveBorrowingsCountAsync();
        Task<int> GetCompletedBorrowingsCountAsync();
        Task<int> GetOverdueBorrowingsCountAsync();
        Task<decimal> GetTotalFineCollectedAsync();
        Task<decimal> GetTotalPendingFineAmountAsync();
        Task<List<Borrowing>> GetOverdueBorrowingsWithDetailsAsync();
        
      
        
        // Category Reports
        Task<List<BookCategory>> GetCategoriesWithBookCountAsync();
        
        // Dashboard Summary
        Task<DashboardSummary> GetDashboardSummaryAsync();
    }

    public class DashboardSummary
    {
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int BorrowedBooks { get; set; }
        public int DamagedBooks { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public decimal TotalFinesCollected { get; set; }
        public decimal PendingFines { get; set; }
    }
}