using System.ComponentModel.DataAnnotations;

namespace Film.API.DTOs
{
    public class AddFilmDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public int ReleaseYear { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
    }
}
