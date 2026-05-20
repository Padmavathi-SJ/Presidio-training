using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.DataAccess.Repositories;
using LibrarySystem.Business.Exceptions;

namespace LibrarySystem.Business.Services
{
    public class FineService : IFineService
    {
        private readonly IFineRepository _fineRepository;
        private readonly IMemberRepository _memberRepository;

        public FineService(IFineRepository fineRepository, IMemberRepository memberRepository)
        {
            _fineRepository = fineRepository;
            _memberRepository = memberRepository;
        }

        public async Task<Fine> GetFineByIdAsync(int id)
        {
            var fine = await _fineRepository.GetByIdAsync(id);
            if (fine == null)
                throw new NotFoundException($"Fine with ID {id} not found.");
            return fine;
        }

        public async Task<List<Fine>> GetAllFinesAsync()
        {
            return await _fineRepository.GetAllFinesAsync();
        }

        public async Task<List<Fine>> GetFinesByMemberIdAsync(int memberId)
        {
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                throw new NotFoundException($"Member with ID {memberId} not found.");
            
            return await _fineRepository.GetFinesByMemberIdAsync(memberId);
        }

        public async Task<List<Fine>> GetUnpaidFinesByMemberIdAsync(int memberId)
        {
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                throw new NotFoundException($"Member with ID {memberId} not found.");
            
            return await _fineRepository.GetUnpaidFinesByMemberIdAsync(memberId);
        }

        public async Task<List<Fine>> GetPaidFinesByMemberIdAsync(int memberId)
        {
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                throw new NotFoundException($"Member with ID {memberId} not found.");
            
            return await _fineRepository.GetPaidFinesByMemberIdAsync(memberId);
        }

        public async Task<Fine> AddFineAsync(Fine fine)
        {
            if (fine.FineAmount <= 0)
                throw new ValidationException("Fine amount must be greater than zero.");
            
            if (string.IsNullOrWhiteSpace(fine.FineReason))
                throw new ValidationException("Fine reason is required.");
            
            var member = await _memberRepository.GetById(fine.MemberId);
            if (member == null)
                throw new NotFoundException($"Member with ID {fine.MemberId} not found.");
            
            return await _fineRepository.AddFineAsync(fine);
        }

        public async Task<Fine> PayFineAsync(int fineId)
        {
            var fine = await _fineRepository.GetByIdAsync(fineId);
            if (fine == null)
                throw new NotFoundException($"Fine with ID {fineId} not found.");
            
            if (fine.PaymentStatus == FinePaymentStatus.Paid)
                throw new BusinessRuleException("FINE_ALREADY_PAID", "Fine has already been paid.");
            
            return await _fineRepository.PayFineAsync(fineId);
        }

        public async Task<FinePaymentResult> ProcessFinePaymentAsync(int memberId, int fineId, decimal paymentAmount)
        {
            var result = new FinePaymentResult();
            
            try
            {
                var fine = await _fineRepository.GetByIdAsync(fineId);
                if (fine == null)
                {
                    result.Success = false;
                    result.Message = $"Fine with ID {fineId} not found.";
                    return result;
                }
                
                if (fine.MemberId != memberId)
                {
                    result.Success = false;
                    result.Message = "This fine does not belong to the current member.";
                    return result;
                }
                
                if (fine.PaymentStatus == FinePaymentStatus.Paid)
                {
                    result.Success = false;
                    result.Message = "This fine has already been paid.";
                    return result;
                }
                
                if (paymentAmount < fine.FineAmount)
                {
                    result.Success = false;
                    result.Message = $"Payment amount (₹{paymentAmount}) is less than fine amount (₹{fine.FineAmount}). Please pay the full amount.";
                    return result;
                }
                
                var paidFine = await _fineRepository.PayFineAsync(fineId);
                
                result.Success = true;
                result.Message = $"Fine of ₹{fine.FineAmount} paid successfully!";
                result.PaidFine = paidFine;
                result.RemainingAmount = 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Payment failed: {ex.Message}";
            }
            
            return result;
        }

        public async Task<decimal> GetTotalUnpaidFineAmountByMemberAsync(int memberId)
        {
            return await _fineRepository.GetTotalUnpaidFineAmountByMemberAsync(memberId);
        }

        public async Task<bool> HasUnpaidFinesAsync(int memberId)
        {
            return await _fineRepository.HasUnpaidFinesAsync(memberId);
        }

        public async Task<List<Fine>> GetAllUnpaidFinesAsync()
        {
            return await _fineRepository.GetAllUnpaidFinesAsync();
        }

        public async Task<decimal> CalculateMemberTotalUnpaidFineWithSPAsync(int memberId)
        {
            return await _fineRepository.GetTotalUnpaidFineAmountByMemberAsync(memberId);
        }
    }
}