using System;

namespace SoftwareArchetypes.Common.Events;

public interface PublishedEvent
{
    Guid Id();

    string Type();

    DateTimeOffset OccurredAt();
}
