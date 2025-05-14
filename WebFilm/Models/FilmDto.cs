using System.ComponentModel.DataAnnotations;

namespace WebFilm.Models
{
    public class FilmDto
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public int ReleaseYear { get; set; }
        [Required]
        public string Genre { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
