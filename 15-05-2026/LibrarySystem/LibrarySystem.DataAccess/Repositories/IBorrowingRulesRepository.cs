using System;
using System.Collections.Generic;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public interface IBorrowingRulesRepository
    {
        Task<BorrowingRules> GetByMembershipTypeAsync(MembershipType membershipType);
        Task<BorrowingRules> AddAsync(BorrowingRules rules);
        Task<List<BorrowingRules>> GetAllAsync();
        Task<BorrowingRules> GetByIdAsync(int id);
        Task<BorrowingRules> UpdateAsync(BorrowingRules rules);

        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByMembershipTypeAsync(MembershipType membershipType);
    }
}