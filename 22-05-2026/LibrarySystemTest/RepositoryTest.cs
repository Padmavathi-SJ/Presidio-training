using LibrarySystem.Data;
using LibrarySystem.Interfaces;
using LibrarySystem.Models;
using LibrarySystem.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LibrarySystemTest
{
    public class Tests
    {
        IMemberRepository memberRepository;
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>().UseInMemoryDatabase("LibraryDb").Options;
            LibraryDbContext libraryDbContext = new LibraryDbContext(options);
            memberRepository = new MemberRepository(libraryDbContext);

        }

        [Test]
        public async Task AddMemberPassTest()
        {
            Member member = new Member
            {
                Id=5,
                Name = "elia",
                Email = "elia@gmail.com",
                PhoneNum = "9246578120",
                Password = "elia@123"
            };
            var result = await memberRepository.AddMember(member);
            Assert.That(result.Id, Is.EqualTo(member.Id));
        }
    }
}