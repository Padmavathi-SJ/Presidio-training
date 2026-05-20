using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.DataAccess.Repositories;

namespace LibrarySystem.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        // Book Reports
        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _reportRepository.GetTotalBooksCountAsync();
        }

        public async Task<int> GetTotalAvailableBooksCountAsync()
        {
            return await _reportRepository.GetTotalAvailableBooksCountAsync();
        }

        public async Task<int> GetTotalBorrowedBooksCountAsync()
        {
            return await _reportRepository.GetTotalBorrowedBooksCountAsync();
        }

        public async Task<int> GetTotalDamagedBooksCountAsync()
        {
            return await _reportRepository.GetTotalDamagedBooksCountAsync();
        }


        // Member Reports
        public async Task<int> GetTotalMembersCountAsync()
        {
            return await _reportRepository.GetTotalMembersCountAsync();
        }

        public async Task<int> GetActiveMembersCountAsync()
        {
            return await _reportRepository.GetActiveMembersCountAsync();
        }

        public async Task<int> GetInactiveMembersCountAsync()
        {
            return await _reportRepository.GetInactiveMembersCountAsync();
        }

        public async Task<Dictionary<MembershipType, int>> GetMembersByMembershipTypeAsync()
        {
            return await _reportRepository.GetMembersByMembershipTypeAsync();
        }

        // Borrowing Reports
        public async Task<int> GetTotalBorrowingsCountAsync()
        {
            return await _reportRepository.GetTotalBorrowingsCountAsync();
        }

        public async Task<int> GetActiveBorrowingsCountAsync()
        {
            return await _reportRepository.GetActiveBorrowingsCountAsync();
        }

        public async Task<int> GetCompletedBorrowingsCountAsync()
        {
            return await _reportRepository.GetCompletedBorrowingsCountAsync();
        }

        public async Task<int> GetOverdueBorrowingsCountAsync()
        {
            return await _reportRepository.GetOverdueBorrowingsCountAsync();
        }

        public async Task<decimal> GetTotalFineCollectedAsync()
        {
            return await _reportRepository.GetTotalFineCollectedAsync();
        }

        public async Task<decimal> GetTotalPendingFineAmountAsync()
        {
            return await _reportRepository.GetTotalPendingFineAmountAsync();
        }

        public async Task<List<Borrowing>> GetOverdueBorrowingsWithDetailsAsync()
        {
            return await _reportRepository.GetOverdueBorrowingsWithDetailsAsync();
        }

       
        // Category Reports
        public async Task<List<BookCategory>> GetCategoriesWithBookCountAsync()
        {
            return await _reportRepository.GetCategoriesWithBookCountAsync();
        }

        // Dashboard Summary
        public async Task<DashboardSummary> GetDashboardSummaryAsync()
        {
            return await _reportRepository.GetDashboardSummaryAsync();
        }
    }
}