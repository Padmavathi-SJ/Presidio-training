using System;
using LibrarySystem.Repositories;
using LibrarySystem.Interfaces;
using LibrarySystem.Models;
using LibrarySystem.Data;
using LibrarySystem.Exceptions;

namespace LibrarySystem.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<Member?> GetByIdAsync(int id)
        {
            var member = await _memberRepository.GetMemberById(id);
            if (member == null)
        throw new Exception($"Member with ID {id} not found.");
    return member;
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
            bool exists =
                await _memberRepository
                    .ExistsByEmail(member.Email);

            if (exists)
            {
                throw new Exception(
                    "Email already exists.");
            }

            return await _memberRepository
                .AddMember(member);
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            var members =
                await _memberRepository.GetMembers();

            if (!members.Any())
            {
                throw new Exception(
                    "No members found.");
            }

            return members;
        }
    }
}