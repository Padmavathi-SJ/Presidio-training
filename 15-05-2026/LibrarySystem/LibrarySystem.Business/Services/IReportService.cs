using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.DataAccess.Repositories;

namespace LibrarySystem.Business.Services
{
    public interface IReportService
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
}