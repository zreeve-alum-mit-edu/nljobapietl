using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("job_locations")]
public class JobLocation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("job_id")]
    [Required]
    public Guid JobId { get; set; }

    [Column("location")]
    [MaxLength(500)]
    public string? Location { get; set; }

    [Column("country")]
    [MaxLength(100)]
    public string? Country { get; set; }

    [Column("region")]
    [MaxLength(100)]
    public string? Region { get; set; }

    [Column("locality")]
    [MaxLength(100)]
    public string? Locality { get; set; }

    [Column("postcode")]
    [MaxLength(20)]
    public string? Postcode { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("gistlocation")]
    public NpgsqlTypes.PostgisGeography? GistLocation { get; set; }

    [Column("generated_city")]
    [MaxLength(100)]
    public string? GeneratedCity { get; set; }

    [Column("generated_state")]
    [MaxLength(2)]
    public string? GeneratedState { get; set; }

    [Column("generated_country")]
    [MaxLength(2)]
    public string? GeneratedCountry { get; set; }

    [Column("llm_location_retry_count")]
    public int LlmLocationRetryCount { get; set; } = 0;

    // Navigation properties
    [ForeignKey("JobId")]
    public Job? Job { get; set; }

    public ICollection<JobLocationUrl> Urls { get; set; } = new List<JobLocationUrl>();
}
