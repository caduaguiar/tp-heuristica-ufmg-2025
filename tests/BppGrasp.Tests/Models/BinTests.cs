using BppGrasp.Core.Models;
using FluentAssertions;
using Xunit;

namespace BppGrasp.Tests.Models;

public class BinTests
{
    [Fact]
    public void Should_CreateBin_When_ValidParametersProvided()
    {
        var bin = new Bin(0, 100.0);

        bin.Id.Should().Be(0);
        bin.Capacity.Should().Be(100.0);
        bin.RemainingCapacity.Should().Be(100.0);
        bin.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Should_AddItem_When_CapacityIsAvailable()
    {
        var bin = new Bin(0, 100.0);
        var item = new Item(1, 30.0);

        var result = bin.AddItem(item);

        result.Should().BeTrue();
        bin.Items.Should().Contain(item);
        bin.RemainingCapacity.Should().Be(70.0);
    }

    [Fact]
    public void Should_NotAddItem_When_CapacityIsInsufficient()
    {
        var bin = new Bin(0, 100.0);
        var item1 = new Item(1, 80.0);
        var item2 = new Item(2, 30.0);

        bin.AddItem(item1);
        var result = bin.AddItem(item2);

        result.Should().BeFalse();
        bin.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Should_RemoveItem_When_ItemExistsInBin()
    {
        var bin = new Bin(0, 100.0);
        var item = new Item(1, 30.0);

        bin.AddItem(item);
        bin.RemoveItem(item);

        bin.Items.Should().BeEmpty();
        bin.RemainingCapacity.Should().Be(100.0);
    }

    [Fact]
    public void Should_CloneBin_When_CloneMethodCalled()
    {
        var bin = new Bin(0, 100.0);
        bin.AddItem(new Item(1, 30.0));
        bin.AddItem(new Item(2, 20.0));

        var clonedBin = bin.Clone();

        clonedBin.Id.Should().Be(bin.Id);
        clonedBin.Capacity.Should().Be(bin.Capacity);
        clonedBin.Items.Should().HaveCount(bin.Items.Count);
    }
}
