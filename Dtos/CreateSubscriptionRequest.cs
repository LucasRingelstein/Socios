using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Socios.Dtos;

public class CreateSubscriptionRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("payerEmail")]
    public string PayerEmail { get; set; } = string.Empty;

    [JsonPropertyName("planName")]
    public string? PlanName { get; set; }

    [JsonPropertyName("plan")]
    public string? Plan { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    [JsonPropertyName("transactionAmount")]
    public decimal TransactionAmount { get; set; }

    [JsonPropertyName("price")]
    public decimal Price
    {
        get => TransactionAmount;
        set => TransactionAmount = value;
    }

    [MaxLength(8)]
    [JsonPropertyName("currencyId")]
    public string CurrencyId { get; set; } = "USD";

    public int Frequency { get; set; } = 1;

    [MaxLength(32)]
    [JsonPropertyName("frequencyType")]
    public string FrequencyType { get; set; } = "months";

    [JsonPropertyName("interval")]
    public string? Interval { get; set; }

    [Url]
    public string? BackUrl { get; set; }

    [Url]
    public string? CallbackUrl { get; set; }

    public string? UserFullName { get; set; }
}
