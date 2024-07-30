using FluentAssertions;
using Moq;

namespace VSlices.Domain.UnitTests;

public class EntityTests
{
    [Fact]
    public void ToString_ShouldStringWithReturnEntityAndKeyInfo()
    {
        // Arrange
        const string key1 = "1";

        var entityMock = new Mock<Entity<string>>(key1);
        var entity = entityMock.Object;

        entityMock.Setup(x => x.ToString()).CallBase();

        // Assert
        entity.ToString().Should().Be($"[{entity.GetType().Name} | {key1}]");


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailSameEntity()
    {
        // Arrange
        const string key1 = "1";

        var entityMock = new Mock<Entity<string>>(key1);
        var entity = entityMock.Object;

        entityMock.Setup(x => x.Equals(entity)).CallBase();

        // Assert
        entity.Equals(entity).Should().BeTrue();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntity()
    {
        // Arrange
        const string key1 = "1";

        var entityMock = new Mock<Entity<string>>(key1);
        var entity1 = entityMock.Object;
        var entity2 = Mock.Of<Entity<string>>(x => x.Id == key1);

        entityMock.Setup(x => x.Equals(entity2)).CallBase();

        // Assert
        entity1.Equals(entity2).Should().BeTrue();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnFalse_DetailDifferentKeys()
    {
        // Arrange
        const string key1 = "1";
        const string key2 = "2";

        var entityMock1 = new Mock<Entity<string>>(key1);
        var entity1 = entityMock1.Object;
        var entityMock2 = new Mock<Entity<string>>(key2);
        var entity2 = entityMock2.Object;

        // Assert
        entity1.Equals(entity2).Should().BeFalse();


    }

    [Fact]  
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntityIsNull()
    {
        // Arrange
        const string key1 = "1";

        var entityMock1 = new Mock<Entity<string>>(key1);
        var entity1 = entityMock1.Object;

        entityMock1.Setup(x => x.Equals(entity1)).CallBase();

        Entity<string>? entity2 = null;

        // Assert
        entity1.Equals(entity2).Should().BeFalse();


    }

}
