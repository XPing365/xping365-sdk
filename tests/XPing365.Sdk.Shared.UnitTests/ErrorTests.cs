namespace XPing365.Sdk.Shared.UnitTests;

public sealed class ErrorTests
{
    [Test]
    public void ThrowsArgumentExceptionWhenCodeIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Error(code: null!, message: "message"));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenMessageIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Error(code: "code", message: null!));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenCodeIsNullAndMessageIsNull()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Error(code: null!, message: null!));
    }

    [Test]
    public void DoesNotThrowWhenCodeIsNotNullAndMessageIsNotNull()
    {
        // Assert
        Assert.DoesNotThrow(() => new Error(code: "code", message: "message"));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenCodeIsEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Error(code: string.Empty, message: "message"));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenMessageIsEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Error(code: "code", message: string.Empty));
    }

    [Test]
    public void ThrowsArgumentExceptionWhenCodeIsEmptyAndMessageIsEmpty()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => new Error(code: string.Empty, message: string.Empty));
    }

    [Test]
    public void ToStringReturnsSpecificString()
    {
        // Arrange
        var error = new Error(code: "101", message: "Error message");

        // Assert
        Assert.That(error.ToString(), Is.EqualTo($"Error 101: Error message"));
    }
}
