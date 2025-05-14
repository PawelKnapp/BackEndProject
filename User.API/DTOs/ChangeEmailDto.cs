namespace User.API.DTOs
{
    public class ChangeEmailDto
    {
        public string NewEmail { get; set; }
        public string CurrentPassword { get; set; }
    }
}
