using NUnit.Framework;
using Supermarket.Core;

namespace Supermarket.Tests;

[TestFixture]
public class SpecialOfferTests
{
    [Test]
    public void Constructor_WithValidInputs_CreatesInstance()
    {
        var offer = new SpecialOffer(3, 130);

        Assert.That(offer.Quantity, Is.EqualTo(3));
        Assert.That(offer.SpecialPrice, Is.EqualTo(130));
    }

    [Test]
    public void Constructor_WithZeroQuantity_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SpecialOffer(0, 100));
    }

    [Test]
    public void Constructor_WithNegativeQuantity_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SpecialOffer(-1, 100));
    }

    [Test]
    public void Constructor_WithNegativePrice_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SpecialOffer(3, -10));
    }

    [Test]
    public void Constructor_WithZeroPrice_CreatesInstance()
    {
        var offer = new SpecialOffer(3, 0);

        Assert.That(offer.SpecialPrice, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithQuantityOne_CreatesInstance()
    {
        var offer = new SpecialOffer(1, 50);

        Assert.That(offer.Quantity, Is.EqualTo(1));
    }

    [Test]
    public void Constructor_WithLargeNumbers_CreatesInstance()
    {
        var offer = new SpecialOffer(100, 10000);

        Assert.That(offer.Quantity, Is.EqualTo(100));
        Assert.That(offer.SpecialPrice, Is.EqualTo(10000));
    }
}
