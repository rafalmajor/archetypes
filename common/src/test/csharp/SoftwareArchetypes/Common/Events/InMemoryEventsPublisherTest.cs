using System;
using System.Collections.Generic;
using Xunit;

namespace SoftwareArchetypes.Common.Events;

public sealed class InMemoryEventsPublisherTest
{
    [Fact]
    public void PublishesOnlyToSupportingHandlers()
    {
        TestEvent @event = new("supported");
        RecordingHandler supporting = new(eventType => eventType == "supported");
        RecordingHandler ignoring = new(_ => false);
        InMemoryEventsPublisher publisher = new();
        publisher.Register(supporting);
        publisher.Register(ignoring);

        publisher.Publish(@event);

        Assert.Equal([@event], supporting.Events);
        Assert.Empty(ignoring.Events);
    }

    [Fact]
    public void PublishesEventListsInOrder()
    {
        TestEvent first = new("first");
        TestEvent second = new("second");
        RecordingHandler handler = new(_ => true);
        InMemoryEventsPublisher publisher = new();
        publisher.Register(handler);

        publisher.Publish([first, second]);

        Assert.Equal([first, second], handler.Events);
    }

    [Fact]
    public void RegisteringSameHandlerTwiceDoesNotDuplicateDelivery()
    {
        RecordingHandler handler = new(_ => true);
        InMemoryEventsPublisher publisher = new();
        publisher.Register(handler);
        publisher.Register(handler);

        publisher.Publish(new TestEvent("event"));

        Assert.Single(handler.Events);
    }

    [Fact]
    public void HandlerExceptionsPropagateAndStopCurrentPublication()
    {
        InvalidOperationException expected = new("failure");
        InMemoryEventsPublisher publisher = new();
        publisher.Register(new ThrowingHandler(expected));

        InvalidOperationException actual = Assert.Throws<InvalidOperationException>(
            () => publisher.Publish(new TestEvent("event")));

        Assert.Same(expected, actual);
    }

    private sealed class TestEvent(string eventType) : PublishedEvent
    {
        private readonly Guid eventId = Guid.NewGuid();
        private readonly DateTimeOffset eventTime = new(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

        public Guid Id() => eventId;

        public string Type() => eventType;

        public DateTimeOffset OccurredAt() => eventTime;
    }

    private sealed class RecordingHandler(Func<string, bool> predicate) : EventHandler
    {
        public List<PublishedEvent> Events { get; } = [];

        public bool Supports(PublishedEvent @event) => predicate(@event.Type());

        public void Handle(PublishedEvent @event) => Events.Add(@event);
    }

    private sealed class ThrowingHandler(InvalidOperationException exception) : EventHandler
    {
        public bool Supports(PublishedEvent @event) => true;

        public void Handle(PublishedEvent @event) => throw exception;
    }
}
