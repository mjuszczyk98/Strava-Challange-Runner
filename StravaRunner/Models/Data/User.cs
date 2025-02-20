using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace StravaRunner.Models.Data;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] 
    public string? Id { get; set; }
    
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public string? AccessToken { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTimeOffset? TokenExpiresAtDateTime { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? RefreshToken { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? Scope { get; set; }
}