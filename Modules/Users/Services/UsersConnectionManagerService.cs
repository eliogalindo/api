using System.Collections.Concurrent;
using api.Modules.Users.Interfaces.Services;

namespace api.Modules.Users.Services;

public class UsersConnectionManagerService : IUsersConnectionManagerService
{
    // Map connection → user
    private readonly ConcurrentDictionary<string, string?> _connectionUserMap = new();

    // Map user → connections
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _userConnectionMap = new();

    public void AddConnection(string? userId, string connectionId)
    {
        if (userId == null) return;

        // Add the connection to the user
        _userConnectionMap.AddOrUpdate(
            userId,
            _ => [connectionId],
            (_, existingBag) =>
            {
                existingBag.Add(connectionId);
                return existingBag;
            });

        // Map the connection to the user
        _connectionUserMap[connectionId] = userId;
    }

    public void RemoveConnection(string connectionId)
    {
        if (!_connectionUserMap.TryRemove(connectionId, out var userId)) return;
        if (userId is null) return;

        if (!_userConnectionMap.TryGetValue(userId, out var connections)) return;
        //The ConcurrentBag is reconstructed if needed
        var updatedConnections = new ConcurrentBag<string>(connections.Where(c => c != connectionId));

        if (updatedConnections.IsEmpty)
            _userConnectionMap.TryRemove(userId, out _);
        else
            _userConnectionMap[userId] = updatedConnections;
    }

    public IEnumerable<string> GetUserConnections(string? userId)
    {
        if (userId != null && _userConnectionMap.TryGetValue(userId, out var connections)) return connections.ToList();
        return [];
    }

    public IEnumerable<string?> GetAllUsers()
    {
        return _userConnectionMap.Keys;
    }

    public string? GetUserByConnection(string connectionId)
    {
        _connectionUserMap.TryGetValue(connectionId, out var userId);
        return userId;
    }
}