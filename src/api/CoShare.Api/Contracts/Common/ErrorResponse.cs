namespace CoShare.Api.Contracts.Common;

/// <summary>
/// Standard error response envelope per Error-Conventions.
/// </summary>
public sealed record ErrorResponse
{
    public required ErrorDetail Error { get; init; }
}

public sealed record ErrorDetail
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string CorrelationId { get; init; }
    public Dictionary<string, string[]>? FieldErrors { get; init; }
}
