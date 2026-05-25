using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> Events = new();

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

    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);

        if (Events.TryGetValue(type, out var existingDelegate))
        {
            ((Action<T>)existingDelegate)?.Invoke(eventData);
        }
    }
}