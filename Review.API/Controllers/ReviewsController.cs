using Microsoft.AspNetCore.Mvc;
using Review.API.Data;
using Review.API.Models;
using Review.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Review.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewDbContext _context;

        public ReviewsController(ReviewDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] int? filmId)
        {
            var query = _context.Reviews.AsQueryable();
            if (filmId.HasValue)
                query = query.Where(r => r.FilmId == filmId.Value);

            var reviews = query.ToList();
            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] AddReviewDto dto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Nieprawidłowy identyfikator użytkownika w tokenie.");

            var review = new Review.API.Models.Review
            {
                FilmId = dto.FilmId,
                UserId = userId,
                Rating = dto.Rating,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Edit(int id, [FromBody] EditReviewDto dto)
        {
            var review = _context.Reviews.Find(id);
            if (review == null)
                return NotFound();

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Nieprawidłowy identyfikator użytkownika w tokenie.");

            if (review.UserId != userId)
                return Forbid();

            review.Rating = dto.Rating;
            review.Content = dto.Content;
            _context.SaveChanges();
            return Ok(review);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null)
                return NotFound();

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Nieprawidłowy identyfikator użytkownika w tokenie.");

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (review.UserId != userId && userRole != "Admin")
                return Forbid();

            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
