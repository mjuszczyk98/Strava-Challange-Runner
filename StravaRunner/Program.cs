using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StravaRunner.Constants;
using StravaRunner.Models.Settings;
using StravaRunner.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<StravaAuthSettings>(builder.Configuration.GetSection(nameof(StravaAuthSettings)));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(nameof(SecuritySettings)));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddScoped<IStravaAuthService, StravaAuthService>();
builder.Services.AddScoped<IStravaService, StravaService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMemoryCache();

var app = builder.Build();

app.MapGet("/activities",
    async (IStravaService stravaService, IMemoryCache cache) =>
    {
        const string clubId = "1401067";
        const string cacheKey = $"strava-activities-{clubId}";

        if (cache.TryGetValue(cacheKey, out var activityList))
        {
            return activityList;
        }
        
        var loadedActivities = await stravaService.GetActivitiesAsync(clubId);
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };
        cache.Set(cacheKey, loadedActivities, cacheEntryOptions);
        
        return loadedActivities;
    });


app.MapGet("/users", async (
    IUserService userService,
    IOptions<StravaAuthSettings> stravaSettings,
string? email) =>
{
    var requiredScopes = string.Join(",", StravaApi.Scopes.Read, StravaApi.Scopes.ActivityRead);
    var redirectUlr = StravaApi.GetAuthorizeUrl(
        stravaSettings.Value.ClientId,
        "http://localhost:5000/users/authorize",
        requiredScopes,
        email);
    
    //var createdUser = await userService.CreateUser(email);
    return Results.Redirect(redirectUlr);
});

app.MapGet("/users/authorize", async (IUserService userService, string state, string code, string scope) =>
{
    var createdUser = await userService.AuthorizeUser(state, code, scope);
    return createdUser;
});

app.Run();

record UserRequest(string Email);
record UR (string State, string Code, string Scope);