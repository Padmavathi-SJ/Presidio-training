using Microsoft.AspNetCore.Mvc;
using System;
using LibrarySystem.Models;
using LibrarySystem.Services;
using LibrarySystem.Data;
using LibrarySystem.Interfaces;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        //get all members GET - api/members
        [HttpGet]
        public async Task<IActionResult> GetAllMembers()
        {
            try
            {
                var members = await _memberService.GetAllMembersAsync();
                return Ok(members);
            } catch(Exception ex)
            {
                return StatusCode(500, new {message = $"An error occured: {ex.Message}"});
            }
        }

        // GET: api/members/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var member = await _memberService.GetByIdAsync(id);
                if(member == null)
                {
                    return NotFound(new {message = "member not found!"}
                    );
                }
                return Ok(member);
            } catch(Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
            }


            //POST : api/member
            [HttpPost]
            public async Task<IActionResult> CreateMember([FromBody] Member member)
        {
             try
            {
                var result = await _memberService.AddMemberAsync(member);
                return CreatedAtAction(nameof(GetById), new {id = result.Id}, result); 
            } catch(Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }
        }

        
            }
