using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IFineRepository
    {
        Task<Fine> GetByIdAsync(int id);
        Task<List<Fine>> GetAllFinesAsync();
        Task<List<Fine>> GetFinesByMemberIdAsync(int memberId);
        Task<List<Fine>> GetUnpaidFinesByMemberIdAsync(int memberId);
        Task<List<Fine>> GetPaidFinesByMemberIdAsync(int memberId);
        Task<Fine> AddFineAsync(Fine fine);
        Task<Fine> UpdateFineAsync(Fine fine);
        Task<Fine> PayFineAsync(int fineId);
        Task<decimal> GetTotalUnpaidFineAmountByMemberAsync(int memberId);
        Task<bool> HasUnpaidFinesAsync(int memberId);
        Task<List<Fine>> GetAllUnpaidFinesAsync();
    }
}