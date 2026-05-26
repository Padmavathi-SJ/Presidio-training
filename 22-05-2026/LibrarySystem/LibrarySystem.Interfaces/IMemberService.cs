using System;
using LibrarySystem.Interfaces;
using LibrarySystem.Repositories;
using LibrarySystem.Models;

namespace LibrarySystem.Interfaces
{
    public interface IMemberService
    {
        Task<Member> AddMemberAsync(Member member);
        Task<List<Member>> GetAllMembersAsync();
        Task<Member?> GetByIdAsync(int id);
    }
}
