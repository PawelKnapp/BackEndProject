using Microsoft.AspNetCore.Mvc;
using Review.API.Data;
using Review.API.Models;
using Review.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult GetAll([FromQuery] int? filmId, [FromQuery] int page = 1, [FromQuery] int pageSize = 5,
                            [FromQuery] string sortBy = "date", [FromQuery] string sortOrder = "desc")
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .AsQueryable();

            if (filmId.HasValue)
                query = query.Where(r => r.FilmId == filmId.Value);

            var totalItems = query.Count();
            double averageRating = totalItems > 0 ? query.Average(r => r.Rating) : 0;

            query = sortBy switch
            {
                "rating" => sortOrder == "asc" ? query.OrderBy(r => r.Rating) : query.OrderByDescending(r => r.Rating),
                _ => sortOrder == "asc" ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt)
            };

            var reviews = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    FilmId = r.FilmId,
                    UserId = r.UserId,
                    AuthorUsername = r.User.Username,
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            return Ok(new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                AverageRating = averageRating,
                Items = reviews
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var review = _context.Reviews
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == id);

            if (review == null)
                return NotFound();

            var dto = new ReviewDto
            {
                Id = review.Id,
                FilmId = review.FilmId,
                UserId = review.UserId,
                AuthorUsername = review.User?.Username,
                Rating = review.Rating,
                Content = review.Content,
                CreatedAt = review.CreatedAt
            };

            return Ok(dto);
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
