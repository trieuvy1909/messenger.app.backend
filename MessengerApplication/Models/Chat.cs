using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public List<User> Users { get; set; } = new ();
}