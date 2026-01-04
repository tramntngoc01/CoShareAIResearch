namespace CoShare.Domain.Models.Auth
{
    public class AdminLoginResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public static AdminLoginResult Ok(string accessToken, string refreshToken, int expiresIn)
        {
            return new AdminLoginResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                ErrorCode = null,
                ErrorMessage = null
            };
        }

        public static AdminLoginResult Fail(string errorCode, string errorMessage)
        {
            return new AdminLoginResult
            {
                Success = false,
                AccessToken = null,
                RefreshToken = null,
                ExpiresIn = 0,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}