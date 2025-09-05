using ErrorOr;

namespace VideGreniers.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static ErrorOr<Money> Create(decimal amount, string currency)
    {
        var errors = new List<Error>();

        if (amount < 0)
        {
            errors.Add(Error.Validation("Money.NegativeAmount", "Amount cannot be negative."));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            errors.Add(Error.Validation("Money.InvalidCurrency", "Currency is required."));
        }
        else if (currency.Length != 3)
        {
            errors.Add(Error.Validation("Money.InvalidCurrencyCode", "Currency must be a 3-letter ISO code."));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        return new Money(Math.Round(amount, 2), currency!.ToUpperInvariant());
    }

    public static ErrorOr<Money> CreateEuros(decimal amount)
    {
        return Create(amount, "EUR");
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");
        }

        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new InvalidOperationException("Result cannot be negative");
        }

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Factor cannot be negative", nameof(factor));
        }

        return new Money(Math.Round(Amount * factor, 2), Currency);
    }

    public bool IsZero => Amount == 0;

    public string GetFormattedValue()
    {
        return Currency switch
        {
            "EUR" => $"€{Amount:F2}",
            "USD" => $"${Amount:F2}",
            "GBP" => $"£{Amount:F2}",
            _ => $"{Amount:F2} {Currency}"
        };
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => !(left < right);
    public static bool operator <=(Money left, Money right) => !(left > right);

    public override string ToString() => GetFormattedValue();
}