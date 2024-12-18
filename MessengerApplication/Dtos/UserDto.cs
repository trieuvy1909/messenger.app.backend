namespace MessengerApplication.Dtos;

public class UserDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public List<string>? Chats { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? PasswordHash { get; set; }
}