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
    [BsonElement("CreatedAt")]
    public string CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7).ToString("HH:mm dd/MM/yyyy");
}
public class Profile
{
    public string FullName { get; set; } = null!;
    public string Avatar { get; set; } = "https://chiemtaimobile.vn/images/companies/1/%E1%BA%A2nh%20Blog/avatar-facebook-dep/Anh-avatar-hoat-hinh-de-thuong-xinh-xan.jpg?1704788263223";
    public string Bio { get; set; } = null!;
}

public class Friends
{
    public string UserId { get; set; } = null!;
    public string Status { get; set; } = null!;
}