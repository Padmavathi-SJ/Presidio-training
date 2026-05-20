
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IBorrowingRepository
    {
        Task<Borrowing?> GetByIdAsync(int id);
        Task<List<Borrowing>> GetByMemberIdAsync(int memberId);
        Task<List<Borrowing>> GetActiveByMemberIdAsync(int memberId);
        Task<List<Borrowing>> GetAllActiveAsync();
        Task<List<Borrowing>> GetOverdueAsync();
        Task<int> GetActiveCountByMemberAsync(int memberId);
        Task<bool> HasMemberBorrowedBookAsync(int memberId, int bookId);
        Task<Borrowing> AddAsync(Borrowing borrowing);
        Task<Borrowing> UpdateAsync(Borrowing borrowing);
        Task<decimal> GetUnpaidFineAmountByMemberAsync(int memberId);
    }
}