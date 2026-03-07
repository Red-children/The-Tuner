using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus
{
    private static EventBus instance;

    public static EventBus Instance 
    {
        get 
        {
            if(instance == null) 
            {
                instance = new EventBus();
            }
            return instance;
        }
    }

    private Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();


    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        lock (_events)
        {
            if (_events.ContainsKey(type))
                _events[type] = Delegate.Combine(_events[type], handler);
            else
                _events[type] = handler;

        }
    }

    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        lock (_events)
        {
            if (_events.TryGetValue(type, out Delegate del))
            {
                del = Delegate.Remove(del, handler);
                if (del == null)
                    _events.Remove(type);
                else
                    _events[type] = del;
            }
        }
    }

    public void Trigger<T>(T eventData) where T : struct
    {
        Type type = typeof(T);
        Delegate del;
        lock (_events)
        {
            if (!_events.TryGetValue(type, out del))
                return;
        }
        (del as Action<T>)?.Invoke(eventData);
    }

}
