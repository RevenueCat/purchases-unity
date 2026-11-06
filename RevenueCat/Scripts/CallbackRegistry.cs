using System;
using System.Collections.Generic;

internal sealed class CallbackRegistry
{
    private sealed class CallbackEntry
    {
        internal readonly Type Type;
        internal readonly object Callback;

        internal CallbackEntry(Type type, object callback)
        {
            Type = type;
            Callback = callback;
        }
    }

    private readonly object _lock = new object();
    private readonly Dictionary<string, CallbackEntry> _callbacks =
        new Dictionary<string, CallbackEntry>();

    internal string Register<T>(T callback) where T : class
    {
        var requestId = Guid.NewGuid().ToString("N");
        lock (_lock)
        {
            _callbacks.Add(requestId, new CallbackEntry(typeof(T), callback));
        }

        return requestId;
    }

    internal bool TryTake<T>(string requestId, out T callback) where T : class
    {
        callback = null;
        if (string.IsNullOrEmpty(requestId))
        {
            return false;
        }

        lock (_lock)
        {
            if (!_callbacks.TryGetValue(requestId, out var entry) || entry.Type != typeof(T))
            {
                return false;
            }

            _callbacks.Remove(requestId);
            callback = (T)entry.Callback;
            return true;
        }
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _callbacks.Clear();
        }
    }
}
