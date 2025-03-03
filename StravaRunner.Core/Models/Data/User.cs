using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StravaRunner.Core.Models.Data;

public class User : Entity
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    public bool IsVerified { get; set; } = false;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public string? AccessToken { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public DateTimeOffset? TokenExpiresAtDateTime { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? RefreshToken { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? Scope { get; set; }
}