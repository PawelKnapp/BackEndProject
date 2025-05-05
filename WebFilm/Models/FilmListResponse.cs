namespace WebFilm.Models
{
    public class FilmListResponse
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<FilmDto> Items { get; set; }
    }
}
