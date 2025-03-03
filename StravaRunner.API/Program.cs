using System.Text.Json.Serialization;
using StravaRunner.API.Endpoints;
using StravaRunner.Core;
using StravaRunner.Core.Models.Settings;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddCoreServices();

builder.Services.Configure<StravaAuthSettings>(builder.Configuration.GetSection(nameof(StravaAuthSettings)));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(nameof(SecuritySettings)));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(nameof(SmtpSettings)));

builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddMemoryCache();

var app = builder.Build();

// app.MapGet("/activities",
//     async (IStravaService stravaService, IMemoryCache cache) =>
//     {
//         const string clubId = "1401067";
//         const string cacheKey = $"strava-activities-{clubId}";
//
//         if (cache.TryGetValue(cacheKey, out var activityList))
//         {
//             return activityList;
//         }
//         
//         var loadedActivities = await stravaService.GetActivitiesAsync(clubId);
//         var cacheEntryOptions = new MemoryCacheEntryOptions
//         {
//             AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
//         };
//         cache.Set(cacheKey, loadedActivities, cacheEntryOptions);
//         
//         return loadedActivities;
//     });


app.MapAuthEndpoints();

app.Run();