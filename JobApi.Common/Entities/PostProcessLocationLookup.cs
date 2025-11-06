using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("post_process_location_lookups")]
public class PostProcessLocationLookup
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("generated_city")]
    [MaxLength(100)]
    public string? GeneratedCity { get; set; }

    [Column("generated_state")]
    [MaxLength(2)]
    public string? GeneratedState { get; set; }

    [Column("generated_country")]
    [MaxLength(2)]
    public string? GeneratedCountry { get; set; }

    [Column("city")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Column("state")]
    [MaxLength(2)]
    public string? State { get; set; }

    [Column("country")]
    [MaxLength(2)]
    public string? Country { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
