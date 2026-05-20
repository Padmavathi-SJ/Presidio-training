using System;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.DataAccess.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByPhoneNumAsync(string phoneNum)
        {
            return await _context.Admins.AnyAsync(a => a.PhoneNum == phoneNum);
        }

        public async Task<int> GetNextIdAsync()
        {
            var maxId = await _context.Admins.MaxAsync(a => (int?)a.Id) ?? 0;
            return maxId + 1;
        }

        public async Task<Admin?> GetByPhoneNumAsync(string phoneNum)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.PhoneNum == phoneNum);
        }
    }
}