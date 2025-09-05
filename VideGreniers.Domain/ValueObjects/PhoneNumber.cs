using System.Text.RegularExpressions;
using ErrorOr;
using VideGreniers.Domain.Common.Errors;

namespace VideGreniers.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{7,14}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static ErrorOr<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Errors.User.InvalidPhoneNumber;
        }

        // Remove all non-digit characters except +
        var cleaned = Regex.Replace(value, @"[^\d+]", "");

        if (!PhoneRegex.IsMatch(cleaned))
        {
            return Errors.User.InvalidPhoneNumber;
        }

        return new PhoneNumber(cleaned);
    }

    /// <summary>
    /// Format the phone number for display (e.g., +33 6 12 34 56 78)
    /// </summary>
    public string GetFormattedValue()
    {
        if (Value.StartsWith("+33") && Value.Length == 12) // French number
        {
            return $"+33 {Value[3]} {Value[4..6]} {Value[6..8]} {Value[8..10]} {Value[10..12]}";
        }

        if (Value.StartsWith("+1") && Value.Length == 12) // US/Canada number
        {
            return $"+1 ({Value[2..5]}) {Value[5..8]}-{Value[8..12]}";
        }

        // Default formatting: +XX XXX XXX XXX
        if (Value.StartsWith("+") && Value.Length > 4)
        {
            var country = Value[..3];
            var remaining = Value[3..];
            var formatted = country + " ";
            
            for (int i = 0; i < remaining.Length; i += 3)
            {
                var chunk = remaining.Substring(i, Math.Min(3, remaining.Length - i));
                formatted += chunk + " ";
            }

            return formatted.Trim();
        }

        return Value;
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    public override string ToString() => GetFormattedValue();
}