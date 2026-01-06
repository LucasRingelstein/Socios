namespace Socios.Options;

public class MercadoPagoSettings
{
    public string AccessToken { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string? WebhookSecret { get; set; }
    public string? WebhookCallbackUrl { get; set; }
    public string? BackUrl { get; set; }
}
