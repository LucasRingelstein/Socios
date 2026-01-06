namespace Socios.Dtos;

public class SubscriptionResponse
{
    public Guid Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MercadoPagoPreapprovalId { get; set; }
    public string? InitPoint { get; set; }
    public string? PayerEmail { get; set; }
    public decimal TransactionAmount { get; set; }
    public string CurrencyId { get; set; } = "USD";
    public int Frequency { get; set; }
    public string FrequencyType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? BackUrl { get; set; }
    public string? CallbackUrl { get; set; }
}
