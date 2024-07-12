using NodaTime;

namespace CardCatalog.Core.Services;

public interface ITimeService
{
    ZonedDateTime Now { get; }
}

public class TimeService : ITimeService
{
    private readonly IClock _clock;

    public TimeService(IClock clock)
    {
        _clock = clock;
    }

    public ZonedDateTime Now => _clock.GetCurrentInstant().InUtc(); // Could use .InZone() here instead
}
