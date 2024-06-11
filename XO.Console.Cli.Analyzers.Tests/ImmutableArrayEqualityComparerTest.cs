using System.Collections.Immutable;

namespace XO.Console.Cli;

public class ImmutableArrayEqualityComparerTest
{
    [Fact]
    public void Equals_WhenArraysAreEqual_ReturnsTrue()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(1, 2, 3);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(array1, array2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WhenArraysAreNotEqual_ReturnsFalse()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(4, 5, 6);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(array1, array2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenArraysAreNotEqualByLength_ReturnsFalse()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(1, 2);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(array1, array2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenArraysAreNotEqualByReference_ReturnsFalse()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(array1, default);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenListsAreEqual_ReturnsTrue()
    {
        // Arrange
        var list1 = ImmutableList.Create(1, 2, 3);
        var list2 = ImmutableList.Create(1, 2, 3);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(list1, list2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WhenListsAreNotEqual_ReturnsFalse()
    {
        // Arrange
        var list1 = ImmutableList.Create(1, 2, 3);
        var list2 = ImmutableList.Create(4, 5, 6);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(list1, list2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenListsAreNotEqualByLength_ReturnsFalse()
    {
        // Arrange
        var list1 = ImmutableList.Create(1, 2, 3);
        var list2 = ImmutableList.Create(1, 2);

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(list1, list2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WhenListsAreNotEqualByReference_ReturnsFalse()
    {
        // Arrange
        var list1 = ImmutableList.Create(1, 2, 3);

        // Act
        var result1 = ImmutableArrayEqualityComparer.Equals(list1, null);
        var result2 = ImmutableArrayEqualityComparer.Equals(null, list1);

        // Assert
        Assert.Multiple(
            () => Assert.False(result1),
            () => Assert.False(result2));
    }

    [Fact]
    public void Equals_WhenListsAreNull_ReturnsTrue()
    {
        // Arrange
        ImmutableList<int>? list1 = null;
        ImmutableList<int>? list2 = null;

        // Act
        var result = ImmutableArrayEqualityComparer.Equals(list1, list2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_ReturnsSameValueForEqualArrays()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(1, 2, 3);

        // Act
        var hashCode1 = ImmutableArrayEqualityComparer.GetHashCode(array1);
        var hashCode2 = ImmutableArrayEqualityComparer.GetHashCode(array2);

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ReturnsZero_WhenArrayIsDefault()
    {
        // Arrange
        ImmutableArray<int> array1 = default;

        // Act
        var hashCode1 = ImmutableArrayEqualityComparer.GetHashCode(array1);

        // Assert
        Assert.Equal(0, hashCode1);
    }

    [Fact]
    public void GetHashCode_ReturnsDifferentValueForDifferentArrays()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(4, 5, 6);

        // Act
        var hashCode1 = ImmutableArrayEqualityComparer.GetHashCode(array1);
        var hashCode2 = ImmutableArrayEqualityComparer.GetHashCode(array2);

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetMatchLength_ReturnsZero_WhenArraysAreEmpty()
    {
        // Arrange
        var array1 = ImmutableArray<int>.Empty;
        var array2 = ImmutableArray<int>.Empty;

        // Act
        var result = ImmutableArrayEqualityComparer.GetMatchLength(array1, array2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetMatchLength_ReturnsZero_WhenArraysAreDefault()
    {
        // Arrange
        ImmutableArray<int> array1 = default;
        ImmutableArray<int> array2 = default;

        // Act
        var result = ImmutableArrayEqualityComparer.GetMatchLength(array1, array2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetMatchLength_ReturnsZero_WhenArraysAreDifferent()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(4, 5, 6);

        // Act
        var result = ImmutableArrayEqualityComparer.GetMatchLength(array1, array2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetMatchLength_ReturnsLength_WhenArraysAreEqual()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(1, 2, 3);

        // Act
        var result = ImmutableArrayEqualityComparer.GetMatchLength(array1, array2);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetMatchLength_ReturnsLength_WhenArraysArePartiallyEqual()
    {
        // Arrange
        var array1 = ImmutableArray.Create(1, 2, 3);
        var array2 = ImmutableArray.Create(1, 2, 4);

        // Act
        var result = ImmutableArrayEqualityComparer.GetMatchLength(array1, array2);

        // Assert
        Assert.Equal(2, result);
    }
}
