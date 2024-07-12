// System namespaces
using System.ComponentModel.DataAnnotations;

// Third-party namespaces (NuGet packages)
using NodaTime;

// Project-specific namespaces
using CardCatalog.Core.Common;

namespace CardCatalog.Core.Models;

public class Job
{
    [Key]
    public Guid Id { get; set; }

    public JobType Type { get; set; }

    public string Data { get; set; }  = default!;

    public ZonedDateTime RunStartTime { get; set; }

    public ZonedDateTime RunEndTime { get; set; }
}