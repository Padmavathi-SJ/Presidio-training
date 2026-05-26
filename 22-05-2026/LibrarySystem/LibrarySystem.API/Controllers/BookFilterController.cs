using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Services;
using LibrarySystem.Models;
using LibrarySystem.Interfaces;
using LibrarySystem.Data;
using LibrarySystem.DTOs;
using System;

namespace LibrarySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookFilterController : ControllerBase
    {
        private readonly IBookFilter _bookFilterService;
        public BookFilterController(IBookFilter bookFilterService)
        {
            _bookFilterService = bookFilterService;
        }

        [HttpGet("publication-year-range")]
        public async Task<IActionResult> GetBooksByRange([FromQuery] int? fromPublicationYear, [FromQuery] int? toPublicationYear)
        {
            try
            {
                var request = new BookFilterDTO
                {
                    FromPublicationYear = fromPublicationYear,
                    ToPublicationYear = toPublicationYear
                };

            var books = await _bookFilterService.GetBooksByPublicationYearRange(request);
            if(books == null || books.Count == 0)
            {
                return NotFound(new { message = "Books not found in the range of given publication years"});
            }
            return Ok(books);
        } catch(Exception ex)
            {
                return StatusCode(500, new {message = $"An error occured: {ex.Message}"});
            }

    }
}
}