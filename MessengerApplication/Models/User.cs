using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Username { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public List<string> Chats { get; set; } = new();
}