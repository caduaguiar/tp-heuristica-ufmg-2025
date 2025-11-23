using BppGrasp.Core.Models;
using FluentAssertions;
using Xunit;

namespace BppGrasp.Tests.Models;

public class PrecedenceConstraintTests
{
    [Fact]
    public void Should_CreateConstraint_When_ValidParametersProvided()
    {
        var constraint = new PrecedenceConstraint(1, 2, 1);

        constraint.ItemA.Should().Be(1);
        constraint.ItemB.Should().Be(2);
        constraint.MinSeparation.Should().Be(1);
    }

    [Fact]
    public void Should_ThrowException_When_SameItemsProvided()
    {
        Action act = () => new PrecedenceConstraint(1, 1, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Items A and B cannot be the same*");
    }

    [Fact]
    public void Should_ReturnTrue_When_ConstraintIsSatisfied()
    {
        var constraint = new PrecedenceConstraint(1, 2, 1);

        constraint.IsSatisfied(3, 2).Should().BeTrue();
        constraint.IsSatisfied(2, 1).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_ConstraintIsNotSatisfied()
    {
        var constraint = new PrecedenceConstraint(1, 2, 1);

        constraint.IsSatisfied(2, 2).Should().BeFalse();
        constraint.IsSatisfied(1, 2).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 0, 0, true)]
    [InlineData(1, 0, 0, true)]
    [InlineData(2, 1, 1, true)]
    [InlineData(1, 1, 1, false)]
    public void Given_DifferentBinAssignments_When_CheckingSatisfaction_Then_ReturnExpectedResult(
        int binA, int binB, int minSeparation, bool expectedResult)
    {
        var constraint = new PrecedenceConstraint(1, 2, minSeparation);

        var result = constraint.IsSatisfied(binA, binB);

        result.Should().Be(expectedResult);
    }
}
