using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Business.Services
{
    public interface IBorrowingRulesService
    {
        Task<BorrowingRules> GetRulesByMembershipTypeAsync(MembershipType membershipType);
        Task<BorrowingRules> AddRulesAsync(BorrowingRules rules);
        Task<List<BorrowingRules>> GetAllRulesAsync();
        Task<BorrowingRules> GetRulesByIdAsync(int id);
        Task<BorrowingRules> UpdateRulesAsync(int id, BorrowingRules rules);
        Task<bool> DeleteRulesAsync(int id);
        Task<int> GetMaxBorrowingsAsync(MembershipType membershipType);
        Task<int> GetMaxBorrowDaysAsync(MembershipType membershipType);

    }
}