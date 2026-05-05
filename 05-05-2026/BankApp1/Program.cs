using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnderstandingOOPSApp.Models;
using UnderstandingOOPSApp.Repositories;
using UnderstandingOOPSApp.Interfaces;

namespace UnderstandingOOPSApp
{
    ICustomerRepository customerInteract;

    class Program
    {
        customerInteract = new CustomerService();
    }

    static void main(string[] args){
        AccountRepository accountRepo = new AccountRepository();
        var acc = accountRepo.Create(new Models.Account)
    }
}