using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApplication.Models;

public class Message
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string ChatId { get; set; } = null!;
    public string Sender { get; set; } = null!;
    public string Payload { get; set; } = null!;
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Date { get; set; }
}