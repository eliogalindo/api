namespace api.Modules.Auth.DTOs;

public class AuthDataDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}