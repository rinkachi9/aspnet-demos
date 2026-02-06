using NodaTime;

namespace AdvancedTimezones.Models;

public record MeetingRequest(
    string RequesterTimeZone,  // e.g. "America/New_York"
    string TargetTimeZone,     // e.g. "Europe/London"
    LocalDateTime RequestedLocalTime // e.g. 2023-10-27T10:00:00 (No Offset!)
);

public record MeetingResponse(
    Instant UtcTime,
    ZonedDateTime RequesterTime,
    ZonedDateTime TargetTime,
    string Warning // Handling ambiguity
);
