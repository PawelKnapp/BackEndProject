using Microsoft.EntityFrameworkCore;
using Review.API.Models;

namespace Review.API.Data
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }
        public DbSet<Review.API.Models.Review> Reviews { get; set; }
    }
}