using AdvancedTimezones.Models;
using AdvancedTimezones.Services;
using FluentAssertions;
using NodaTime;
using NodaTime.Testing;
using Xunit;

namespace AdvancedTimezones.Tests;

public class MeetingSchedulingTests
{
    private readonly MeetingSchedulingService _service;
    private readonly IDateTimeZoneProvider _provider;

    public MeetingSchedulingTests()
    {
        // FakeClock is not strictly needed for the calculation itself, but useful if service relied on "Now"
        var clock = new FakeClock(Instant.FromUtc(2023, 1, 1, 0, 0));
        _provider = DateTimeZoneProviders.Tzdb;
        _service = new MeetingSchedulingService(clock, _provider);
    }

    [Fact]
    public void Schedule_Across_Zones_Returns_Correct_Instant()
    {
        // Arrange
        // London is UTC+1 in Summer (BST)
        // New York is UTC-4 in Summer (EDT)
        // Diff = 5 hours
        var request = new MeetingRequest(
            RequesterTimeZone: "Europe/London",
            TargetTimeZone: "America/New_York",
            RequestedLocalTime: new LocalDateTime(2023, 7, 1, 15, 0) // 15:00 London
        );

        // Act
        var response = _service.ScheduleMeeting(request);

        // Assert
        // 15:00 London (UTC+1) => 14:00 UTC
        // 14:00 UTC => 10:00 New York (UTC-4)
        response.UtcTime.Should().Be(Instant.FromUtc(2023, 7, 1, 14, 0));
        response.TargetTime.Hour.Should().Be(10);
    }

    [Fact]
    public void Schedule_DstGap_ShiftsForward()
    {
        // Arrange
        // In Europe/London, clocks went forward at 01:00 UTC on March 26, 2023.
        // 01:00 becomes 02:00. So 01:30 local time does NOT exist.
        var request = new MeetingRequest(
            RequesterTimeZone: "Europe/London",
            TargetTimeZone: "UTC",
            RequestedLocalTime: new LocalDateTime(2023, 3, 26, 1, 30) // Does not exist
        );

        // Act
        var response = _service.ScheduleMeeting(request);

        // Assert
        // NodaTime Lenient resolver shifts forward by the gap length (usually 1 hour).
        // So 01:30 -> 02:30 BST (which is 01:30 UTC)
        response.Warning.Should().Contain("DST gap");
        response.RequesterTime.Hour.Should().Be(2); 
        response.RequesterTime.Minute.Should().Be(30);
    }

    [Fact]
    public void Schedule_DstAmbiguity_SelectsLaterOffset()
    {
        // Arrange
        // In Europe/London, clocks went back at 02:00 local time on Oct 29, 2023.
        // 01:30 happens twice: Once strictly UTC+1 (BST), then again as UTC+0 (GMT).
        var request = new MeetingRequest(
            RequesterTimeZone: "Europe/London",
            TargetTimeZone: "UTC",
            RequestedLocalTime: new LocalDateTime(2023, 10, 29, 1, 30)
        );

        // Act
        var response = _service.ScheduleMeeting(request);

        // Assert
        // NodaTime Lenient resolver usually picks the later occurrence (post-transition, standard time).
        // 01:30 GMT (Standard) is 01:30 UTC.
        // 01:30 BST (Daylight) would be 00:30 UTC.
        
        response.Warning.Should().Contain("DST ambiguous");
        response.UtcTime.ToDateTimeUtc().Hour.Should().Be(1); // Means it picked GMT (UTC+0)
        response.UtcTime.ToDateTimeUtc().Minute.Should().Be(30);
    }
}
