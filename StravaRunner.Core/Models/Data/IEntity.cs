using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StravaRunner.Core.Models.Data;

public interface IEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] 
    public string Id { get; set; }
}

public class Entity : IEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
}