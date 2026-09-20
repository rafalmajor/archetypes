using System;
using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.userjourney;

internal sealed class Condition : IEquatable<Condition>
{
    private readonly ConditionType typeValue;
    private readonly IDictionary<string, object> attributesValue;

    internal Condition(ConditionType type, IDictionary<string, object> attributes)
    {
        typeValue = type;
        attributesValue = attributes;
    }

    internal enum ConditionType
    {
        LATE_PAYMENT,
        PAYMENT_ON_TIME,
        RESTRUCTURING,
        PROMOTION_APPROVED
    }

    internal ConditionType type() => typeValue;

    internal IDictionary<string, object> attributes() => attributesValue;

    internal static Condition of(ConditionType type) => new(type, new Dictionary<string, object>());

    internal static Condition of(ConditionType type, IDictionary<string, object> attributes) =>
        new(type, new Dictionary<string, object>(attributes));

    internal static Condition latePayments(int count) =>
        new(ConditionType.LATE_PAYMENT, new Dictionary<string, object> { ["counter"] = count });

    internal static Condition paymentOnTime() => of(ConditionType.PAYMENT_ON_TIME);

    internal static Condition restructuring() => of(ConditionType.RESTRUCTURING);

    internal static Condition promotionApproved() => of(ConditionType.PROMOTION_APPROVED);

    internal static Condition withCost(ConditionType type, double cost) =>
        new(type, new Dictionary<string, object> { ["cost"] = cost });

    internal static Condition withTime(ConditionType type, int timeInDays) =>
        new(type, new Dictionary<string, object> { ["time"] = timeInDays });

    internal static Condition withRisk(ConditionType type, double riskScore) =>
        new(type, new Dictionary<string, object> { ["risk"] = riskScore });

    internal static Condition withAttributes(ConditionType type, double cost, int time, double risk) =>
        new(type, new Dictionary<string, object>
        {
            ["cost"] = cost,
            ["time"] = time,
            ["risk"] = risk
        });

    internal double getCost() => attributesValue.TryGetValue("cost", out object? value) ? (double)value : 1.0;

    internal int getTime() => attributesValue.TryGetValue("time", out object? value) ? (int)value : 1;

    internal double getRisk() => attributesValue.TryGetValue("risk", out object? value) ? (double)value : 0.0;

    public bool Equals(Condition? other) =>
        other is not null && typeValue == other.typeValue && DictionaryEquals(attributesValue, other.attributesValue);

    public override bool Equals(object? obj) => obj is Condition other && Equals(other);

    public override int GetHashCode()
    {
        int hash = (int)typeValue;
        foreach ((string key, object value) in attributesValue)
        {
            hash ^= HashCode.Combine(key, value);
        }

        return hash;
    }

    public override string ToString() => $"Condition[type={typeValue}, attributes={attributesValue}]";

    private static bool DictionaryEquals(
        IDictionary<string, object> first,
        IDictionary<string, object> second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        foreach ((string key, object value) in first)
        {
            if (!second.TryGetValue(key, out object? otherValue) || !Equals(value, otherValue))
            {
                return false;
            }
        }

        return true;
    }
}
