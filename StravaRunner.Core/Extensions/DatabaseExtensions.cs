using Humanizer;
using MongoDB.Driver;
using StravaRunner.Core.Models.Data;

namespace StravaRunner.Core.Extensions;

public static class DatabaseExtensions
{
    public static IMongoCollection<T> GetDbCollection<T>(this IMongoDatabase database)
        where T : class, IEntity
    {
        var collectionName = typeof(T).Name.Pluralize();
        
        return database.GetCollection<T>(collectionName);
    }
}