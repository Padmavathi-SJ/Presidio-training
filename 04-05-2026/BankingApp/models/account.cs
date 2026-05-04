using system;
using System.Collections.generic;


namespace BankingApp.Models{
    public enum AccType {
        SavingAccount = 1,
        CurrentAccount = 2
    }
    // Encapsulation: 'internal class account' -> the account class is only accessible within your backingApp project. 
    // to protect bank data being accessed by external code.
    // use encapsulation when I want to control who can see and modfy my class.
    internal class Account{
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName {get; set; } = string.Empty;
        public AccType AccountType { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string phoneNumber { get;  set; } = string.Empty;
        public decimal Balance { get; set; }
        
        // Constructos:
        // parameterless constructor: allows to creating empty account object (useful for later filling data)
    
        public Account(){

        }

        // parameterized constructor: allows to create accoiunt with all required information from the start.
        public Account(string accountNumber, string accountHolderName, AccType accountType, DateTime dateofBirth, string email, string phoneNumber, decimal balance){
            AccountNumber = accountNumber;
            AccountHolderName = accountHolderName;
            AccountType = accountType;
            DateOfBirth = dateofBirth;
            Email = email;
            phoneNumber = phoneNumber;
            Balance = balance;
        } // constructor overloading gives flexibility - create accounts with or without initial data
        
        //method overriding(polymorphism): to customize how the account objects display as strings.
        // when i want to meaingful text representation instead of default bankingApp.Models.Account
        public override string ToString(){
            return $"Account Number: {AccountNumber}\n 
            Account Holder Name: {AccountHolderName}\n
            Account Type: {AccountType}\n
            Email: {Email}\n
            Phone Number: {phoneNumber}\n
            Balance: {Balance}
            ";
        }
    }
}