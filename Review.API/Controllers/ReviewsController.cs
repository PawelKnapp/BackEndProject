using Microsoft.AspNetCore.Mvc;
using Review.API.Data;
using Review.API.Models;
using Review.API.DTOs;

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
        public IActionResult GetAll()
        {
            var reviews = _context.Reviews.ToList();
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
        public IActionResult Add([FromBody] AddReviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var review = new Review.API.Models.Review
            {
                FilmId = dto.FilmId,
                Author = dto.Author,
                Rating = dto.Rating,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null) return NotFound();
            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
