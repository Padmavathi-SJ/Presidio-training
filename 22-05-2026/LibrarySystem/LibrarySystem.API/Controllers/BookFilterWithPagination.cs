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
    public class BookFilterWithPaginationController : ControllerBase
    {
        private readonly IBookFilterWithPagination _bookFilterWithPaginationService;
        public BookFilterWithPaginationController(IBookFilterWithPagination bookFilterWithPaginationService)
        {
            _bookFilterWithPaginationService = bookFilterWithPaginationService;
        }

        [HttpGet("publication-year-range-with-pagination")]
        public async Task<IActionResult> GetBooksByRangeWithPagination(
            [FromQuery] int? fromPublicationYear = null,
            [FromQuery] int? toPublicationYear = null,
            [FromQuery] string? title = null,
            [FromQuery] string? author = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                if(pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Page size must be between 1 and 100"
                    });
                }
                if(fromPublicationYear.HasValue && toPublicationYear.HasValue)
                {
                    if(fromPublicationYear > toPublicationYear)
                    {
                        return BadRequest(new
                    {
                        success = false,
                        message = "frompublication year should not be greater than topublicationyear"
                    });
                    }
                }

                //ceate filter DTO
                var filter = new BookFilterWithPaginationDTO
                {
                    FromPublicationYear = fromPublicationYear,
                    ToPublicationYear = toPublicationYear,
                    Title = title,
                    Author = author,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                
                var books = await _bookFilterWithPaginationService.GetBooksWithPaginationAsync(filter);

                if(books == null || books.Items == null || books.Items.Count == 0)
                {
                    return NotFound(new { message = "Books not found in the range of given publication years"});
                }

                var response = new
                {
                    success = true,
                    message = $"books fetched, totaly {books.TotalCount} books fetched",
                    data = books.Items,
                    pagination = new
                    {
                        books.TotalCount,
                        books.PageNumber,
                        books.PageSize,
                        books.TotalPages,
                        books.HasPreviousPage,
                        books.HasNextPage
                        
                    },
                    filters = new
                    {
                        FromPublicationYear = fromPublicationYear,
                        ToPublicationYear = toPublicationYear,
                        Title = string.IsNullOrEmpty(title) ? "not applied" : title,
                        Author = string.IsNullOrEmpty(author) ? "not applied" : author
                    }

                };
                return Ok(books);
            
            }
            catch (Exception ex)
            {
                 return StatusCode(500, new
                {
                    success = false,
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }
    }
}