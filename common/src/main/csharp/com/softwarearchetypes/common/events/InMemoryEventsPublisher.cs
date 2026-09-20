using System.Collections.Generic;

namespace com.softwarearchetypes.common.events;

public class InMemoryEventsPublisher : EventPublisher
{
    private readonly HashSet<EventHandler> observers = [];

    public void publish(PublishedEvent @event)
    {
        foreach (EventHandler observer in observers)
        {
            if (observer.supports(@event))
            {
                observer.handle(@event);
            }
        }
    }

    public void publish(IReadOnlyList<PublishedEvent> events)
    {
        foreach (PublishedEvent @event in events)
        {
            publish(@event);
        }
    }

    public void register(EventHandler eventHandler) => observers.Add(eventHandler);
}
