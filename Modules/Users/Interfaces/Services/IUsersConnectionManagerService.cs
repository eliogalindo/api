namespace api.Modules.Users.Interfaces.Services;

public interface IUsersConnectionManagerService
{
    void AddConnection(string? userId, string connectionId);
    void RemoveConnection(string connectionId);
    IEnumerable<string> GetUserConnections(string? userId);
    IEnumerable<string?> GetAllUsers();
    string? GetUserByConnection(string connectionId);
}