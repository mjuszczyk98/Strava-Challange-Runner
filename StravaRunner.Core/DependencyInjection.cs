using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StravaRunner.Core.Models.Settings;
using StravaRunner.Core.Services;

namespace StravaRunner.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddMediatR(config => 
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IStravaApiService, StravaApiService>();

        return services;
    }
}