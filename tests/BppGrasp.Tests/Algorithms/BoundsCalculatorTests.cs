using BppGrasp.Core.Algorithms;
using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;
using FluentAssertions;
using Xunit;

namespace BppGrasp.Tests.Algorithms;

public class BoundsCalculatorTests
{
    private readonly BoundsCalculator _calculator;

    public BoundsCalculatorTests()
    {
        var logger = new ConsoleLogger(false);
        _calculator = new BoundsCalculator(logger);
    }

    [Fact]
    public void Should_ComputeLowerBound_When_SimpleInstanceProvided()
    {
        var instance = new BppInstance("Test", 100.0);
        instance.AddItem(new Item(0, 50));
        instance.AddItem(new Item(1, 40));
        instance.AddItem(new Item(2, 35));
        instance.AddItem(new Item(3, 30));

        var lowerBound = _calculator.ComputeLowerBound(instance);

        lowerBound.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Should_ReturnAtLeastTwo_When_ItemsRequireMultipleBins()
    {
        var instance = new BppInstance("Test", 100.0);
        instance.AddItem(new Item(0, 60));
        instance.AddItem(new Item(1, 60));

        var lowerBound = _calculator.ComputeLowerBound(instance);

        lowerBound.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void Given_InstanceWithPrecedences_When_ComputingBound_Then_ReturnValidLowerBound()
    {
        var instance = new BppInstance("Test", 100.0);
        instance.AddItem(new Item(0, 30));
        instance.AddItem(new Item(1, 30));
        instance.AddItem(new Item(2, 30));
        instance.AddPrecedence(new PrecedenceConstraint(0, 1, 1));
        instance.AddPrecedence(new PrecedenceConstraint(1, 2, 1));

        var lowerBound = _calculator.ComputeLowerBound(instance);

        lowerBound.Should().BeGreaterOrEqualTo(1);
    }
}
