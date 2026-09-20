using System;
using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.userjourney;

internal sealed class Product : IEquatable<Product>
{
    private readonly ProductType typeValue;
    private readonly IDictionary<string, object> attributesValue;

    internal Product(ProductType type, IDictionary<string, object> attributes)
    {
        typeValue = type;
        attributesValue = attributes;
    }

    internal enum ProductType
    {
        NEW_LOAN,
        PENALTY,
        DISCOUNT
    }

    internal ProductType type() => typeValue;

    internal IDictionary<string, object> attributes() => attributesValue;

    internal static Product of(ProductType type) => new(type, new Dictionary<string, object>());

    internal static Product of(ProductType type, IDictionary<string, object> attributes) =>
        new(type, new Dictionary<string, object>(attributes));

    internal static Product newLoan() => of(ProductType.NEW_LOAN);

    internal static Product penalty() => of(ProductType.PENALTY);

    internal static Product discount(int percentage) =>
        new(ProductType.DISCOUNT, new Dictionary<string, object> { ["percentage"] = percentage });

    public bool Equals(Product? other)
    {
        if (other is null || typeValue != other.typeValue || attributesValue.Count != other.attributesValue.Count)
        {
            return false;
        }

        foreach ((string key, object value) in attributesValue)
        {
            if (!other.attributesValue.TryGetValue(key, out object? otherValue) || !Equals(value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is Product other && Equals(other);

    public override int GetHashCode()
    {
        int hash = (int)typeValue;
        foreach ((string key, object value) in attributesValue)
        {
            hash ^= HashCode.Combine(key, value);
        }

        return hash;
    }
}
