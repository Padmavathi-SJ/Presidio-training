using LibrarySystem.DataAccess.Entities;

namespace LibrarySystem.Business.Services
{
    public interface IFineService
    {
        Task<Fine> GetFineByIdAsync(int id);
        Task<List<Fine>> GetAllFinesAsync();
        Task<List<Fine>> GetFinesByMemberIdAsync(int memberId);
        Task<List<Fine>> GetUnpaidFinesByMemberIdAsync(int memberId);
        Task<List<Fine>> GetPaidFinesByMemberIdAsync(int memberId);
        Task<Fine> AddFineAsync(Fine fine);
        Task<Fine> PayFineAsync(int fineId);
        Task<FinePaymentResult> ProcessFinePaymentAsync(int memberId, int fineId, decimal paymentAmount);
        Task<decimal> GetTotalUnpaidFineAmountByMemberAsync(int memberId);
        Task<bool> HasUnpaidFinesAsync(int memberId);
        Task<List<Fine>> GetAllUnpaidFinesAsync();
        Task<decimal> CalculateMemberTotalUnpaidFineWithSPAsync(int memberId);  // No body here - just the declaration
    }

    public class FinePaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Fine? PaidFine { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}