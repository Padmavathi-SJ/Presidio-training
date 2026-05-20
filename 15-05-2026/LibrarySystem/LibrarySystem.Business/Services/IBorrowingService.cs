
using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Business.Services
{
    public interface IBorrowingService
    {
        Task<Borrowing> BorrowBookAsync(int memberId, int bookCopyId);
        Task<Borrowing> ReturnBookAsync(int borrowingId);
        Task<List<Borrowing>> GetMemberBorrowingsAsync(int memberId);
        Task<List<Borrowing>> GetActiveBorrowingsAsync(int memberId);
        Task<(bool CanBorrow, string Message, int CurrentCount, int MaxAllowed, decimal UnpaidFines)> 
            CheckBorrowingEligibilityAsync(int memberId);
        Task<(bool IsValid, string ErrorMessage, int CurrentBorrowings, int AllowedBorrowings, decimal UnpaidFines)> 
            ValidateBorrowingAsync(int memberId, int bookCopyId);
        Task<decimal> GetUnpaidFineAmountAsync(int memberId);

        Task<List<Borrowing>> GetAllBorrowingsAsync();
        Task<List<Borrowing>> GetAllActiveBorrowingsAsync();
        Task<List<Borrowing>> GetOverdueBorrowingsAsync();
    }
}