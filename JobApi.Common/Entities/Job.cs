using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("jobs")]
public class Job
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("date_inserted")]
    public DateTime DateInserted { get; set; }

    [Column("status")]
    [MaxLength(50)]
    [Required]
    public string Status { get; set; } = "pending";

    [Column("status_change_date")]
    public DateTime? StatusChangeDate { get; set; }

    [Column("is_valid")]
    public bool IsValid { get; set; } = true;

    [Column("file_id")]
    [Required]
    public Guid FileId { get; set; }

    [Column("portal")]
    [MaxLength(100)]
    public string? Portal { get; set; }

    [Column("source")]
    [MaxLength(100)]
    public string? Source { get; set; }

    [Column("sourcecc")]
    [MaxLength(10)]
    public string? SourceCC { get; set; }

    [Column("isduplicate")]
    public bool IsDuplicate { get; set; }

    [Column("locale")]
    [MaxLength(10)]
    public string? Locale { get; set; }

    [Column("job_title")]
    [MaxLength(500)]
    public string? JobTitle { get; set; }

    [Column("job_description")]
    public string? JobDescription { get; set; }

    [Column("job_description_hash")]
    [MaxLength(64)]
    public string? JobDescriptionHash { get; set; }

    [Column("date_posted")]
    public DateTime? DatePosted { get; set; }

    [Column("employment_type")]
    [MaxLength(100)]
    public string? EmploymentType { get; set; }

    [Column("company_name")]
    [MaxLength(500)]
    public string? CompanyName { get; set; }

    [Column("company_url")]
    [MaxLength(1000)]
    public string? CompanyUrl { get; set; }

    [Column("validthrough")]
    public DateTime? ValidThrough { get; set; }

    [Column("workplace_type")]
    [MaxLength(50)]
    public string? WorkplaceType { get; set; }

    [Column("generated_workplace")]
    [MaxLength(20)]
    public string? GeneratedWorkplace { get; set; }

    [Column("generated_workplace_inferred")]
    public bool? GeneratedWorkplaceInferred { get; set; }

    [Column("generated_workplace_confidence")]
    [MaxLength(20)]
    public string? GeneratedWorkplaceConfidence { get; set; }

    [Column("llm_workplace_retry_count")]
    public int LlmWorkplaceRetryCount { get; set; } = 0;

    // Navigation properties
    [ForeignKey("FileId")]
    public File? File { get; set; }

    public ICollection<JobLocation> Locations { get; set; } = new List<JobLocation>();
}
