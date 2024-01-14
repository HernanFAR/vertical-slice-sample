using FluentAssertions;
using Xunit;

namespace VSlices.Domain.UnitTests;

public class EntityTests
{
    public class TwoKeyEntity1 : Entity
    {
        public int Key1 { get; }
        public int Key2 { get; }

        public TwoKeyEntity1(int key1, int key2)
        {
            Key1 = key1;
            Key2 = key2;
        }

        public override object[] GetKeys() => new object[] { Key1, Key2 };

    }

    public class TwoKeyEntity2 : Entity
    {
        public int Key1 { get; }
        public int Key2 { get; }

        public TwoKeyEntity2(int key1, int key2)
        {
            Key1 = key1;
            Key2 = key2;
        }

        public override object[] GetKeys() => new object[] { Key1, Key2 };

    }

    [Fact]
    public void ToString_ShouldStringWithReturnEntityAndKeyInfo()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;

        IEntity entity = new TwoKeyEntity1(key1, key2);


        // Assert
        entity.ToString().Should().Be($"[{nameof(TwoKeyEntity1)} | {key1}, {key2}]");


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailSameEntity()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;

        IEntity entity = new TwoKeyEntity1(key1, key2);


        // Assert
        entity.EntityEquals(entity).Should().BeTrue();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntity()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;

        IEntity entity1 = new TwoKeyEntity1(key1, key2);
        IEntity entity2 = new TwoKeyEntity1(key1, key2);


        // Assert
        entity1.EntityEquals(entity2).Should().BeTrue();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnFalse_DetailDifferentKeys()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;
        const int key3 = 3;

        IEntity entity1 = new TwoKeyEntity1(key1, key2);
        IEntity entity2 = new TwoKeyEntity1(key1, key3);


        // Assert
        entity1.EntityEquals(entity2).Should().BeFalse();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntityIsNull()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;

        IEntity entity1 = new TwoKeyEntity1(key1, key2);
        IEntity? entity2 = null;


        // Assert
        entity1.EntityEquals(entity2).Should().BeFalse();


    }

    [Fact]
    public void EqualsOperator_ShouldReturnTrue_DetailOtherEntityIsOtherType()
    {
        // Arrange
        const int key1 = 1;
        const int key2 = 2;

        IEntity entity1 = new TwoKeyEntity1(key1, key2);
        IEntity entity2 = new TwoKeyEntity2(key1, key2);


        // Assert
        entity1.EntityEquals(entity2).Should().BeFalse();


    }
}
