namespace Review.API.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public string Author { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
