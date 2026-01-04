namespace CoShare.Api.Contracts.Auth
{
    public class AdminLoginResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }
}