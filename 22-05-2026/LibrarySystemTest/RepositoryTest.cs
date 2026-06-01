using LibrarySystem.Data;
using LibrarySystem.Interfaces;
using LibrarySystem.Models;
using LibrarySystem.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

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
            
            // creates a fake logger for testing puposes, 
            // they are not printed, not stored permanently and just mocked
            // 
            var loggerMock = new Mock<ILogger<MemberRepository>>();

            // I can print the log to console for debugging  when running the test, 
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<MemberRepository>();

            memberRepository = new MemberRepository(libraryDbContext, loggerMock.Object, logger);

        }

        [Test]
        public async Task AddMemberPassTest()
        {
            Member member = new Member
            {
                Id=6,
                Name = "elian",
                Email = "elian@gmail.com",
                PhoneNum = "9246578120",
                Password = "elian@123"
            };
            /*
            Member member2 = new Member
            {
                Id=5,
                Name = "elia",
                Email = "elia@gmail.com",
                PhoneNum = "9246578120",
                Password = "elia@123"
            };
*/
            var result = await memberRepository.AddMember(member);
       //     var result2 = await memberRepository.AddMember(member2);

            Assert.That(result.Id, Is.EqualTo(member.Id));
           // Assert.That(result2.Id, Is.EqualTo(member2.Id));

        }

        [Test]
        public async Task GetCustomerPassTest()
        {
            Member member = new Member
            {
                Id=7,
                Name = "falia",
                Email = "falia@gmail.com",
                PhoneNum = "9246578120",
                Password = "falia@123"
            };
            var member1 = await memberRepository.AddMember(member);

            var result = await memberRepository.GetMemberById(member1.Id);

            //Assert
            Assert.That(result.Id, Is.EqualTo(member1.Id));
        }


    }
}