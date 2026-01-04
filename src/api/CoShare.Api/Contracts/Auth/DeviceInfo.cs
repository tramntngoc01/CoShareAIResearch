namespace CoShare.Api.Contracts.Auth;

/// <summary>
/// Device/browser fingerprint for token binding.
/// Maps to OpenAPI: DeviceInfo
/// </summary>
public sealed record DeviceInfo
{
    public string? DeviceId { get; init; }
    public string? UserAgent { get; init; }
    public string? Platform { get; init; }
}
