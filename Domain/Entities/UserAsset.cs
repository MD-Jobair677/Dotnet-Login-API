namespace EmsSystem.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmsSystem.Domain.Entities;
public class UserAsset
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string AssetName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AssetType { get; set; } = string.Empty;

    [Required]
    public string Path { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User User { get; set; } = null!;
}