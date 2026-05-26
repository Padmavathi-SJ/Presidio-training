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
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // get all books -> GET : api/book
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync();
                return Ok(books);
            } catch(Exception ex)
            {
                return StatusCode(500, new {message = $"An error occured: {ex.Message}"});
            }
        }

        // GET: api/book/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            try
            {
                var book = await _bookService.GetByIdAsync(id);
                if(book == null)
                {
                    return NotFound(new {message = "book not found!"}
                    );
                }
                return Ok(book);
            } catch(Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
            }

        // POST : api/book
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            try
            {
                var result = await _bookService.AddBookAsync(book);
                return CreatedAtAction(nameof(GetBookById), new {id = result.Id}, result); 
            } catch(Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }
        }
    }
