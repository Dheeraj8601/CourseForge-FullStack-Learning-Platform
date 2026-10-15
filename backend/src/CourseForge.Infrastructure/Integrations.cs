
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using CourseForge.Application;
using CourseForge.Domain;

using Google.Apis.Auth;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CourseForge.Infrastructure;

// ==========================================
// 1. JWT Token Service
// ==========================================

public sealed class TokenService(IConfiguration config) : ITokenService
{
    public string Create(User user)
    {
        var key = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Configure Jwt:Key");

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "JWT key must contain at least 32 bytes");
        }

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Role,
                user.Role),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                ClaimTypes.Name,
                user.Name)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}

// ==========================================
// 2. Google Identity Verification Service
// ==========================================

public sealed class GoogleIdentityVerifier(
    IConfiguration config) : IGoogleIdentityVerifier
{
    public async Task<(string Subject, string Email, string Name)> VerifyAsync(
        string credential)
    {
        var clientId = config["Google:ClientId"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException(
                "Configure Google:ClientId");
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(
            credential,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

        if (!payload.EmailVerified)
        {
            throw new UnauthorizedAccessException(
                "Google email is not verified");
        }

        return (
            payload.Subject,
            payload.Email,
            payload.Name ?? payload.Email
        );
    }
}

// ==========================================
// 3. Razorpay Payment Gateway Service
// ==========================================

public sealed class RazorpayGateway(
    IConfiguration config,
    HttpClient http) : IPaymentGateway
{
    // ==========================================
    // Create Razorpay Order
    // ==========================================

    public async Task<(string OrderId, string KeyId)> CreateOrderAsync(
        decimal amount,
        string receipt,
        CancellationToken ct)
    {
        var keyId = config["Razorpay:KeyId"] ?? "";
        var keySecret = config["Razorpay:KeySecret"] ?? "";

        if (string.IsNullOrWhiteSpace(keyId) ||
            string.IsNullOrWhiteSpace(keySecret))
        {
            throw new InvalidOperationException(
                "Set Razorpay KeyId and KeySecret before accepting payments");
        }

        var orderRequest = new
        {
            amount = (long)(amount * 100),
            currency = "INR",
            receipt
        };

        var jsonContent = JsonSerializer.Serialize(orderRequest);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.razorpay.com/v1/orders")
        {
            Content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json")
        };

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                credentials);

        using var response = await http.SendAsync(
            request,
            ct);

        var responseJson = await response.Content
            .ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Razorpay order creation failed: " + responseJson);
        }

        using var document = JsonDocument.Parse(responseJson);

        var orderId = document.RootElement
            .GetProperty("id")
            .GetString()!;

        return (orderId, keyId);
    }

    // ==========================================
    // Verify Razorpay Payment Signature
    // ==========================================

    public bool Verify(
        string orderId,
        string paymentId,
        string signature)
    {
        var message = orderId + "|" + paymentId;

        return Matches(
            message,
            signature,
            config["Razorpay:KeySecret"]);
    }

    // ==========================================
    // Verify Razorpay Webhook Signature
    // ==========================================

    public bool VerifyWebhook(
        string payload,
        string signature)
    {
        return Matches(
            payload,
            signature,
            config["Razorpay:WebhookSecret"]);
    }

    // ==========================================
    // HMAC SHA256 Signature Validation
    // ==========================================

    private static bool Matches(
        string message,
        string hex,
        string? secret)
    {
        if (string.IsNullOrEmpty(secret) ||
            string.IsNullOrEmpty(hex))
        {
            return false;
        }

        try
        {
            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(secret));

            var computedHash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(message));

            var receivedHash = Convert.FromHexString(hex);

            return CryptographicOperations.FixedTimeEquals(
                computedHash,
                receivedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
