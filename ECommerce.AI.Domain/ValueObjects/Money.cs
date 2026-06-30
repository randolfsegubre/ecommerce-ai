using System.Text.Json.Serialization;

namespace ECommerce.AI.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary amounts
/// </summary>
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    [JsonConstructor]
    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");
        
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");
        
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        return new Money(Amount * multiplier, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {Currency} and {other.Currency}");
        
        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {Currency} and {other.Currency}");
        
        return Amount < other.Amount;
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public override string ToString() => $"{Amount:C} {Currency}";

    public static implicit operator decimal(Money money) => money.Amount;
    public static implicit operator Money(decimal amount) => new(amount);
}