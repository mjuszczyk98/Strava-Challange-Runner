using System.IdentityModel.Tokens.Jwt;
using MediatR;
using MongoDB.Driver;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models.Data;
using StravaRunner.Core.Services;

namespace StravaRunner.Core.Messages.Auth.Commands;

public record AuthenticateCommand(string Token) : IRequest<AuthDto>;

public class AuthenticateCommandHandler(IMongoDatabase database, IStravaApiService stravaApiService,
    ITokenService tokenService) 
    : IRequestHandler<AuthenticateCommand, AuthDto>
{
    public async Task<AuthDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var tokenCollection = database.GetDbCollection<LoginToken>();
        var userId = await tokenService.ValidateLoginTokenAndGetUserId(tokenCollection, request.Token, cancellationToken);
        
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var user = await database.GetDbCollection<User>()
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);

        var (userJwtToken, tokenExpiryDate) = tokenService.CreateJwtToken(user);
        var newRefreshToken = tokenService.CreateRefreshToken(user);
        
        await database.GetDbCollection<RefreshToken>()
            .InsertOneAsync(newRefreshToken, cancellationToken: cancellationToken);
        
        return new AuthDto
        {
            AccessToken = userJwtToken,
            RefreshToken = newRefreshToken.Token,
            ExpiredAt = tokenExpiryDate
        };
    }
}