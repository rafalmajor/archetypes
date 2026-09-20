using System.Collections.Generic;

namespace SoftwareArchetypes.Common.Events;

public interface EventPublisher
{
    void Publish(PublishedEvent @event);

    void Publish(IReadOnlyList<PublishedEvent> events);

    void Register(EventHandler eventHandler);
}
