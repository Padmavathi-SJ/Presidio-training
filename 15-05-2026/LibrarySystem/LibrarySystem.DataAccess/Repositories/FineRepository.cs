using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Repositories
{
    public class FineRepository : IFineRepository
    {
        private readonly ApplicationDbContext _context;

        public FineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Fine> GetByIdAsync(int id)
        {
            return await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.Borrowing)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<Fine>> GetAllFinesAsync()
        {
            return await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.Borrowing)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Fine>> GetFinesByMemberIdAsync(int memberId)
        {
            return await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.Borrowing)
                .Where(f => f.MemberId == memberId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Fine>> GetUnpaidFinesByMemberIdAsync(int memberId)
        {
            return await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.Borrowing)
                .Where(f => f.MemberId == memberId && 
                           f.PaymentStatus == FinePaymentStatus.Pending &&
                           f.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Fine>> GetPaidFinesByMemberIdAsync(int memberId)
        {
            return await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.Borrowing)
                .Where(f => f.MemberId == memberId && 
                           f.PaymentStatus == FinePaymentStatus.Paid)
                .OrderByDescending(f => f.PaymentDate)
                .ToListAsync();
        }

        public async Task<Fine> AddFineAsync(Fine fine)
        {
            fine.CreatedAt = DateTime.UtcNow;
            fine.UpdatedAt = DateTime.UtcNow;
            
            await _context.Fines.AddAsync(fine);
            await _context.SaveChangesAsync();
            return fine;
        }

        public async Task<Fine> UpdateFineAsync(Fine fine)
        {
            fine.UpdatedAt = DateTime.UtcNow;
            _context.Fines.Update(fine);
            await _context.SaveChangesAsync();
            return fine;
        }

        public async Task<Fine> PayFineAsync(int fineId)
        {
            var fine = await GetByIdAsync(fineId);
            if (fine == null)
                throw new Exception($"Fine with ID {fineId} not found.");
            
            if (fine.PaymentStatus == FinePaymentStatus.Paid)
                throw new Exception("Fine has already been paid.");
            
            fine.PaymentStatus = FinePaymentStatus.Paid;
            fine.PaymentDate = DateTime.UtcNow;
            fine.UpdatedAt = DateTime.UtcNow;
            
            _context.Fines.Update(fine);
            await _context.SaveChangesAsync();
            
            return fine;
        }

      public async Task<decimal> GetTotalUnpaidFineAmountByMemberAsync(int memberId)
{
    try
    {
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT calculate_member_fine(@p_member_id)";
        command.CommandType = System.Data.CommandType.Text;
        
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@p_member_id";
        parameter.Value = memberId;
        command.Parameters.Add(parameter);
        
        if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync();
        }
        
        var result = await command.ExecuteScalarAsync();
        
        await _context.Database.CloseConnectionAsync();
        
        return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error calculating fine: {ex.Message}");
        return 0;
    }
}

        public async Task<bool> HasUnpaidFinesAsync(int memberId)
        {
            return await _context.Fines
                .AnyAsync(f => f.MemberId == memberId && 
                              f.PaymentStatus == FinePaymentStatus.Pending &&
                              f.IsActive);
        }

        public async Task<List<Fine>> GetAllUnpaidFinesAsync()
        {
            return await _context.Fines
                .Include(f => f.Member)
                .Include(f => f.Borrowing)
                .Where(f => f.PaymentStatus == FinePaymentStatus.Pending && f.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}