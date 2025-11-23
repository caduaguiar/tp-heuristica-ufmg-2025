using BppGrasp.Core.Models;
using FluentAssertions;
using Xunit;

namespace BppGrasp.Tests.Models;

public class ItemTests
{
    [Fact]
    public void Should_CreateItem_When_ValidParametersProvided()
    {
        var item = new Item(1, 10.5);

        item.Id.Should().Be(1);
        item.Weight.Should().Be(10.5);
    }

    [Fact]
    public void Should_ThrowException_When_NegativeIdProvided()
    {
        Action act = () => new Item(-1, 10.5);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Item ID must be non-negative*");
    }

    [Fact]
    public void Should_ThrowException_When_NonPositiveWeightProvided()
    {
        Action act = () => new Item(1, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Item weight must be positive*");
    }

    [Fact]
    public void Should_ReturnTrue_When_ComparingItemsWithSameId()
    {
        var item1 = new Item(1, 10.5);
        var item2 = new Item(1, 20.0);

        item1.Equals(item2).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_ComparingItemsWithDifferentId()
    {
        var item1 = new Item(1, 10.5);
        var item2 = new Item(2, 10.5);

        item1.Equals(item2).Should().BeFalse();
    }
}
