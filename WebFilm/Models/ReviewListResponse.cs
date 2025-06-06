﻿namespace WebFilm.Models
{
    public class ReviewListResponse
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public double AverageRating { get; set; }
        public List<ReviewDto> Items { get; set; } = new List<ReviewDto>();
    }
}
