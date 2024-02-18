using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Extensions;

using TestContext = XPing365.Sdk.Core.Components.TestContext;

namespace XPing365.Sdk.UnitTests.Extensions;

internal class TestContextExtensionTests
{
    [Test]
    public void GetNonSerializablePropertyBagValueThrowsArugmentNullExceptionWhenContextIsNull()
    {
        // Arrange
        TestContext context = null!;

        // Assert
        Assert.Throws<ArgumentNullException>(() => 
            context.GetNonSerializablePropertyBagValue<object>(new PropertyBagKey("key")));
    }
}
