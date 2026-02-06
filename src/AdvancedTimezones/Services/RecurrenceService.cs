using NodaTime;

namespace AdvancedTimezones.Services;

public class RecurrenceService
{
    private readonly IDateTimeZoneProvider _zoneProvider;

    public RecurrenceService(IDateTimeZoneProvider zoneProvider)
    {
        _zoneProvider = zoneProvider;
    }

    public IEnumerable<ZonedDateTime> GenerateDailyOccurrences(
        string zoneId, 
        LocalDateTime startLocal, 
        int occurrences)
    {
        var zone = _zoneProvider[zoneId];
        // Ensure start is valid
        var zonedStart = zone.AtLeniently(startLocal);
        
        // Pattern: "At 10:00 Local Time Every Day"
        // Correct way: Add days to the LocalDateTime, THEN re-map to ZonedTime
        // Incorrect way: Add Duration.FromDays(1) to ZonedTime (this would shift 10:00 to 11:00/09:00 across DST)
        
        for (int i = 0; i < occurrences; i++)
        {
            var nextLocal = startLocal.PlusDays(i);
            
            // Re-evaluating zone rules for every single occurrence
            // This handles DST transitions correctly (e.g. 10:00 stays 10:00)
            yield return zone.AtLeniently(nextLocal);
        }
    }
}
