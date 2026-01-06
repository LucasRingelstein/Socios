using System.ComponentModel.DataAnnotations;

namespace Socios.Dtos;

public class CreateSubscriptionRequest
{
    [Required]
    [EmailAddress]
    public string PayerEmail { get; set; } = string.Empty;

    [Required]
    public string PlanName { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal TransactionAmount { get; set; }

    [Required]
    [MaxLength(8)]
    public string CurrencyId { get; set; } = "USD";

    public int Frequency { get; set; } = 1;

    [MaxLength(32)]
    public string FrequencyType { get; set; } = "months";

    [Url]
    public string? BackUrl { get; set; }

    [Url]
    public string? CallbackUrl { get; set; }

    public string? UserFullName { get; set; }
}
