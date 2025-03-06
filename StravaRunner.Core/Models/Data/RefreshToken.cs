using MongoDB.Bson.Serialization.Attributes;
using StravaRunner.Core.Extensions;

namespace StravaRunner.Core.Models.Data;

public class RefreshToken : Entity
{
    [BsonIgnore]
    public required string Token
    {
        get => field;
        init
        {
            HashedToken = value.CreateHash();
            field = value;
        }
    }

    public required string UserId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    
    [BsonIgnoreIfNull]
    public byte[]? HashedToken { get; private set; }
}