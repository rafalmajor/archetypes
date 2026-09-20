using System.Collections.Generic;

namespace com.softwarearchetypes.common.events;

public interface EventPublisher
{
    void publish(PublishedEvent @event);

    void publish(IReadOnlyList<PublishedEvent> events);

    void register(EventHandler eventHandler);
}
