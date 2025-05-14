using User.API.Models;


namespace Review.API.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
