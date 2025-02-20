using MongoDB.Driver;
using StravaRunner.Models;
using StravaRunner.Models.Data;

namespace StravaRunner.Services;

public interface IUserService
{
    Task<User?> CreateUser(string email);
    Task<User?> AuthorizeUser(string email, string authorizationCode, string scope);
}

public class UserService(IMongoDatabase database, IStravaAuthService stravaAuthService) : IUserService
{
    private readonly IMongoDatabase _database = database;
    private readonly IStravaAuthService _stravaAuthService = stravaAuthService;
    
    public async Task<User?> CreateUser(string email)
    {
        var collection = GetUsersCollection();
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var existingUser = await collection.FindAsync(filter);
        if (await existingUser.AnyAsync())
        {
            return null;
        }
        
        var newUser = new User { Email = email };
        await collection.InsertOneAsync(newUser);
        
        return newUser;
    }

    public async Task<User?> AuthorizeUser(string email, string authorizationCode, string scope)
    {
        var securedResponse = await _stravaAuthService.AuthorizeUser(authorizationCode);
        var collection = GetUsersCollection();
        
        var newUser = new User
        {
            Email = email,
            FirstName = securedResponse.Value?.FirstName,
            LastName = securedResponse.Value?.LastName,
            City = securedResponse.Value?.City,
            Country = securedResponse.Value?.Country,
            AccessToken = securedResponse.SecuredAccessToken,
            RefreshToken = securedResponse.SecuredRefreshToken,
            TokenExpiresAtDateTime= securedResponse.ExpiresAtDateTime,
            Scope = scope
        };
        
        
        await collection.InsertOneAsync(newUser);
        return newUser;
        
    }
    
    private IMongoCollection<User> GetUsersCollection() => _database.GetCollection<User>("Users");
}