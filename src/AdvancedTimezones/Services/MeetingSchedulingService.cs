using NodaTime;
using AdvancedTimezones.Models;

namespace AdvancedTimezones.Services;

public class MeetingSchedulingService
{
    private readonly IClock _clock;
    private readonly IDateTimeZoneProvider _zoneProvider;

    public MeetingSchedulingService(IClock clock, IDateTimeZoneProvider zoneProvider)
    {
        _clock = clock;
        _zoneProvider = zoneProvider;
    }

    public MeetingResponse ScheduleMeeting(MeetingRequest request)
    {
        var requesterZone = _zoneProvider[request.RequesterTimeZone];
        var targetZone = _zoneProvider[request.TargetTimeZone];

        // 1. Resolve Local Time to Zoned Time (Requester's perspective)
        // LENIENTLY: If ambiguity (1:30 AM exists twice), pick the later one. If gap (2:00 AM doesn't exist), shift forward.
        var requesterZoned = requesterZone.AtLeniently(request.RequestedLocalTime);
        
        // 2. Convert to UTC (Absolute Truth)
        var instant = requesterZoned.ToInstant();

        // 3. Convert to Target Zone
        var targetZoned = instant.InZone(targetZone);

        string warning = string.Empty;
        
        // Check for Ambiguity/Gap explicitly for educational purposes
        var mapping = requesterZone.MapLocal(request.RequestedLocalTime);
        if (mapping.Count == 0)
        {
            warning = "Requested time fall into a DST gap (skipped hour). Shifted forward.";
        }
        else if (mapping.Count == 2)
        {
            warning = "Requested time is ambiguous (DST fallback). Assumed standard time.";
        }

        return new MeetingResponse(instant, requesterZoned, targetZoned, warning);
    }
}
