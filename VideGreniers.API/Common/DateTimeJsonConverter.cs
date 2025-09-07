using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideGreniers.API.Common;

/// <summary>
/// Custom DateTime JSON converter that formats dates in ISO 8601 without microseconds
/// to ensure compatibility with iOS Swift date decoders
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

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
    private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

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

/// <summary>
/// Custom DateTimeOffset JSON converter that formats dates in ISO 8601 without microseconds
/// to ensure compatibility with iOS Swift date decoders
/// </summary>
public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Invalid DateTimeOffset value");
        }

        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
        {
            return dateTimeOffset;
        }

        throw new JsonException($"Unable to parse '{value}' as DateTimeOffset");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        // Convert to UTC and format without microseconds
        var utcValue = value.ToUniversalTime();
        writer.WriteStringValue(utcValue.ToString(DateTimeFormat));
    }
}

/// <summary>
/// Custom nullable DateTimeOffset JSON converter
/// </summary>
public class NullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    private const string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
        {
            return dateTimeOffset;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            // Convert to UTC and format without microseconds
            var utcValue = value.Value.ToUniversalTime();
            writer.WriteStringValue(utcValue.ToString(DateTimeFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}