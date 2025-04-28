using System.ComponentModel.DataAnnotations;

namespace Review.API.DTOs
{
    public class AddReviewDto
    {
        [Required]
        public int FilmId { get; set; }

        [Required]
        [StringLength(50)]
        public string Author { get; set; }

        [Required]
        [Range(1, 10)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
    }
}
