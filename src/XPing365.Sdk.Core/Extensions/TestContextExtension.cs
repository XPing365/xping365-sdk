using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;

namespace XPing365.Sdk.Core.Extensions;

public static class TestContextExtension
{
    public static TValue? GetNonSerializablePropertyBagValue<TValue>(this TestContext context, PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.TryGetPropertyBagValue(key, out NonSerializable<TValue>? popertyBagValue) &&
            popertyBagValue != null)
        {
            return popertyBagValue.Value;
        }

        return default;
    }

    public static TValue? GetPropertyBagValue<TValue>(this TestContext context, PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.TryGetPropertyBagValue(key, out PropertyBagValue<TValue>? popertyBagValue) &&
            popertyBagValue != null)
        {
            return popertyBagValue.Value;
        }

        return default;
    }

    internal static bool TryGetPropertyBagValue<TValue>(
        this TestContext context,
        PropertyBagKey key,
        out PropertyBagValue<TValue>? value) 
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        PropertyBagValue<TValue>? propertyBagValue = null;
        value = default;

        if (context.SessionBuilder.Steps.FirstOrDefault(step =>
            step.PropertyBag != null &&
            step.PropertyBag.TryGetProperty(key, out propertyBagValue)) != null &&
            propertyBagValue != null)
        {
            value = propertyBagValue;
            return true;
        }

        return false;
    }

    internal static bool TryGetPropertyBagValue<TValue>(
        this TestContext context,
        PropertyBagKey key,
        out NonSerializable<TValue>? value)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        NonSerializable<TValue>? propertyBagValue = null;
        value = default;

        if (context.SessionBuilder.Steps.FirstOrDefault(step =>
            step.PropertyBag != null &&
            step.PropertyBag.TryGetProperty(key, out propertyBagValue)) != null &&
            propertyBagValue != null)
        {
            value = propertyBagValue;
            return true;
        }

        return false;
    }
}
