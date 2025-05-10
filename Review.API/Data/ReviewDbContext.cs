using Microsoft.EntityFrameworkCore;
using Review.API.Models;

namespace Review.API.Data
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }
        public DbSet<Review.API.Models.Review> Reviews { get; set; }
        public DbSet<Review.API.Models.User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review.API.Models.User>().ToTable("Users");
            modelBuilder.Entity<Review.API.Models.Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
