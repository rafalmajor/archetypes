using System;

namespace com.softwarearchetypes.common.events;

public interface PublishedEvent
{
    Guid id();

    string type();

    DateTimeOffset occurredAt();
}
