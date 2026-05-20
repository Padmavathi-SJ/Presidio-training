using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.Business.Services
{
    public interface IMemberService
    {
        Task<Member> AddMemberAsync(Member member);
        Task<Member> GetById(int id);
        Task<List<Member>> GetAllMembersAsync();
        Task<Member> UpdateMemberAsync(int id, MembershipType membershipType, MembershipStatus membershipStatus, bool isActive);
        Task<List<Member>> GetByMembershipTypeAsync(MembershipType type);
        Task<bool> LoginAsync(string email, string password);
        
        //  Returns a tuple, not a nullable object
        Task<(bool Success, int MemberId)> LoginWithDetailsAsync(string email, string password);
        
      Task<(int Active, int Returned, int Overdue, decimal Fine)>
    GetMemberBorrowingSummaryAsync(int memberId);
    }
}