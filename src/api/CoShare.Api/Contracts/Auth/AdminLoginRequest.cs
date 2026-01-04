using System.ComponentModel.DataAnnotations;

namespace CoShare.Api.Contracts.Auth
{
    public class AdminLoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}