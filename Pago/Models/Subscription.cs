using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Socios.Models;

public class Subscription
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    public string PlanName { get; set; } = string.Empty;

    [Required]
    public decimal TransactionAmount { get; set; }

    [Required]
    [MaxLength(8)]
    public string CurrencyId { get; set; } = "USD";

    [MaxLength(32)]
    public string FrequencyType { get; set; } = "months";

    public int Frequency { get; set; } = 1;

    [MaxLength(128)]
    public string? MercadoPagoPreapprovalId { get; set; }

    [MaxLength(64)]
    public string Status { get; set; } = "pending";

    [MaxLength(512)]
    public string? BackUrl { get; set; }

    [MaxLength(512)]
    public string? CallbackUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;
}
