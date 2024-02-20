using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Session;

namespace XPing365.Sdk.Core.Extensions;

public static class TestSessionExtension
{
    public static TValue? GetNonSerializablePropertyBagValue<TValue>(this TestSession session, PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.TryGetPropertyBagValue(key, out NonSerializable<TValue>? popertyBagValue) &&
            popertyBagValue != null)
        {
            return popertyBagValue.Value;
        }

        return default;
    }

    public static TValue? GetPropertyBagValue<TValue>(this TestSession session, PropertyBagKey key)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.TryGetPropertyBagValue(key, out PropertyBagValue<TValue>? popertyBagValue) &&
            popertyBagValue != null)
        {
            return popertyBagValue.Value;
        }

        return default;
    }

    public static bool TryGetPropertyBagValue<TValue>(
        this TestSession session,
        PropertyBagKey key,
        out PropertyBagValue<TValue>? value)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        PropertyBagValue<TValue>? propertyBagValue = null;
        value = default;

        if (session.Steps.FirstOrDefault(step =>
            step.PropertyBag != null &&
            step.PropertyBag.TryGetProperty(key, out propertyBagValue)) != null &&
            propertyBagValue != null)
        {
            value = propertyBagValue;
            return true;
        }

        return false;
    }

    public static bool TryGetPropertyBagValue<TValue>(
        this TestSession session,
        PropertyBagKey key,
        out NonSerializable<TValue>? value)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        NonSerializable<TValue>? propertyBagValue = null;
        value = default;

        if (session.Steps.FirstOrDefault(step =>
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
