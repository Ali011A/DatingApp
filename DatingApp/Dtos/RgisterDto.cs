using System.ComponentModel.DataAnnotations;

namespace DatingApp.Dtos
{
    public class RgisterDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Username must be between 3 and 50 characters.", MinimumLength = 3)]
        public  string Username { get; set; }
        [Required]
        [EmailAddress] // optional validation
        public string Email { get; set; }
        [Required]
        public string? KnownAs { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required]
        public DateTime? DateOfBirth { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? Country { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Password must be between 3 and 50 characters.", MinimumLength = 6)]
        public  string Password { get; set; }

    }
}
