using FluentAssertions;
using Moq;
using Xunit;

namespace VSlices.Domain.UnitTests;

public class EntityTests_OneGeneric
{
    [Fact]
    public void ToString_ShouldStringWithReturnEntityAndKeyInfo()
    {
        // Arrange
        const int key1 = 1;
        
        var entity = Mock.Of<Entity<int>>(x => x.Id == key1);
        var entityMock = Mock.Get(entity);

        entityMock.Setup(x => x.ToString()).CallBase();
        entityMock.Setup(x => x.GetKeys()).Returns(new object[] { key1 });

        // Assert
        entity.ToString().Should().Be($"[{entity.GetType().Name} | {key1}]");


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailSameEntity()
    {
        // Arrange
        const int key1 = 1;

        var entity = Mock.Of<Entity<int>>(x => x.Id == key1);
        var entityMock = Mock.Get(entity);

        entityMock.Setup(x => x.Equals(entity)).CallBase();

        // Assert
        entity.EntityEquals(entity).Should().BeTrue();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntity()
    {
        // Arrange
        const int key1 = 1;

        var entity1 = Mock.Of<Entity<int>>(x => x.Id == key1);
        var entity2 = Mock.Of<Entity<int>>(x => x.Id == key1);
        
        // Assert
        entity1.EntityEquals(entity2).Should().BeTrue();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnFalse_DetailDifferentKeys()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;

        var entity1 = Mock.Of<Entity<int>>(x => x.Id == key1);
        var entity1Mock = Mock.Get(entity1);
        var entity2 = Mock.Of<Entity<int>>(x => x.Id == key2);
        var entity2Mock = Mock.Get(entity2);

        entity1Mock.Setup(x => x.GetKeys()).Returns(new object[] { key1 });
        entity2Mock.Setup(x => x.GetKeys()).Returns(new object[] { key2 });

        // Assert
        entity1.EntityEquals(entity2).Should().BeFalse();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntityIsNull()
    {
        // Arrange
        const int key1 = 1;

        var entity1 = Mock.Of<Entity<int>>(x => x.Id == key1);
        var entityMock = Mock.Get(entity1);

        entityMock.Setup(x => x.Equals(entity1)).CallBase();

        Entity<int>? entity2 = null;

        // Assert
        entity1.EntityEquals(entity2).Should().BeFalse();


    }

    [Fact]
    public void GetKeys_ShouldReturnKeys()
    {
        // Arrange
        const int key1 = 1;

        var entityMock = new Mock<Entity<int>>(key1);
        var entity1 = entityMock.Object;

        entityMock.Setup(x => x.GetKeys()).CallBase();

        // Assert
        entity1.GetKeys().Should().BeEquivalentTo(new object[] { key1 });


    }

    //[Fact]
    //public void EqualsOperator_ShouldReturnTrue_DetailOtherEntityIsOtherType()
    //{
    //    // Arrange
    //    const int key1 = 1;
    //    const int key2 = 2;

    //    var entity1 = new TwoKeyEntity1(key1, key2);
    //    var entity2 = new TwoKeyEntity2(key1, key2);


    //    // Assert
    //    entity1.EntityEquals(entity2).Should().BeFalse();


    //}
}
