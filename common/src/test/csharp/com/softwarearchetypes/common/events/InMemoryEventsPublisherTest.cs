using System;
using System.Collections.Generic;
using Xunit;

namespace com.softwarearchetypes.common.events;

public sealed class InMemoryEventsPublisherTest
{
    [Fact]
    public void publishesOnlyToSupportingHandlers()
    {
        TestEvent @event = new("supported");
        RecordingHandler supporting = new(eventType => eventType == "supported");
        RecordingHandler ignoring = new(_ => false);
        InMemoryEventsPublisher publisher = new();
        publisher.register(supporting);
        publisher.register(ignoring);

        publisher.publish(@event);

        Assert.Equal([@event], supporting.Events);
        Assert.Empty(ignoring.Events);
    }

    [Fact]
    public void publishesEventListsInOrder()
    {
        TestEvent first = new("first");
        TestEvent second = new("second");
        RecordingHandler handler = new(_ => true);
        InMemoryEventsPublisher publisher = new();
        publisher.register(handler);

        publisher.publish([first, second]);

        Assert.Equal([first, second], handler.Events);
    }

    [Fact]
    public void registeringSameHandlerTwiceDoesNotDuplicateDelivery()
    {
        RecordingHandler handler = new(_ => true);
        InMemoryEventsPublisher publisher = new();
        publisher.register(handler);
        publisher.register(handler);

        publisher.publish(new TestEvent("event"));

        Assert.Single(handler.Events);
    }

    [Fact]
    public void handlerExceptionsPropagateAndStopCurrentPublication()
    {
        InvalidOperationException expected = new("failure");
        InMemoryEventsPublisher publisher = new();
        publisher.register(new ThrowingHandler(expected));

        InvalidOperationException actual = Assert.Throws<InvalidOperationException>(
            () => publisher.publish(new TestEvent("event")));

        Assert.Same(expected, actual);
    }

    private sealed class TestEvent(string eventType) : PublishedEvent
    {
        private readonly Guid eventId = Guid.NewGuid();
        private readonly DateTimeOffset eventTime = new(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

        public Guid id() => eventId;

        public string type() => eventType;

        public DateTimeOffset occurredAt() => eventTime;
    }

    private sealed class RecordingHandler(Func<string, bool> predicate) : EventHandler
    {
        public List<PublishedEvent> Events { get; } = [];

        public bool supports(PublishedEvent @event) => predicate(@event.type());

        public void handle(PublishedEvent @event) => Events.Add(@event);
    }

    private sealed class ThrowingHandler(InvalidOperationException exception) : EventHandler
    {
        public bool supports(PublishedEvent @event) => true;

        public void handle(PublishedEvent @event) => throw exception;
    }
}
