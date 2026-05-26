using BankingAPI.Interfaces;
using BankingAPI.Services;
using BankingAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BankingAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    // api/transaction
    public class TransactionController : ControllerBase
    {
        private readonly ITransaction _transactionService;
        public TransactionController(ITransaction transactionService)
        {
            _transactionService = transactionService;
        }
    

    //deposit money into an account
    [HttpPost("deposit")] // /api/transaction/deposit
    public IActionResult Deposit([FromBody] DepositRequest request)
        {
            if(request == null)
            {
                return BadRequest(new
                {
                    message = "Invalid request"
                });
            }
            if (string.IsNullOrWhiteSpace(request.ToAccountNumber))
            {
                return BadRequest(new { message = "Account number is required"});
            }

            if(request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero"});
            }

            try
            {
                var result = _transactionService.Deposit(request);
                return Ok(new
                {
                    success = true,
                    message = "Deposit completed successfully,",
                    data = result
                });
            } catch(ArgumentException ex)
            {
                return NotFound(new
                {
                    success = false, message = ex.Message
                });
            } catch(Exception ex)
            {
                return StatusCode(500, new {message = $"An error occured: {ex.Message}"});
            }
        }

        [HttpPost("withdraw")] // api/transaction/withdraw
        public IActionResult Withdraw([FromBody] WithdrawRequest request)
        {
            if(request == null)
            {
                return BadRequest(new
                {
                    message = "Invalid request"
                });
            }

             if (string.IsNullOrWhiteSpace(request.FromAccountNumber))
            {
                return BadRequest(new { message = "Account number is required"});
            }

            if(request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero"});
            }

            try
            {
                var result = _transactionService.Withdraw(request);
                return Ok(new
                {
                    success = true,
                    message = "Withdraw completed successfully,",
                    data = result
                });
            } catch(ArgumentException ex)
            {
                return NotFound(new {success = false, message = ex.Message});
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false, 
                    message = ex.Message
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }

        }

        
    }
}