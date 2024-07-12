using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCatalog.Core.Models;

public class AppliedTag
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("File")]
    public virtual File FileRefId { get; set; }  = default!;

    [ForeignKey("Tag")]
    [Required]
    public Tag TagRefId { get; set; }  = default!;
}