using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password", ErrorMessage = "Both passwords must match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
