using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("location_batches")]
public class LocationBatch
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("batch_file_path")]
    [MaxLength(500)]
    [Required]
    public string BatchFilePath { get; set; } = string.Empty;

    [Column("openai_batch_id")]
    [MaxLength(100)]
    public string? OpenAiBatchId { get; set; }

    [Column("openai_input_file_id")]
    [MaxLength(100)]
    public string? OpenAiInputFileId { get; set; }

    [Column("status")]
    [MaxLength(50)]
    [Required]
    public string Status { get; set; } = "pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }
}
