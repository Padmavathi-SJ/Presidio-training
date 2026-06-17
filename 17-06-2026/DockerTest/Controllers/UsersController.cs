using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DockerTest.Data;
using DockerTest.Models;

namespace DockerTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            // Validate email uniqueness
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return Conflict($"User with email {user.Email} already exists.");
            }

            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Update fields
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.Age = user.Age;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.City = user.City;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/users/search?name=john
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers([FromQuery] string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return await _context.Users.ToListAsync();
            }

            var users = await _context.Users
                .Where(u => u.FirstName.Contains(name) || u.LastName.Contains(name))
                .ToListAsync();

            return users;
        }
    }
}