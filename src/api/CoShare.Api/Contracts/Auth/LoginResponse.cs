namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Standard auth tokens response for both end-user and admin logins.
/// Maps to OpenAPI: LoginResponse
/// </summary>
public sealed record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public EndUserProfileSummary? User { get; init; }
    public AdminProfileSummary? Admin { get; init; }
    public List<string>? Roles { get; init; }
}

/// <summary>
/// Summary of an end user profile included in login response.
/// Maps to OpenAPI: EndUserProfileSummary
/// </summary>
public sealed record EndUserProfileSummary
{
    public required long UserId { get; init; }
    public required string FullName { get; init; }
    public required string Phone { get; init; }
    public required long CompanyId { get; init; }
    public required int Tier { get; init; }
}

/// <summary>
/// Summary of an admin profile included in login response.
/// Maps to OpenAPI: AdminProfileSummary
/// </summary>
public sealed record AdminProfileSummary
{
    public required long AdminUserId { get; init; }
    public required string LoginId { get; init; }
}
