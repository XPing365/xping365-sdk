using XPing365.Sdk.Core;

namespace XPing365.Sdk.UnitTests;

public sealed class PropertyBagKeyTests
{
    [Test]
    public void ThrowsExceptionWhenArgumentIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new PropertyBagKey(key: null!));
    }

    [Test]
    public void ThrowsExceptionWhenArgumentIsEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new PropertyBagKey(key: string.Empty));
    }

    [Test]
    public void DoesNotThrowsExceptionWhenArgumentIsNull()
    {
        // Assert
        Assert.DoesNotThrow(() => new PropertyBagKey(key: "key"));
    }
}
