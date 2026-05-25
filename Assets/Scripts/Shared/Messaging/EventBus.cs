using System;
using System.Collections.Generic;

// Sends simple typed events between game systems without direct references.
public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> Events = new();

    // Registers a listener for an event type.
    public static void Subscribe<T>(Action<T> listener)
    {
        var type = typeof(T);

        if (Events.TryGetValue(type, out var existingDelegate))
        {
            Events[type] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            Events[type] = listener;
        }
    }

    // Removes a listener from an event type.
    public static void Unsubscribe<T>(Action<T> listener)
    {
        var type = typeof(T);

        if (!Events.TryGetValue(type, out var existingDelegate))
            return;

        var newDelegate = Delegate.Remove(existingDelegate, listener);

        if (newDelegate == null)
            Events.Remove(type);
        else
            Events[type] = newDelegate;
    }

    // Sends event data to all listeners of its type.
    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);

        if (Events.TryGetValue(type, out var existingDelegate))
        {
            ((Action<T>)existingDelegate)?.Invoke(eventData);
        }
    }
}
