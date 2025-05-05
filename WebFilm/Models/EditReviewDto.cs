namespace WebFilm.Models
{
    public class EditReviewDto
    {
        public int ReviewId { get; set; }
        public int FilmId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
    }
}
