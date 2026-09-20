using System.Collections.Generic;

namespace SoftwareArchetypes.Common.Events;

public class InMemoryEventsPublisher : EventPublisher
{
    private readonly HashSet<EventHandler> observers = [];

    public void Publish(PublishedEvent @event)
    {
        foreach (EventHandler observer in observers)
        {
            if (observer.Supports(@event))
            {
                observer.Handle(@event);
            }
        }
    }

    public void Publish(IReadOnlyList<PublishedEvent> events)
    {
        foreach (PublishedEvent @event in events)
        {
            Publish(@event);
        }
    }

    public void Register(EventHandler eventHandler) => observers.Add(eventHandler);
}
