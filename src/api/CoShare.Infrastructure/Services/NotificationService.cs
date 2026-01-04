using CoShare.Domain.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoShare.Infrastructure.Services;

/// <summary>
/// Stub implementation of INotificationService for NOTIFICATIONS module integration.
/// In production, this will call the actual NOTIFICATIONS API.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IHostEnvironment _env;

    public NotificationService(ILogger<NotificationService> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public Task<string?> SendOtpAsync(
        string phone,
        string otpCode,
        string templateCode,
        int ttlSeconds,
        string correlationId,
        CancellationToken ct = default)
    {
        // In production, this would call POST /api/v1/notifications/otp/send
        // For now, return a mock message ID and log (without OTP code per Security-Baseline)
        var messageId = Guid.NewGuid().ToString();

        // DEV ONLY: Log OTP for testing purposes
        if (_env.IsDevelopment())
        {
            _logger.LogWarning(
                "⚠️ [DEV ONLY] OTP Code: {OtpCode} for Phone: {Phone} (expires in {TtlSeconds}s)",
                otpCode, phone, ttlSeconds);
        }

        _logger.LogInformation(
            "OTP sent via NOTIFICATIONS. TemplateCode={TemplateCode}, MessageId={MessageId}, TtlSeconds={TtlSeconds}, CorrelationId={CorrelationId}",
            templateCode, messageId, ttlSeconds, correlationId);

        return Task.FromResult<string?>(messageId);
    }
}
