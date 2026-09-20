namespace SoftwareArchetypes.Common.Events;

public interface EventHandler
{
    bool Supports(PublishedEvent @event);

    void Handle(PublishedEvent @event);
}
