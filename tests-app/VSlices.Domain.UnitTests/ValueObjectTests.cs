using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace VSlices.Domain.UnitTests;

public class ValueObjectTests
{
    public class FirstValueObject : ValueObject
    {
        public int? Number { get; }

        public FirstValueObject(int? number)
        {
            Number = number;
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Number;
        }
    }

    public class SecondValueObject : ValueObject
    {
        public int? Number { get; }

        public SecondValueObject(int? number)
        {
            Number = number;
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return Number;
        }
    }

    [Fact]
    public void ValueEquals_ShouldReturnFalse_DetailDifferentType()
    {
        // Arrange
        const int number = 1;

        var firstValueObject = new FirstValueObject(number);
        var secondValueObject = new SecondValueObject(number);

        // Assert
        firstValueObject.ValueEquals(secondValueObject).Should().BeFalse();
    }
    
    [Fact]
    public void ValueEquals_ShouldReturnFalse_DetailDifferentValues()
    {
        // Arrange
        const int number1 = 1;
        const int number2 = 2;

        var firstValueObject = new FirstValueObject(number1);
        var secondValueObject = new FirstValueObject(number2);

        // Assert
        firstValueObject.ValueEquals(secondValueObject).Should().BeFalse();
    }
    
    [Fact]
    public void ValueEquals_ShouldReturnTrue_DetailSameValues()
    {
        // Arrange
        const int number = 1;

        var firstValueObject = new FirstValueObject(number);
        var secondValueObject = new FirstValueObject(number);

        // Assert
        firstValueObject.ValueEquals(secondValueObject).Should().BeTrue();
    }
    
    [Fact]
    public void ValueEquals_ShouldReturnTrue_DetailDifferentValues()
    {
        // Arrange
        const int number = 1;

        var firstValueObject = new FirstValueObject(number);
        var secondValueObject = new FirstValueObject(null);

        // Assert
        firstValueObject.ValueEquals(secondValueObject).Should().BeFalse();
    }

}
