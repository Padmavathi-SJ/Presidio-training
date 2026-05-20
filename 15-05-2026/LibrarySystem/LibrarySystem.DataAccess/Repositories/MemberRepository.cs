using Microsoft.EntityFrameworkCore;

using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public MemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Member> AddMember(Member member)
        {
            await _context.Members.AddAsync(member);

            await _context.SaveChangesAsync();

            return member;
        }

        public async Task<List<Member>> GetAll()
        {
            return await _context.Members
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Member?> GetById(int id)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Member?> GetByEmail(string email)
        {
            return await _context.Members
                .FirstOrDefaultAsync(
                    m => m.Email.ToLower() ==
                    email.ToLower());
        }

        public async Task<List<Member>>
            GetByMembershipType(MembershipType type)
        {
            return await _context.Members
                .Where(m => m.MembershipType == type)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Members
                .AnyAsync(m =>
                    m.Email.ToLower() ==
                    email.ToLower());
        }

        public async Task<Member> UpdateMember(Member member)
        {
            _context.Members.Update(member);

            await _context.SaveChangesAsync();

            return member;
        }

public async Task<(int Active, int Returned, int Overdue, decimal Fine)>
    GetMemberBorrowingSummaryWithSPAsync(int memberId)
{
    try
    {
        using var command =
            _context.Database.GetDbConnection().CreateCommand();

        command.CommandText =
            "SELECT * FROM get_member_borrowing_summary(@p_member_id)";

        command.CommandType =
            System.Data.CommandType.Text;

        var parameter = command.CreateParameter();

        parameter.ParameterName = "@p_member_id";

        parameter.Value = memberId;

        command.Parameters.Add(parameter);

        if (_context.Database.GetDbConnection().State
            != System.Data.ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync();
        }

        using var reader =
            await command.ExecuteReaderAsync();

        int active = 0;
        int returned = 0;
        int overdue = 0;
        decimal fine = 0;

        if (await reader.ReadAsync())
        {
            active = reader.GetInt32(0);
            returned = reader.GetInt32(1);
            overdue = reader.GetInt32(2);
            fine = reader.GetDecimal(3);
        }

        await _context.Database.CloseConnectionAsync();

        return (active, returned, overdue, fine);
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Error getting summary: {ex.Message}");

        return (0, 0, 0, 0);
    }
}
  
  
    }
}