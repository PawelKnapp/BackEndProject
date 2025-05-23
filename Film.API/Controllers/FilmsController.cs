﻿using Microsoft.AspNetCore.Mvc;
using Film.API.Data;
using Film.API.Models;
using Film.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Film.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmsController : ControllerBase
    {
        private readonly FilmDbContext _context;

        public FilmsController(FilmDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string genre = null,
            [FromQuery] string sortBy = "Title",
            [FromQuery] string sortOrder = "asc")
        {
            var query = _context.Films.AsQueryable();

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(f => f.Genre == genre);

            query = sortBy switch
            {
                "ReleaseYear" => sortOrder == "desc" ? query.OrderByDescending(f => f.ReleaseYear) : query.OrderBy(f => f.ReleaseYear),
                "Genre" => sortOrder == "desc" ? query.OrderByDescending(f => f.Genre) : query.OrderBy(f => f.Genre),
                _ => sortOrder == "desc" ? query.OrderByDescending(f => f.Title) : query.OrderBy(f => f.Title)
            };

            var totalItems = query.Count();
            var films = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var result = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                Items = films
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var film = _context.Films.Find(id);
            if (film == null) return NotFound();
            return Ok(film);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] AddFilmDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var film = new Film.API.Models.Film
            {
                Title = dto.Title,
                ReleaseYear = dto.ReleaseYear,
                Genre = dto.Genre,
                Description = dto.Description
            };

            _context.Films.Add(film);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = film.Id }, film);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] Film.API.Models.Film updatedFilm)
        {
            var film = _context.Films.Find(id);
            if (film == null) return NotFound();
            film.Title = updatedFilm.Title;
            film.ReleaseYear = updatedFilm.ReleaseYear;
            film.Genre = updatedFilm.Genre;
            film.Description = updatedFilm.Description;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var film = _context.Films.Find(id);
            if (film == null) return NotFound();
            _context.Films.Remove(film);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
