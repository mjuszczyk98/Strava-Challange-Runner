using MediatR;
using MongoDB.Driver;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models.Data;
using StravaRunner.Core.Services;

namespace StravaRunner.Core.Messages.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthDto>;

public class RefreshTokenCommandHandler(IMongoDatabase database, ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, AuthDto>
{
    public async Task<AuthDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenCollection = database.GetDbCollection<RefreshToken>();
        var dbRefreshToken = await refreshTokenCollection
            .Find(rt => rt.Token == request.RefreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (dbRefreshToken is null)
        {
            return null;
        }

        if (dbRefreshToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            await refreshTokenCollection.DeleteOneAsync(rt => rt.Token == request.RefreshToken, cancellationToken);
            return null;
        }
        
        var user = await database.GetDbCollection<User>()
            .Find(Builders<User>.Filter.Eq(u => u.Id, dbRefreshToken.UserId))
            .SingleOrDefaultAsync(cancellationToken);

        var (newJwtToken, tokenExpiryDate) = tokenService.CreateJwtToken(user);
        var newRefreshToken = tokenService.CreateRefreshToken(user);

        await refreshTokenCollection.DeleteOneAsync(rt => rt.Token == request.RefreshToken, cancellationToken);
        await refreshTokenCollection.InsertOneAsync(newRefreshToken, cancellationToken: cancellationToken);

        return new AuthDto
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken.Token,
            ExpiredAt = tokenExpiryDate
        };
    }
}