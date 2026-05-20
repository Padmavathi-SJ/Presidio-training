using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.DataAccess.Repositories;
using LibrarySystem.Business.Exceptions;

namespace LibrarySystem.Business.Services
{
    public class BorrowingRulesService : IBorrowingRulesService
    {
        private readonly IBorrowingRulesRepository _rulesRepository;

        public BorrowingRulesService(IBorrowingRulesRepository rulesRepository)
        {
            _rulesRepository = rulesRepository;
        }

        public async Task<BorrowingRules> GetRulesByMembershipTypeAsync(MembershipType membershipType)
        {
            var rules = await _rulesRepository.GetByMembershipTypeAsync(membershipType);
            
            if (rules == null)
                throw new NotFoundException($"Borrowing rules for {membershipType} membership not found.");
            
            return rules;
        }

        public async Task<List<BorrowingRules>> GetAllRulesAsync()
        {
            var rules = await _rulesRepository.GetAllAsync();
            
            if (rules == null || !rules.Any())
                throw new NotFoundException("No borrowing rules found in the system.");
            
            return rules;
        }

        public async Task<BorrowingRules> AddRulesAsync(BorrowingRules rules)
        {
            // Validate
            if (rules == null)
                throw new ValidationException("Borrowing rules cannot be null.");
            
            if (rules.MaxActiveBorrowings <= 0)
                throw new ValidationException("Max active borrowings must be greater than 0.");
            
            if (rules.MaxBorrowDays <= 0)
                throw new ValidationException("Max borrow days must be greater than 0.");
            
            // Check if rules already exist for this membership type
            var exists = await _rulesRepository.ExistsByMembershipTypeAsync(rules.MembershipType);
            if (exists)
                throw new DuplicateException($"Borrowing rules for {rules.MembershipType} membership already exists.");
            
            return await _rulesRepository.AddAsync(rules);
        }

        public async Task<BorrowingRules> UpdateRulesAsync(int id, BorrowingRules rules)
        {
            // Validate
            if (rules == null)
                throw new ValidationException("Borrowing rules cannot be null.");
            
            if (id <= 0)
                throw new ValidationException("Invalid rule ID.");
            
            if (rules.MaxActiveBorrowings <= 0)
                throw new ValidationException("Max active borrowings must be greater than 0.");
            
            if (rules.MaxBorrowDays <= 0)
                throw new ValidationException("Max borrow days must be greater than 0.");
            
            var existingRules = await _rulesRepository.GetByIdAsync(id);
            if (existingRules == null)
                throw new NotFoundException($"Borrowing rules with ID {id} not found.");
            
            // Update properties
            existingRules.MembershipType = rules.MembershipType;
            existingRules.MaxActiveBorrowings = rules.MaxActiveBorrowings;
            existingRules.MaxBorrowDays = rules.MaxBorrowDays;
            existingRules.UpdatedAt = DateTime.UtcNow;
            
            return await _rulesRepository.UpdateAsync(existingRules);
        }

        public async Task<bool> DeleteRulesAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("Invalid rule ID.");
            
            var rules = await _rulesRepository.GetByIdAsync(id);
            if (rules == null)
                throw new NotFoundException($"Borrowing rules with ID {id} not found.");
            
            return await _rulesRepository.DeleteAsync(id);
        }

        public async Task<int> GetMaxBorrowingsAsync(MembershipType membershipType)
        {
            var rules = await _rulesRepository.GetByMembershipTypeAsync(membershipType);
            
            if (rules == null)
            {
                // Return default values based on membership type
                return membershipType switch
                {
                    MembershipType.Basic => 2,
                    MembershipType.Student => 3,
                    MembershipType.Premium => 5,
                  
                    _ => 2
                };
            }
            
            return rules.MaxActiveBorrowings;
        }

        public async Task<int> GetMaxBorrowDaysAsync(MembershipType membershipType)
        {
            var rules = await _rulesRepository.GetByMembershipTypeAsync(membershipType);
            
            if (rules == null)
            {
                // Return default values based on membership type
                return membershipType switch
                {
                    MembershipType.Basic => 7,
                    MembershipType.Student => 10,
                    MembershipType.Premium => 15,
                  
                    _ => 7
                };
            }
            
            return rules.MaxBorrowDays;
        }

        public async Task<BorrowingRules> GetRulesByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("Invalid rule ID.");
            
            var rules = await _rulesRepository.GetByIdAsync(id);
            
            if (rules == null)
                throw new NotFoundException($"Borrowing rules with ID {id} not found.");
            
            return rules;
        }
    }
}