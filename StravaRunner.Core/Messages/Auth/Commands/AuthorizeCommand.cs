using MediatR;
using MongoDB.Driver;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models;
using StravaRunner.Core.Models.Data;
using StravaRunner.Core.Models.Strava;
using StravaRunner.Core.Services;

namespace StravaRunner.Core.Messages.Auth.Commands;

public record AuthorizeCommand(string State, string AuthorizationCode) : IRequest<User>;

public class AuthorizeCommandHandler(IMongoDatabase database, IStravaApiService stravaApiService,
    ITokenService tokenService)
    : IRequestHandler<AuthorizeCommand, User>
{
    
    public async Task<User> Handle(AuthorizeCommand request, CancellationToken cancellationToken)
    {
        var tokenCollection = database.GetDbCollection<LoginToken>();
        var userId = await tokenService.ValidateLoginTokenAndGetUserId(tokenCollection, request.State, cancellationToken);
        
        var securedResponse = await stravaApiService.AuthorizeUser(request.AuthorizationCode);
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = CreateUpdateRequest(securedResponse);
        await database
            .GetDbCollection<User>()
            .FindOneAndUpdateAsync(filter, update, cancellationToken: cancellationToken);
        
        var user = await database.GetDbCollection<User>()
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);
        
        return user;
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