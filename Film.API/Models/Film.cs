namespace Film.API.Models
{
    public class Film
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
    }
}
