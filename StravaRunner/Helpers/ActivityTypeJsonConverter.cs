using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StravaRunner.Models;
using StravaRunner.Models.Strava;

namespace StravaRunner.Helpers;

public class ActivityTypeJsonConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (JsonSerializationException)
        {
            return ActivityType.None;
        }
    }
}