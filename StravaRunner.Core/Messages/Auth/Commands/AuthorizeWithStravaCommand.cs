using MediatR;
using MongoDB.Driver;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models;
using StravaRunner.Core.Models.Data;
using StravaRunner.Core.Models.Strava;
using StravaRunner.Core.Services;

namespace StravaRunner.Core.Messages.Auth.Commands;

public record AuthorizeWithStravaCommand(string State, string AuthorizationCode) : IRequest;

public class AuthorizeWithStravaHandler(IMongoDatabase database, IStravaApiService stravaApiService, IEncryptionService encryptionService)
    : IRequestHandler<AuthorizeWithStravaCommand>
{
    private readonly IMongoDatabase _database = database;
    private readonly IStravaApiService _stravaApiService = stravaApiService;
    private readonly IEncryptionService _encryptionService = encryptionService;
    
    public async Task Handle(AuthorizeWithStravaCommand request, CancellationToken cancellationToken)
    {
        var userId = await ValidateTokenAndGetUserId(request.State, cancellationToken);
        
        var securedResponse = await _stravaApiService.AuthorizeUser(request.AuthorizationCode);
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = CreateUpdateRequest(securedResponse);
        await _database
            .GetDbCollection<User>()
            .FindOneAndUpdateAsync(filter, update, cancellationToken: cancellationToken);
    }

    private async Task<string> ValidateTokenAndGetUserId(string token, CancellationToken cancellationToken)
    {
        var tokenFilter = Builders<LoginToken>.Filter
            .Eq(t => t.TokenHash, token.CreateHash());
        var loginTokenCollection = _database.GetDbCollection<LoginToken>();
        var loginTokenCursor = await loginTokenCollection.FindAsync(tokenFilter, cancellationToken: cancellationToken);
        var loginToken = await loginTokenCursor.FirstOrDefaultAsync(cancellationToken);

        if (loginToken is null || !loginToken.IsValid)
        {
            throw new UnauthorizedAccessException("Invalid login token");
        }
        
        var tokenUpdate = Builders<LoginToken>.Update.Set(t => t.IsUsed, true);
        await loginTokenCollection.UpdateOneAsync(tokenFilter, tokenUpdate, cancellationToken: cancellationToken);
        
        return loginToken.UserId;
    }

    private static UpdateDefinition<User> CreateUpdateRequest(SecuredTokenWrapper<Athlete> securedResponse)
    {
        return Builders<User>.Update
            .Set(u => u.FirstName, securedResponse.Value?.FirstName)
            .Set(u => u.LastName, securedResponse.Value?.LastName)
            .Set(u => u.City, securedResponse.Value?.City)
            .Set(u => u.Country, securedResponse.Value?.Country)
            .Set(u => u.AccessToken, securedResponse.SecuredAccessToken)
            .Set(u => u.RefreshToken, securedResponse.SecuredRefreshToken)
            .Set(u => u.TokenExpiresAtDateTime, securedResponse.ExpiresAtDateTime)
            .Set(u => u.IsVerified, true);
    }
}