using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.UserJourney;

internal sealed class State : IEquatable<State>
{
    private readonly ISet<Product> productsValue;

    internal State(ISet<Product> products)
    {
        productsValue = products;
    }

    internal ISet<Product> Products() => productsValue;

    internal static State Empty() => new(new HashSet<Product>());

    internal static State Of(params Product[] products) => new(new HashSet<Product>(products));

    internal State WithProduct(Product product)
    {
        HashSet<Product> newProducts = new(productsValue) { product };
        return new State(newProducts);
    }

    internal bool Contains(Product.ProductType productType)
    {
        foreach (Product product in productsValue)
        {
            if (product.Type() == productType)
            {
                return true;
            }
        }

        return false;
    }

    public bool Equals(State? other) => other is not null && productsValue.SetEquals(other.productsValue);

    public override bool Equals(object? obj) => obj is State other && Equals(other);

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (Product product in productsValue)
        {
            hash ^= product.GetHashCode();
        }

        return hash;
    }
}
