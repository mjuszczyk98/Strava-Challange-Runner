using MediatR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StravaRunner.Core.Constants;
using StravaRunner.Core.Extensions;
using StravaRunner.Core.Models.Data;
using StravaRunner.Core.Models.Settings;
using StravaRunner.Core.Services;

namespace StravaRunner.Core.Messages.Auth.Commands;

public record LoginCommand(string UserEmail, bool ForceReauthorization = false) : IRequest;

public class LoginCommandHandler(
    IMongoDatabase database,
    IEncryptionService encryptionService,
    IEmailService emailService,
    IOptions<StravaAuthSettings> authSettings,
    IOptions<EnvSettings> envSettings)
    : IRequestHandler<LoginCommand>
{
    public async Task Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usersCollection = database.GetDbCollection<User>();
        var filter = Builders<User>.Filter.Eq(u => u.Email, request.UserEmail);
        var existingUserCursor = await usersCollection.FindAsync(filter, cancellationToken: cancellationToken);
        
        var user = await existingUserCursor.FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            user = new User { Email = request.UserEmail };
            await usersCollection.InsertOneAsync(user, cancellationToken: cancellationToken);
        }

        var authorizeToken = Guid.NewGuid().ToString("N");
        var loginToken = new LoginToken
        {
            UserId = user.Id,
            TokenHash = authorizeToken.CreateHash(),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
        };
        
        var loginTokenCollection = database.GetDbCollection<LoginToken>();
        await loginTokenCollection.InsertOneAsync(loginToken, cancellationToken: cancellationToken);

        var authorizeUrl = !user.IsVerified || request.ForceReauthorization
            ? GetStravaAuthorizeUrl(authorizeToken)
            : GetAuthenticateUrl(authorizeToken);

        await emailService.SendEmailAsync(
            request.UserEmail,
            EmailConstants.LoginSubject, 
            authorizeUrl, 
            cancellationToken: cancellationToken);
    }
    
    private string GetStravaAuthorizeUrl(string context)
    {
        var requiredScopes = string.Join(",", StravaApi.Scopes.Read, StravaApi.Scopes.ActivityRead);
        var stravaAuthUrl = StravaApi.GetAuthorizeUrl(
            authSettings.Value.ClientId,
         "http://localhost:5000/api/auth/strava",
         requiredScopes,
            context);

        return stravaAuthUrl;
    }

    private string GetAuthenticateUrl(string token) => $"{envSettings.Value.AuthenticateUrl}?token={token}";
}