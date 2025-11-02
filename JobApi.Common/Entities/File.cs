using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApi.Common.Entities;

[Table("files")]
public class File
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("filename")]
    [Required]
    [MaxLength(500)]
    public string Filename { get; set; } = string.Empty;

    [Column("dateprocessed")]
    public DateTime? DateProcessed { get; set; }

    // Navigation property
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
