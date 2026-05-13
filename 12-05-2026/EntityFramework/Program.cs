using EntityFramework.Contexts;  // Change to your actual namespace
using EntityFramework.Models;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Entity Framework Demo Starting...");
        
        // Test the database connection
        using (var context = new BankingContext())
        {
            Console.WriteLine("Database context created successfully!");
            
            // Optional: Add a test customer
            var customer = new Customer
            {
                Name = "Test Customer",
                Phone = "1234567890",
                Email = "test@example.com",
                DateOfBirth = new DateTime(1990, 1, 1)
            };
            
            context.customers.Add(customer);
            context.SaveChanges();
            
            Console.WriteLine($"Customer added with ID: {customer.Id}");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}