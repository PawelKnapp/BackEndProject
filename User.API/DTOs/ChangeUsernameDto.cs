namespace User.API.DTOs
{
    public class ChangeUsernameDto
    {
        public string NewUsername { get; set; }
        public string CurrentPassword { get; set; }
    }
}
