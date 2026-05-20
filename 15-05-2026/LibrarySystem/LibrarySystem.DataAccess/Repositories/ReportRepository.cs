using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Book Reports
        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books.CountAsync(b => b.IsActive);
        }

        public async Task<int> GetTotalAvailableBooksCountAsync()
        {
            return await _context.BookCopies
                .CountAsync(bc => bc.IsAvailable && !bc.IsBorrowed && !bc.IsDamaged && bc.IsAvailable);
        }

        public async Task<int> GetTotalBorrowedBooksCountAsync()
        {
            return await _context.Borrowings
                .CountAsync(b => b.Status == BookBorrowStatus.Borrowed && b.IsActive);
        }

        public async Task<int> GetTotalDamagedBooksCountAsync()
        {
            return await _context.BookCopies
                .CountAsync(bc => bc.IsDamaged && bc.IsAvailable);
        }

        // Member Reports
        public async Task<int> GetTotalMembersCountAsync()
        {
            return await _context.Members.CountAsync();
        }

        public async Task<int> GetActiveMembersCountAsync()
        {
            return await _context.Members.CountAsync(m => m.IsActive && m.MembershipStatus == MembershipStatus.Active);
        }

        public async Task<int> GetInactiveMembersCountAsync()
        {
            return await _context.Members.CountAsync(m => !m.IsActive || m.MembershipStatus != MembershipStatus.Active);
        }

        public async Task<Dictionary<MembershipType, int>> GetMembersByMembershipTypeAsync()
        {
            return await _context.Members
                .GroupBy(m => m.MembershipType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
        }

        // Borrowing Reports
        public async Task<int> GetTotalBorrowingsCountAsync()
        {
            return await _context.Borrowings.CountAsync();
        }

        public async Task<int> GetActiveBorrowingsCountAsync()
        {
            return await _context.Borrowings
                .CountAsync(b => b.Status == BookBorrowStatus.Borrowed && b.IsActive);
        }

        public async Task<int> GetCompletedBorrowingsCountAsync()
        {
            return await _context.Borrowings
                .CountAsync(b => b.Status == BookBorrowStatus.Returned && b.IsActive);
        }

        public async Task<int> GetOverdueBorrowingsCountAsync()
        {
            return await _context.Borrowings
                .CountAsync(b => b.Status == BookBorrowStatus.Borrowed && 
                                b.DueDate < DateTime.UtcNow && 
                                b.IsActive);
        }

        public async Task<decimal> GetTotalFineCollectedAsync()
        {
            return await _context.Fines
                .Where(f => f.PaymentStatus == FinePaymentStatus.Paid && f.IsActive)
                .SumAsync(f => f.FineAmount);
        }

        public async Task<decimal> GetTotalPendingFineAmountAsync()
        {
            return await _context.Fines
                .Where(f => f.PaymentStatus == FinePaymentStatus.Pending && f.IsActive)
                .SumAsync(f => f.FineAmount);
        }

        public async Task<List<Borrowing>> GetOverdueBorrowingsWithDetailsAsync()
        {
            return await _context.Borrowings
                .Include(b => b.Member)
                .Include(b => b.Book)
                .Include(b => b.BookCopy)
                .Where(b => b.Status == BookBorrowStatus.Borrowed && 
                           b.DueDate < DateTime.UtcNow && 
                           b.IsActive)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }


        // Category Reports
        public async Task<List<BookCategory>> GetCategoriesWithBookCountAsync()
        {
            return await _context.BookCategories
                .Include(c => c.Books.Where(b => b.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        // Dashboard Summary
        public async Task<DashboardSummary> GetDashboardSummaryAsync()
        {
            var summary = new DashboardSummary();
            
            summary.TotalBooks = await GetTotalBooksCountAsync();
            summary.AvailableBooks = await GetTotalAvailableBooksCountAsync();
            summary.BorrowedBooks = await GetTotalBorrowedBooksCountAsync();
            summary.DamagedBooks = await GetTotalDamagedBooksCountAsync();
            summary.TotalMembers = await GetTotalMembersCountAsync();
            summary.ActiveMembers = await GetActiveMembersCountAsync();
            summary.ActiveBorrowings = await GetActiveBorrowingsCountAsync();
            summary.OverdueBorrowings = await GetOverdueBorrowingsCountAsync();
            summary.TotalFinesCollected = await GetTotalFineCollectedAsync();
            summary.PendingFines = await GetTotalPendingFineAmountAsync();
            
            return summary;
        }
    }
}