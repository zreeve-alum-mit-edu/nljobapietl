using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("job_location_urls")]
public class JobLocationUrl
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("job_location_id")]
    [Required]
    public Guid JobLocationId { get; set; }

    [Column("url")]
    [MaxLength(1000)]
    [Required]
    public string Url { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("JobLocationId")]
    public JobLocation? JobLocation { get; set; }
}
