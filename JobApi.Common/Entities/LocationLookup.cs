using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("location_lookups")]
public class LocationLookup
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("location_text")]
    [MaxLength(500)]
    [Required]
    public string LocationText { get; set; } = string.Empty;

    [Column("city")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Column("state")]
    [MaxLength(2)]
    public string? State { get; set; }

    [Column("country")]
    [MaxLength(2)]
    public string? Country { get; set; }

    [Column("confidence")]
    public int Confidence { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
