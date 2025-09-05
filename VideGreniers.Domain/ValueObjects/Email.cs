using System.Text.RegularExpressions;
using ErrorOr;
using VideGreniers.Domain.Common.Errors;

namespace VideGreniers.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static ErrorOr<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Errors.User.InvalidEmail;
        }

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalizedValue))
        {
            return Errors.User.InvalidEmail;
        }

        if (normalizedValue.Length > 320) // RFC 5321 limit
        {
            return Errors.User.InvalidEmail;
        }

        return new Email(normalizedValue);
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}