using System.ComponentModel.DataAnnotations;

namespace WebFilm.Models
{
    public class EditReviewDto
    {
        public int ReviewId { get; set; }

        public int FilmId { get; set; }

        [Required]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }
    }
}
