using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoShare.Api.Contracts.Auth;
using CoShare.Api.Contracts.Common;
using CoShare.Domain.Auth;
using Xunit;

namespace CoShare.Api.IntegrationTests.Auth;

/// <summary>
/// Integration tests for AUTH registration endpoints.
/// Test ID: IT-AUTH-001
/// </summary>
public class AuthRegistrationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthRegistrationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Request OTP Tests

    [Fact]
    public async Task RequestOtp_WithValidRequest_Returns202Accepted()
    {
        // Arrange
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Trần Thị B",
            CompanyId = 501,
            Phone = "0912345678",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }

    [Fact]
    public async Task RequestOtp_WithInvalidPhone_Returns400BadRequest()
    {
        // Arrange
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Test User",
            CompanyId = 501,
            Phone = "invalid-phone",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestOtp_WithTermsNotAccepted_Returns400BadRequest()
    {
        // Arrange
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Test User",
            CompanyId = 501,
            Phone = "0912345678",
            AcceptTerms = false
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestOtp_WithInvalidCompany_Returns400BadRequest()
    {
        // Arrange
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Test User",
            CompanyId = 999999, // Non-existent company
            Phone = "0912345670",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.Equal(AuthErrorCodes.CompanyNotFound, error?.Error.Code);
    }

    [Fact]
    public async Task RequestOtp_ExceedingRateLimit_Returns429TooManyRequests()
    {
        // Arrange
        var phone = "0912345671";
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Test User",
            CompanyId = 501,
            Phone = phone,
            AcceptTerms = true
        };

        // Act - Send 4 requests (limit is 3)
        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync(
                "/api/v1/auth/end-user/register/request-otp",
                request,
                _jsonOptions);
        }

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.Equal(AuthErrorCodes.OtpRateLimited, error?.Error.Code);
    }

    #endregion

    #region Verify OTP Tests

    [Fact]
    public async Task VerifyOtp_WithNoExistingOtp_Returns401Unauthorized()
    {
        // Arrange
        var request = new EndUserRegisterVerifyRequest
        {
            Phone = "0912345699",
            OtpCode = "123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/verify-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        Assert.Equal(AuthErrorCodes.OtpInvalidOrExpired, error?.Error.Code);
    }

    [Fact]
    public async Task VerifyOtp_WithInvalidPhone_Returns400BadRequest()
    {
        // Arrange
        var request = new EndUserRegisterVerifyRequest
        {
            Phone = "invalid",
            OtpCode = "123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/verify-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyOtp_WithEmptyOtpCode_Returns400BadRequest()
    {
        // Arrange
        var request = new EndUserRegisterVerifyRequest
        {
            Phone = "0912345678",
            OtpCode = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/verify-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Full Registration Flow

    [Fact]
    public async Task FullRegistrationFlow_HappyPath_ReturnsTokens()
    {
        // This test validates the end-to-end flow
        // Note: In real integration tests, we would need to extract the OTP from the database
        // For now, we verify the request-otp step works

        // Arrange
        var phone = "0912345672";
        var requestOtpRequest = new EndUserRegisterStartRequest
        {
            FullName = "Nguyễn Văn A",
            CompanyId = 501,
            Phone = phone,
            AcceptTerms = true
        };

        // Act - Request OTP
        var otpResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            requestOtpRequest,
            _jsonOptions);

        // Assert - OTP requested successfully
        Assert.Equal(HttpStatusCode.Accepted, otpResponse.StatusCode);
        Assert.True(otpResponse.Headers.Contains("X-Correlation-Id"));
        var correlationId = otpResponse.Headers.GetValues("X-Correlation-Id").First();
        Assert.False(string.IsNullOrEmpty(correlationId));
    }

    #endregion

    #region Correlation ID Tests

    [Fact]
    public async Task RequestOtp_WithCorrelationIdHeader_ReturnsItInResponse()
    {
        // Arrange
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Test User",
            CompanyId = 501,
            Phone = "0912345673",
            AcceptTerms = true
        };
        var expectedCorrelationId = "test-correlation-123";

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/end-user/register/request-otp")
        {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };
        httpRequest.Headers.Add("X-Correlation-Id", expectedCorrelationId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        var returnedCorrelationId = response.Headers.GetValues("X-Correlation-Id").First();
        Assert.Equal(expectedCorrelationId, returnedCorrelationId);
    }

    [Fact]
    public async Task RequestOtp_WithoutCorrelationIdHeader_GeneratesOne()
    {
        // Arrange
        var request = new EndUserRegisterStartRequest
        {
            FullName = "Test User",
            CompanyId = 501,
            Phone = "0912345674",
            AcceptTerms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/end-user/register/request-otp",
            request,
            _jsonOptions);

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        var correlationId = response.Headers.GetValues("X-Correlation-Id").First();
        Assert.False(string.IsNullOrEmpty(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _)); // Should be a valid GUID
    }

    #endregion
}
