using System.ComponentModel.DataAnnotations;

namespace Socios.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? FullName { get; set; }

    public List<Subscription> Subscriptions { get; set; } = new();
}
