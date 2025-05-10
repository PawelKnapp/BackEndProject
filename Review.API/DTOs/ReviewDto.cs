namespace Review.API.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public int UserId { get; set; }
        public string AuthorUsername { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
