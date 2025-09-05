using ErrorOr;

namespace VideGreniers.Domain.ValueObjects;

public sealed record Address
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }
    public string? State { get; }

    private Address(string street, string city, string postalCode, string country, string? state = null)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
        State = state;
    }

    public static ErrorOr<Address> Create(
        string street,
        string city,
        string postalCode,
        string country,
        string? state = null)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(street) || street.Length < 2)
        {
            errors.Add(Error.Validation("Address.InvalidStreet", "Street must be at least 2 characters long."));
        }

        if (street?.Length > 200)
        {
            errors.Add(Error.Validation("Address.StreetTooLong", "Street cannot exceed 200 characters."));
        }

        if (string.IsNullOrWhiteSpace(city) || city.Length < 2)
        {
            errors.Add(Error.Validation("Address.InvalidCity", "City must be at least 2 characters long."));
        }

        if (city?.Length > 100)
        {
            errors.Add(Error.Validation("Address.CityTooLong", "City cannot exceed 100 characters."));
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            errors.Add(Error.Validation("Address.InvalidPostalCode", "Postal code is required."));
        }

        if (postalCode?.Length > 20)
        {
            errors.Add(Error.Validation("Address.PostalCodeTooLong", "Postal code cannot exceed 20 characters."));
        }

        if (string.IsNullOrWhiteSpace(country) || country.Length < 2)
        {
            errors.Add(Error.Validation("Address.InvalidCountry", "Country must be at least 2 characters long."));
        }

        if (country?.Length > 100)
        {
            errors.Add(Error.Validation("Address.CountryTooLong", "Country cannot exceed 100 characters."));
        }

        if (!string.IsNullOrWhiteSpace(state) && state.Length > 100)
        {
            errors.Add(Error.Validation("Address.StateTooLong", "State cannot exceed 100 characters."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Address(street!, city!, postalCode!, country!, state);
    }

    public string GetFormattedAddress()
    {
        var parts = new List<string> { Street, City };
        
        if (!string.IsNullOrWhiteSpace(State))
        {
            parts.Add(State);
        }
        
        parts.AddRange(new[] { PostalCode, Country });

        return string.Join(", ", parts);
    }

    public override string ToString() => GetFormattedAddress();
}