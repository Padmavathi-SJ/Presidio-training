using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public class BorrowingRulesRepository : IBorrowingRulesRepository
    {
        private readonly ApplicationDbContext _context;

        public BorrowingRulesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BorrowingRules> GetByMembershipTypeAsync(MembershipType membershipType)
        {
            return await _context.BorrowingRules
                .FirstOrDefaultAsync(r => r.MembershipType == membershipType);
        }

        public async Task<List<BorrowingRules>> GetAllAsync()
        {
            return await _context.BorrowingRules
                .OrderBy(r => r.MembershipType)
                .ToListAsync();
        }

        public async Task<BorrowingRules> GetByIdAsync(int id)
        {
            return await _context.BorrowingRules
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<BorrowingRules> AddAsync(BorrowingRules rules)
        {
            rules.CreatedAt = DateTime.UtcNow;
            rules.UpdatedAt = DateTime.UtcNow;
            
            await _context.BorrowingRules.AddAsync(rules);
            await _context.SaveChangesAsync();
            return rules;
        }

        public async Task<BorrowingRules> UpdateAsync(BorrowingRules rules)
        {
            rules.UpdatedAt = DateTime.UtcNow;
            _context.BorrowingRules.Update(rules);
            await _context.SaveChangesAsync();
            return rules;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rules = await GetByIdAsync(id);
            if (rules == null)
                return false;
            
            _context.BorrowingRules.Remove(rules);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByMembershipTypeAsync(MembershipType membershipType)
        {
            return await _context.BorrowingRules
                .AnyAsync(r => r.MembershipType == membershipType);
        }
    }
}