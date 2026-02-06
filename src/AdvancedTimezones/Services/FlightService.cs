using NodaTime;

namespace AdvancedTimezones.Services;

public class FlightService
{
    private readonly IDateTimeZoneProvider _zoneProvider;

    public FlightService(IDateTimeZoneProvider zoneProvider)
    {
        _zoneProvider = zoneProvider;
    }

    public Duration CalculateFlightDuration(
        string departureZoneId, LocalDateTime departureLocal,
        string arrivalZoneId, LocalDateTime arrivalLocal)
    {
        var departureZone = _zoneProvider[departureZoneId];
        var arrivalZone = _zoneProvider[arrivalZoneId];

        // 1. Convert everything to Instant (Physics Time)
        // We assume schedule is clean, using Lenient in case of gaps
        var departureInstant = departureZone.AtLeniently(departureLocal).ToInstant();
        var arrivalInstant = arrivalZone.AtLeniently(arrivalLocal).ToInstant();

        // 2. Subtract
        var duration = arrivalInstant - departureInstant;

        return duration;
    }
}
