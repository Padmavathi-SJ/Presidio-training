using Microsoft.EntityFrameworkCore;
using System;
using LibrarySystem.Interfaces;
using LibrarySystem.Data;
using LibrarySystem.Models;
using BCrypt.Net;

namespace LibrarySystem.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryDbContext _context;

        public MemberRepository(LibraryDbContext context){
            _context = context;
        }

        public async Task<Member> AddMember(Member member)
        {
            member.CreatedAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(member.Password);
            member.Password = passwordHash;


            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<List<Member>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<Member?> GetMemberById(int id)
        {
            return await _context.Members.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Members
                .AnyAsync(m =>
                    m.Email.ToLower() ==
                    email.ToLower());
        }
    }
}