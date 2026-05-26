using BankingAPI.Contexts;
using BankingAPI.Interfaces;
using BankingAPI.Models;
using BankingAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;  // ← THIS IS MISSING - ADD THIS LINE
using System;
using System.Threading.Tasks;

namespace BankingApiTest
{
    public class Tests
    {
        IRepository<int, Customer> customerRepository;
        BankingContext bankingContext;  // Store reference for cleanup

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<BankingContext>()
                .UseInMemoryDatabase("BankingDb")
                .Options;
            bankingContext = new BankingContext(options);
            customerRepository = new Repository<int, Customer>(bankingContext);
        }

        [TearDown]
        public void TearDown()
        {
            bankingContext?.Dispose();
        }

        [Test]
        public async Task AddCustomerPassTest()
        {
            Customer customer = new Customer
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@test.com",
                DateOfBirth = new DateTime(2000, 1, 1),
                Phone = "1234567890",
                Status = "Active",
                Username = null
            };
            var result = await customerRepository.Create(customer);
           
            Assert.That(result.Id, Is.EqualTo(customer.Id));
        }

        [Test]
        public async Task GetCustomerPassTest()
        {
            // Arrange
            Customer customer = new Customer
            {
                Id = 2,
                Name = "John Doe",
                Email = "john@test.com",
                DateOfBirth = new DateTime(2000, 1, 1),
                Phone = "1234567890",
                Status = "Active",
                Username = null
            };
            var customer1 = await customerRepository.Create(customer);

            // Act
            var result = await customerRepository.Get(customer1.Id);

            // Assert
            Assert.That(result.Name, Is.EqualTo(customer.Name));
        }
    }
}