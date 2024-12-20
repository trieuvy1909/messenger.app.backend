using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? Title { get; set; }
    public List<UserSummary> Members { get; set; } = new ();
    public UserSummary? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(7);
}