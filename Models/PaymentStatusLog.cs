using System.ComponentModel.DataAnnotations;

namespace Socios.Models;

public class PaymentStatusLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SubscriptionId { get; set; }

    [MaxLength(128)]
    public string? MercadoPagoEventId { get; set; }

    [MaxLength(64)]
    public string? Status { get; set; }

    [MaxLength(64)]
    public string? Topic { get; set; }

    public string RawPayload { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
