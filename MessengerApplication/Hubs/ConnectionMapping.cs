using System.Collections.Concurrent;

namespace MessengerApplication.Hubs;

public class ConnectionMapping
{
    private readonly ConcurrentDictionary<string, List<string>> _connections = new();

    public void Add(string userId, string connectionId)
    {
        _connections.AddOrUpdate(userId, 
            new List<string> { connectionId }, 
            (key, oldValue) =>
            {
                oldValue.Add(connectionId);
                return oldValue;
            });
    }

    public void Remove(string userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var connectionIds))
        {
            connectionIds.Remove(connectionId);
            if (connectionIds.Count == 0)
            {
                _connections.TryRemove(userId, out _);
            }
        }
    }

    public List<string>? GetConnections(string userId)
    {
        return _connections.TryGetValue(userId, out var connectionIds) ? connectionIds : null;
    }
}
