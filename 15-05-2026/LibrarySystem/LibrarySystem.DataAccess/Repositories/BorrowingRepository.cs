
using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;


namespace LibrarySystem.DataAccess.Repositories
{
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly ApplicationDbContext _context;

        public BorrowingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Borrowing?> GetByIdAsync(int id)
        {
            return await _context.Borrowings
                .Include(b => b.Member)
                .Include(b => b.Book)
                .Include(b => b.BookCopy)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Borrowing>> GetByMemberIdAsync(int memberId)
        {
            return await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.BookCopy)
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.BorrowedDate)
                .ToListAsync();
        }

        public async Task<List<Borrowing>> GetActiveByMemberIdAsync(int memberId)
        {
            return await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.BookCopy)
                .Where(b => b.MemberId == memberId && 
                           b.Status == BookBorrowStatus.Borrowed &&
                           b.IsActive)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<List<Borrowing>> GetAllActiveAsync()
        {
            return await _context.Borrowings
                .Include(b => b.Member)
                .Include(b => b.Book)
                .Include(b => b.BookCopy)
                .Where(b => b.Status == BookBorrowStatus.Borrowed && b.IsActive)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<List<Borrowing>> GetOverdueAsync()
        {
            return await _context.Borrowings
                .Include(b => b.Member)
                .Include(b => b.Book)
                .Include(b => b.BookCopy)
                .Where(b => b.Status == BookBorrowStatus.Borrowed && 
                           b.DueDate < DateTime.UtcNow &&
                           b.IsActive)
                .ToListAsync();
        }

        public async Task<int> GetActiveCountByMemberAsync(int memberId)
        {
            return await _context.Borrowings
                .CountAsync(b => b.MemberId == memberId && 
                                b.Status == BookBorrowStatus.Borrowed &&
                                b.IsActive);
        }

        public async Task<bool> HasMemberBorrowedBookAsync(int memberId, int bookId)
        {
            return await _context.Borrowings
                .AnyAsync(b => b.MemberId == memberId && 
                              b.BookId == bookId && 
                              b.Status == BookBorrowStatus.Borrowed &&
                              b.IsActive);
        }

        public async Task<Borrowing> AddAsync(Borrowing borrowing)
        {
            await _context.Borrowings.AddAsync(borrowing);
            await _context.SaveChangesAsync();
            return borrowing;
        }

        public async Task<Borrowing> UpdateAsync(Borrowing borrowing)
        {
            _context.Borrowings.Update(borrowing);
            await _context.SaveChangesAsync();
            return borrowing;
        }

        public async Task<decimal> GetUnpaidFineAmountByMemberAsync(int memberId)
        {
            return await _context.Fines
                .Where(f => f.MemberId == memberId && 
                           f.PaymentStatus == FinePaymentStatus.Pending &&
                           f.IsActive)
                .SumAsync(f => f.FineAmount);
        }
    }
}