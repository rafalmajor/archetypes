namespace com.softwarearchetypes.common.events;

public interface EventHandler
{
    bool supports(PublishedEvent @event);

    void handle(PublishedEvent @event);
}
