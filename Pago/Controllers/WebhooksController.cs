using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Socios.Dtos;
using Socios.Services;

namespace Socios.Controllers;

[ApiController]
[Route("webhooks/mercadopago")]
public class WebhooksController : ControllerBase
{
    private readonly WebhookSignatureValidator _signatureValidator;
    private readonly SubscriptionService _subscriptionService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        WebhookSignatureValidator signatureValidator,
        SubscriptionService subscriptionService,
        ILogger<WebhooksController> logger)
    {
        _signatureValidator = signatureValidator;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromHeader(Name = "x-signature")] string? signature)
    {
        string rawBody;
        using (var reader = new StreamReader(Request.Body))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        if (!_signatureValidator.IsValid(signature, rawBody))
        {
            return Unauthorized(new { message = "Invalid signature" });
        }

        WebhookEvent? webhookEvent = null;
        try
        {
            webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not deserialize Mercado Pago webhook payload.");
        }

        await _subscriptionService.ProcessWebhookAsync(rawBody, webhookEvent);
        return Ok(new { received = true });
    }
}
