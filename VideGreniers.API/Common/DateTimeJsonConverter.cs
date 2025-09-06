using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideGreniers.API.Common;

/// <summary>
/// Custom DateTime JSON converter that formats dates in ISO 8601 without microseconds
/// to ensure compatibility with iOS Swift date decoders
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Invalid DateTime value");
        }

        if (DateTime.TryParse(value, out var dateTime))
        {
            return dateTime;
        }

        throw new JsonException($"Unable to parse '{value}' as DateTime");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Convert to UTC and format without microseconds
        var utcValue = value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
        writer.WriteStringValue(utcValue.ToString(DateTimeFormat));
    }
}

/// <summary>
/// Custom nullable DateTime JSON converter
/// </summary>
public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (DateTime.TryParse(value, out var dateTime))
        {
            return dateTime;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            // Convert to UTC and format without microseconds
            var utcValue = value.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) 
                : value.Value.ToUniversalTime();
            writer.WriteStringValue(utcValue.ToString(DateTimeFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}