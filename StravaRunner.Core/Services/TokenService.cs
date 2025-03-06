using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models.Data;
using StravaRunner.Core.Models.Settings;

namespace StravaRunner.Core.Services;

public interface ITokenService
{
    Task<string> ValidateLoginTokenAndGetUserId(IMongoCollection<LoginToken> collection, string token,
        CancellationToken cancellationToken);
    
    (string token, DateTimeOffset expiresAt) CreateJwtToken(User user);
    RefreshToken CreateRefreshToken(User user);
}

public class TokenService(IOptions<JwtSettings> jwtSettings) : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    
    public async Task<string> ValidateLoginTokenAndGetUserId(IMongoCollection<LoginToken> collection, string token,
        CancellationToken cancellationToken)
    {
        var tokenFilter = Builders<LoginToken>.Filter
                .And(
                    Builders<LoginToken>.Filter.Eq(t => t.TokenHash, token.CreateHash()),
                    Builders<LoginToken>.Filter.Eq(t => t.IsUsed, false),
                    Builders<LoginToken>.Filter.Gte(t => t.ExpiresAt, DateTimeOffset.Now)
                );
        var loginTokenCursor = await collection.FindAsync(tokenFilter, cancellationToken: cancellationToken);
        var loginToken = await loginTokenCursor.FirstOrDefaultAsync(cancellationToken);

        if (loginToken is null || !loginToken.IsValid)
        {
            throw new UnauthorizedAccessException("Invalid login token");
        }
        
        var tokenUpdate = Builders<LoginToken>.Update.Set(t => t.IsUsed, true);
        await collection.UpdateOneAsync(tokenFilter, tokenUpdate, cancellationToken: cancellationToken);
        
        return loginToken.UserId;
    }

    public (string token, DateTimeOffset expiresAt) CreateJwtToken(User user)
    {
        var expiresAt = DateTimeOffset.Now.AddMinutes(_jwtSettings.ExpiresInMinutes);
        List<Claim> claims = 
        [
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
            new ("email", user.Email),
            new ("role", user.Scope ?? string.Empty)
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
    
    public RefreshToken CreateRefreshToken(User user) => new ()
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays)
        };
}