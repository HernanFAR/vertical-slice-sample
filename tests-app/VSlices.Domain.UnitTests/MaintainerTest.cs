using FluentAssertions;
using JetBrains.Annotations;
using LanguageExt;

namespace VSlices.Domain.UnitTests;

[TestSubject(typeof(Maintainer<,>))]
public class MaintainerTest
{
    public sealed class Test(string id) : Maintainer<Test, string>(id)
    {
        public static Test Test1 { get; } = new("1");
        public static Test Test2 { get; } = new("2");

    }

    [Fact]
    public void FindOrOption_Success_ShouldReturnTestStaticInstance()
    {
        // Arrange
        const string id = "1";  

        // Act
        Option<Test> result = Test.FindOrOption(id);   

        // Assert
        result.Case.Should().Be(Test.Test1); 

    }

    [Fact]
    public void FindOrOption_Fail_ShouldReturnNone()
    {
        // Arrange
        const string id = "3";

        // Act
        Option<Test> result = Test.FindOrOption(id);    

        // Assert
        result.IsNone.Should().BeTrue();    
    }

    [Fact]
    public void Find_Success_ShouldReturnTestStaticInstance()
    {
        // Arrange
        const string id = "1";

        // Act
        Test result = Test.Find(id);    

        // Assert
        result.Should().Be(Test.Test1); 
    }

    [Fact]
    public void Find_Fail_ShouldThrowException()
    {
        // Arrange
        const string id = "3";
        string message = $"The specified {typeof(string).FullName} does not correlates to " +
                         $"an public static readonly property of {typeof(Test).FullName}";

        // Act
        Action act = () => Test.Find(id);   

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage(message);    
    }
}