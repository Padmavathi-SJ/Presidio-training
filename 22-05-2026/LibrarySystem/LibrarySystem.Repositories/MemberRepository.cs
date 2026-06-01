using Microsoft.EntityFrameworkCore;
using System;
using LibrarySystem.Interfaces;
using LibrarySystem.Data;
using LibrarySystem.Models;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<MemberRepository> _logger;

        public MemberRepository(LibraryDbContext context, ILogger<MemberRepository> logger){
            _context = context;
            _logger = logger;
        }

        public MemberRepository(LibraryDbContext context, ILogger<MemberRepository> logger, ILogger<MemberRepository> logger2)
        {
            _context = context;
            _logger = logger2;
        }

        public async Task<Member?> AddMember(Member member)
        {
            try
            {
                
            member.CreatedAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(member.Password);
            member.Password = passwordHash;


            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            _logger.LogInformation(" Adding member with email {Email} to the database", member.Email);
            return member;
        }
        catch(Exception ex)
            {
                _logger.LogError(ex, "Error adding member with email {Email} to the database", member.Email);
                throw; // rethrow the exception to be handled by the calling code
            }

        }

        public async Task<List<Member>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<Member?> GetMemberById(int id)
        {
            try
            {
                
                 _logger.LogInformation("Retrived the member with id - {Id} from the database", id);
                  return await _context.Members.FirstOrDefaultAsync(m => m.Id == id);
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Error retriving the member with id - {Id} from the database", id);
                throw;
            }
           
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