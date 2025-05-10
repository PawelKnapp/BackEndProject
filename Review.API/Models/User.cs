using System.ComponentModel.DataAnnotations.Schema;


namespace Review.API.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }
}
