using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
public class DisplayStatAttribute : Attribute
{
    public int Order { get; }
    public float MaxValue { get; }
    public float MinValue { get; }

    public Func<object, float> GetNonFloatValue { get; set; } = null;

    public string StatName { get; set; }

    public DisplayStatAttribute(int order, float minValue, float maxValue)
    {
        Order = order;
        MinValue = minValue;
        MaxValue = maxValue;
    }
}
