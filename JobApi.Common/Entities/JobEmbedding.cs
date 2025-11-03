using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace JobApi.Common.Entities;

[Table("job_embeddings")]
public class JobEmbedding
{
    [Key]
    [Column("job_id")]
    public Guid JobId { get; set; }

    [Column("embedding")]
    [Required]
    public Vector Embedding { get; set; } = null!;

    // Navigation property
    [ForeignKey("JobId")]
    public Job? Job { get; set; }
}
