using System;
using System.Collections.Generic; // for list of accounts
using BankApp.Models; // to use the account class and accounttype enum from models
using System.Linq; // for LINQ queries to search accounts

namespace BankApp.Services{
    public class AccountServices{
        private List<Account> accounts = new List<Account>();
        private long nextAccountNumber = 1000000000; 
        
        // create account method 
    }
}