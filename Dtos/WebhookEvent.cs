using System.Text.Json.Serialization;

namespace Socios.Dtos;

public class WebhookEvent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("data")]
    public WebhookEventData? Data { get; set; }
}

public class WebhookEventData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
