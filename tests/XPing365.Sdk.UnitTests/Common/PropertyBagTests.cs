using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.UnitTests.Common;

public sealed class PropertyBagTests
{
    [Test]
    public void IsEmptyWhenInstantiatedWithDefaultCtor()
    {
        // Arrange
        var propertyBag = new PropertyBag();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(propertyBag.Keys, Is.Empty);
            Assert.That(propertyBag.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void IsEmptyWhenInstantiatedWithEmptyProperties()
    {
        // Arrange
        var propertyBag = new PropertyBag(properties: (Dictionary<PropertyBagKey, object>)([]));

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(propertyBag.Keys, Is.Empty);
            Assert.That(propertyBag.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void IsNotEmptyWhenInstantiatedWithNotEmptyProperties()
    {
        // Arrange
        var propertyBag = new PropertyBag(properties: new Dictionary<PropertyBagKey, object>
        {
            { new PropertyBagKey("key"), new object() }
        });

        // Assert
        Assert.That(propertyBag.Keys, Is.Not.Empty);
        Assert.That(propertyBag.Count, Is.Not.EqualTo(0));
    }

    [Test]
    public void HasOneItemWhenAddedOneItem()
    {
        // Arrange
        const int expectedItemsCount = 1;
        const string keyName = "key";

        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(new PropertyBagKey(keyName), new object());

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(propertyBag.Keys, Has.Count.EqualTo(expectedItemsCount));
            Assert.That(propertyBag.Count, Is.EqualTo(expectedItemsCount));
        });
    }

    [Test]
    public void HasOneItemWhenUpdatedItemAfterAdd()
    {
        // Arrange
        const int expectedItemsCount = 1;
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());
        propertyBag.AddOrUpdateProperty(key, new object());

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(propertyBag.Keys, Has.Count.EqualTo(expectedItemsCount));
            Assert.That(propertyBag.Count, Is.EqualTo(expectedItemsCount));
        });
    }

    [Test]
    public void ValueChangesWhenItemUpdated()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());
        propertyBag.AddOrUpdateProperty(key, new string(""));

        // Assert
        Assert.That(propertyBag.GetProperty(key), Is.TypeOf<string>());
    }

    [Test]
    public void ContainsKeyReturnsTrueWhenFoundKey()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.That(propertyBag.ContainsKey(key), Is.True);
    }

    [Test]
    public void ContainsKeyReturnsFalseWhenNotFoundKey()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var newKey = new PropertyBagKey("newKey");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.That(propertyBag.ContainsKey(newKey), Is.False);
    }

    [Test]
    public void AddOrUpdatePropertyThrowsExceptionWhenKeyIsNull()
    {
        // Arrange
        PropertyBagKey key = null!;
        var propertyBag = new PropertyBag();

        // Assert
        Assert.Throws<ArgumentNullException>(() => propertyBag.AddOrUpdateProperty(key, new object()));
    }

    [Test]
    public void AddOrUpdatePropertyThrowsExceptionWhenValueIsNull()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Assert
        Assert.Throws<ArgumentNullException>(() => propertyBag.AddOrUpdateProperty(key, null!));
    }

    [Test]
    public void AddOrUpdatePropertiesThrowsExceptionWhenPropertiesParameterIsNull()
    {
        // Arrange
        Dictionary<PropertyBagKey, object> properties = null!;
        var propertyBag = new PropertyBag();

        // Assert
        Assert.Throws<ArgumentNullException>(() => propertyBag.AddOrUpdateProperties(properties));
    }

    [Test]
    public void HasCorrectNumberOfItemsAfterAddOrUpdateProperties()
    {
        // Arrange
        var properties = new Dictionary<PropertyBagKey, object>()
        {
            { new PropertyBagKey("key"), new object() },
            { new PropertyBagKey("yek"), new object() }
        };
        int expectedItemsCount = properties.Count;
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperties(properties);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(propertyBag.Keys, Has.Count.EqualTo(expectedItemsCount));
            Assert.That(propertyBag.Count, Is.EqualTo(expectedItemsCount));
        });
    }

    [Test]
    public void DoesNotThrowWhenDuplicateKeyIsAddOrUpdated()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.DoesNotThrow(() => propertyBag.AddOrUpdateProperty(key, new object()));
    }

    [Test]
    public void TryGetPropertyReturnsTrueWhenPropertyExist()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.That(propertyBag.TryGetProperty(key, out _), Is.True);
    }

    [Test]
    public void TryGetPropertyReturnsFalseWhenPropertyDoesNotExist()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.That(propertyBag.TryGetProperty(new PropertyBagKey("not_foudn"), out _), Is.False);
    }

    [Test]
    public void TryGetPropertyReturnsNullValueWhenPropertyDoesNotExist()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());
        propertyBag.TryGetProperty(new PropertyBagKey("not_foudn"), out var value);

        // Assert
        Assert.That(value, Is.Null);
    }

    [Test]
    public void GenericTryGetPropertyReturnsTrueWhenPropertyExist()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.That(propertyBag.TryGetProperty<object>(key, out _), Is.True);
    }

    [Test]
    public void GenericTryGetPropertyReturnsFalseWhenPropertyDoesNotExist()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.That(propertyBag.TryGetProperty<object>(new PropertyBagKey("not_foudn"), out _), Is.False);
    }

    [Test]
    public void GenericTryGetPropertyReturnsNullValueWhenPropertyDoesNotExist()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());
        propertyBag.TryGetProperty<object>(new PropertyBagKey("not_foudn"), out var value);

        // Assert
        Assert.That(value, Is.Null);
    }

    [Test]
    public void GenericTryGetPropertyDoesNotThrowInvalidCastExceptionWhenTypeMismatch()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert
        Assert.DoesNotThrow(() => propertyBag.TryGetProperty<string>(key, out var value));
    }

    [Test]
    public void GetPropertyThrowsExceptionWhenKeyIsNull()
    {
        // Arrange
        PropertyBagKey key = null!;
        var propertyBag = new PropertyBag();

        // Assert
        Assert.Throws<ArgumentNullException>(() => propertyBag.GetProperty(key));
    }

    [Test]
    public void GetPropertyThrowsKeyNotFoundExceptionWhenKeyDoesNotExist()
    {
        // Arrange
        var propertyBag = new PropertyBag();

        // Assert            
        Assert.Throws<KeyNotFoundException>(() => propertyBag.GetProperty(new PropertyBagKey("no_found")));
    }

    [Test]
    public void GenericGetPropertyThrowsExceptionWhenKeyIsNull()
    {
        // Arrange
        PropertyBagKey key = null!;
        var propertyBag = new PropertyBag();

        // Assert
        Assert.Throws<ArgumentNullException>(() => propertyBag.GetProperty<object>(key));
    }

    [Test]
    public void GenericGetPropertyThrowsKeyNotFoundExceptionWhenKeyDoesNotExist()
    {
        // Arrange
        var propertyBag = new PropertyBag();

        // Assert            
        Assert.Throws<KeyNotFoundException>(() => propertyBag.GetProperty<object>(new PropertyBagKey("no_found")));
    }

    [Test]
    public void GenericGetPropertyThrowsInvalidCastExceptionWhenTypeMismatch()
    {
        // Arrange
        var key = new PropertyBagKey("key");
        var propertyBag = new PropertyBag();

        // Act
        propertyBag.AddOrUpdateProperty(key, new object());

        // Assert            
        Assert.Throws<InvalidCastException>(() => propertyBag.GetProperty<string>(key));
    }

    [Test]
    public void ClearRemovesAllPropertes()
    {
        // Arrange
        var propertyBag = new PropertyBag(new Dictionary<PropertyBagKey, object>
        {
            { new PropertyBagKey("key"), new object() },
            { new PropertyBagKey("yek"), new object() }
        });

        // Act
        propertyBag.Clear();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(propertyBag.Keys, Is.Empty);
            Assert.That(propertyBag.Count, Is.EqualTo(0));
        });
    }
}
