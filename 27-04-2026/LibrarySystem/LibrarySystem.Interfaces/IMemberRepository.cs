using System;
using LibrarySystem.Models;
using LibrarySystem.Data;

namespace LibrarySystem.Interfaces
{
    public interface IMemberRepository
    {
        Task<Member> AddMember(Member member);
        Task<List<Member>> GetMembers();
        Task<Member?> GetMemberById(int id);
        Task<bool> ExistsByEmail(string email);
    }
}