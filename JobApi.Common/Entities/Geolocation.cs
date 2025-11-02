using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("geolocations")]
public class Geolocation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("city")]
    [MaxLength(100)]
    [Required]
    public string City { get; set; } = string.Empty;

    [Column("state")]
    [MaxLength(2)]
    [Required]
    public string State { get; set; } = string.Empty;

    [Column("country")]
    [MaxLength(2)]
    public string? Country { get; set; }

    [Column("latitude")]
    public decimal Latitude { get; set; }

    [Column("longitude")]
    public decimal Longitude { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
