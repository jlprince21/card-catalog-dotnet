using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCatalog.Core.Models;

public class File
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("JobId")]
    public Job? Job { get; set; }

    public string Checksum { get; set; }  = default!;

    [Required]
    public DateTime FoundOn { get; set; }

    [Required]
    public string FileName { get; set; }  = default!;

    [Required]
    public string FilePath { get; set; }  = default!;

    public long FileSize { get; set; }
}