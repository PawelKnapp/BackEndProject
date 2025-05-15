using Microsoft.AspNetCore.Mvc;
using User.API.Data;
using global::User.API.Models;
using Microsoft.AspNetCore.Authorization;
using User.API.DTOs;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _context;

        public UsersController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public IActionResult Add([FromBody] User.API.Models.User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/role")]
        public IActionResult ChangeRole(int id, [FromBody] ChangeRoleDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            user.Role = dto.NewRole;
            _context.SaveChanges();
            return Ok(new { user.Id, user.Username, user.Role });
        }

    }
}
