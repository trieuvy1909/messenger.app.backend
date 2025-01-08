using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class User :UserSummary
{
    public List<string> Chats { get; set; } = new();
    public string Password { get; set; } = null!;
    public List<Friends> Friends { get; set; } = new();
}
public class UserSummary
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Status { get; set; } = "online";
    public Profile Profile { get; set; } = new();
    public bool IsAdmin { get; set; } = false;
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
}
public class Profile
{
    public string FullName { get; set; } = null!;
    public string Avatar { get; set; } = null!;
    public string Bio { get; set; } = null!;
}

public class Friends
{
    public string UserId { get; set; } = null!;
    public string Status { get; set; } = null!;
}