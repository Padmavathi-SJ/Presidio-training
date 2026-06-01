using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.DataAccess.Repositories;

namespace LibrarySystem.Business.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IBorrowingRulesService _borrowingRulesService;

        public MemberService(
            IMemberRepository memberRepository, IBorrowingRulesService borrowingRulesService)
        {
            _memberRepository = memberRepository;
            _borrowingRulesService = borrowingRulesService;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
             if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("email id is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password is required.");
                return false;
            } 

            // get member
            var member = await _memberRepository.GetByEmail(email);
            if(member == null)
            {
                Console.WriteLine("member not found.");
                return false;
            }
            // verify password
            if (member.Password != password)
            {
                Console.WriteLine("Invalid password.");
                return false;
            }
             // check active
            if (!member.IsActive)
            {
                Console.WriteLine("member account inactive.");
                return false;
            }

            return true;

        }

public async Task<Member> GetById(int id)
{
    var member = await _memberRepository.GetById(id);
    if (member == null)
        throw new Exception($"Member with ID {id} not found.");
    return member;
}

public async Task<(bool Success, int MemberId)> LoginWithDetailsAsync(string email, string password)
{
    if (string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("Email id is required.");
        return (false, 0);
    }

    if (string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("Password is required.");
        return (false, 0);
    }

    var member = await _memberRepository.GetByEmail(email);
    if (member == null)
    {
        Console.WriteLine("Member not found.");
        return (false, 0);
    }
    
    if (member.Password != password)
    {
        Console.WriteLine("Invalid password.");
        return (false, 0);
    }
    
    if (!member.IsActive)
    {
        Console.WriteLine("Member account is inactive.");
        return (false, 0);
    }

    return (true, member.Id);
}
     
        public async Task<Member> AddMemberAsync(Member member)
        {
            if (member == null)
            {
                throw new Exception(
                    "Member data is required.");
            }

            if (string.IsNullOrWhiteSpace(member.Name))
            {
                throw new Exception(
                    "Member name is required.");
            }

            if (string.IsNullOrWhiteSpace(member.Email))
            {
                throw new Exception(
                    "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(member.PhoneNum))
            {
                throw new Exception(
                    "Phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(member.Password))
            {
                throw new Exception(
                    "Password is required.");
            }

            bool exists =
                await _memberRepository
                    .ExistsByEmail(member.Email);

            if (exists)
            {
                throw new Exception(
                    "Email already exists.");
            }

            // SET borrowing limits based on membership type
            int maxBorrowings = await _borrowingRulesService.GetMaxBorrowingsAsync(member.MembershipType);
            member.AllowedBorrowingCount = maxBorrowings;
            member.CurrentBorrowedCount = 0;

            return await _memberRepository
                .AddMember(member);
        }


        public async Task<List<Member>>
            GetAllMembersAsync()
        {
            var members =
                await _memberRepository.GetAll();

            if (!members.Any())
            {
                throw new Exception(
                    "No members found.");
            }

            return members;
        }

        public async Task<Member>
            UpdateMemberAsync(
                int id,
                MembershipType membershipType,
                MembershipStatus membershipStatus,
                bool isActive)
        {
            if (id <= 0)
            {
                throw new Exception(
                    "Invalid member id.");
            }

            var member =
                await _memberRepository.GetById(id);

            if (member == null)
            {
                throw new Exception(
                    "Member not found.");
            }

            //if membership type changed , update allowed borrowing count
            if(member.MembershipType != membershipType)
            {
                int maxBorrowings = await _borrowingRulesService.GetMaxBorrowingsAsync(membershipType);
                member.AllowedBorrowingCount = maxBorrowings;
            }

            member.MembershipType =
                membershipType;

            member.MembershipStatus =
                membershipStatus;

            member.IsActive =
                isActive;

            member.UpdatedAt = DateTime.UtcNow;

            return await _memberRepository
                .UpdateMember(member);
        }

        public async Task<List<Member>>
            GetByMembershipTypeAsync(
                MembershipType type)
        {
            var members =
                await _memberRepository
                    .GetByMembershipType(type);

            if (!members.Any())
            {
                throw new Exception(
                    "No members found.");
            }

            return members;
        }


public async Task<(int Active, int Returned, int Overdue, decimal Fine)>
    GetMemberBorrowingSummaryAsync(int memberId)
{
    var member = await _memberRepository.GetById(memberId);

    if (member == null)
    {
        throw new Exception(
            $"Member with ID {memberId} not found.");
    }

    return await _memberRepository
        .GetMemberBorrowingSummaryWithSPAsync(memberId);
}
    
    }
}