using Microsoft.EntityFrameworkCore;
using Film.API.Models;

namespace Film.API.Data
{
    public class FilmDbContext : DbContext
    {
        public FilmDbContext(DbContextOptions<FilmDbContext> options) : base(options) { }
        public DbSet<Film.API.Models.Film> Films { get; set; }
    }
}
