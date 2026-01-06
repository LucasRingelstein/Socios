using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Socios.Options;

namespace Socios.Services;

public class WebhookSignatureValidator
{
    private readonly MercadoPagoSettings _settings;
    private readonly ILogger<WebhookSignatureValidator> _logger;

    public WebhookSignatureValidator(IOptions<MercadoPagoSettings> settings, ILogger<WebhookSignatureValidator> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsValid(string? signatureHeader, string payload)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            _logger.LogWarning("No Mercado Pago webhook secret configured; skipping signature validation.");
            return true;
        }

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            _logger.LogWarning("Signature header missing.");
            return false;
        }

        var provided = ParseSignature(signatureHeader);
        if (string.IsNullOrWhiteSpace(provided))
        {
            _logger.LogWarning("Unable to parse signature header.");
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(provided));
    }

    private static string? ParseSignature(string signatureHeader)
    {
        const string prefix = "sha256=";

        foreach (var part in signatureHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return part[prefix.Length..].Trim();
            }
        }

        if (signatureHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return signatureHeader[prefix.Length..].Trim();
        }

        return null;
    }
}
