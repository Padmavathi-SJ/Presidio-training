using System;

namespace BankApp.Models
{
    public enum AccountType
    {
        Savings = 1, 
        Current = 2
    }

    public class Account
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (!value.Contains("@"))
                    throw new ArgumentException("Invalid email");
                _email = value;
            }
        }

        private string _phone = string.Empty;
        public string PhoneNumber
        {
            get => _phone;
            set
            {
               
                if (value.Length != 10 || !long.TryParse(value, out _))
                    throw new ArgumentException("Phone number must be 10 digits");
                _phone = value;
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Balance cannot be negative");  
                _balance = value;
            }
        }

        public DateTime DateOfBirth { get; set; }  
        public AccountType accType { get; set; }

        // Constructors
        // parameterless constructor: allows creating an empty account object
        public Account() { }

        // parameterized constructor: allows creating an account with all required information from the start
        public Account(string accountNumber, string accountHolderName, AccountType accType, DateTime dateOfBirth, string email, string phoneNumber, decimal balance)
        {
            AccountNumber = accountNumber;
            AccountHolderName = accountHolderName;
            this.accType = accType;
            DateOfBirth = dateOfBirth;
            Email = email;
            PhoneNumber = phoneNumber;
            Balance = balance;
        }

        // method overriding (polymorphism): to customize how the account objects display
        public override string ToString()
        {
            return $"Account Number: {AccountNumber}\n" +
                   $"Account Holder Name: {AccountHolderName}\n" +
                   $"Account Type: {accType}\n" +
                   $"Date of Birth: {DateOfBirth.ToShortDateString()}\n" +
                   $"Email: {Email}\n" +
                   $"Phone Number: {PhoneNumber}\n" +
                   $"Balance: {Balance:F2}";
        }
    }
}