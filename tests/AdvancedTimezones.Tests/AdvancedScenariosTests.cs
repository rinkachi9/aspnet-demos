using AdvancedTimezones.Services;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace AdvancedTimezones.Tests;

public class AdvancedScenariosTests
{
    private readonly FlightService _flightService;
    private readonly RecurrenceService _recurrenceService;
    private readonly IDateTimeZoneProvider _provider;

    public AdvancedScenariosTests()
    {
        _provider = DateTimeZoneProviders.Tzdb;
        _flightService = new FlightService(_provider);
        _recurrenceService = new RecurrenceService(_provider);
    }

    [Fact]
    public void CalculateFlightDuration_TokyoToLondon_CrossesDayAndZones()
    {
        // Arrange
        // Departure: Tokyo, 2023-10-01 19:00 (UTC+9) -> 10:00 UTC
        var depLocal = new LocalDateTime(2023, 10, 1, 19, 0); 
        
        // Arrival: London, 2023-10-02 05:00 (UTC+1 BST) -> 04:00 UTC next day
        var arrLocal = new LocalDateTime(2023, 10, 2, 5, 0);

        // Act
        var duration = _flightService.CalculateFlightDuration("Asia/Tokyo", depLocal, "Europe/London", arrLocal);

        // Assert
        // 10:00 UTC (Oct 1) to 04:00 UTC (Oct 2) = 18 hours
        duration.TotalHours.Should().Be(18);
    }

    [Fact]
    public void GenerateDailyOccurrences_KeepsWallTime_AcrossDstChange()
    {
        // Arrange
        var zoneId = "Europe/London";
        // March 25th 2023, 10:00 AM. 
        // DST change is March 26th morning.
        var startLocal = new LocalDateTime(2023, 3, 25, 10, 0);

        // Act
        var occurrences = _recurrenceService.GenerateDailyOccurrences(zoneId, startLocal, 3).ToList();

        // Assert
        occurrences.Should().HaveCount(3);
        
        // Day 1: 10:00 Standard (UTC+0) -> 10:00 UTC
        occurrences[0].Hour.Should().Be(10);
        occurrences[0].ToInstant().ToDateTimeUtc().Hour.Should().Be(10);

        // Day 2: 10:00 Daylight (UTC+1) -> 09:00 UTC (Clock moved forward, so 10:00 is earlier in UTC)
        occurrences[1].Hour.Should().Be(10); // Wall time is still 10:00
        occurrences[1].ToInstant().ToDateTimeUtc().Hour.Should().Be(9); // UTC shifted

        // Day 3: 10:00 Daylight (UTC+1)
        occurrences[2].Hour.Should().Be(10);
    }
}
