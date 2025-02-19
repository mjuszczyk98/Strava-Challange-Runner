using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using StravaRunner.Models.Settings;
using StravaRunner.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<StravaTokenSettings>(builder.Configuration.GetSection(nameof(StravaTokenSettings)));

builder.Services.AddSingleton<IStravaService, StravaService>();
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

app.Run();
