using NUnit.Framework;
using Supermarket.Core;

namespace Supermarket.Tests;

[TestFixture]
public class PricingRuleTests
{
    [Test]
    public void Constructor_WithValidInputs_CreatesInstance()
    {
        var rule = new PricingRule("A", 50);

        Assert.That(rule.ItemCode, Is.EqualTo("A"));
        Assert.That(rule.UnitPrice, Is.EqualTo(50));
        Assert.That(rule.SpecialOffer, Is.Null);
    }

    [Test]
    public void Constructor_WithSpecialOffer_CreatesInstance()
    {
        var offer = new SpecialOffer(3, 130);
        var rule = new PricingRule("A", 50, offer);

        Assert.That(rule.SpecialOffer, Is.Not.Null);
        Assert.That(rule.SpecialOffer!.Quantity, Is.EqualTo(3));
        Assert.That(rule.SpecialOffer.SpecialPrice, Is.EqualTo(130));
    }

    [Test]
    public void Constructor_ConvertsItemCodeToUpperCase()
    {
        var rule = new PricingRule("a", 50);

        Assert.That(rule.ItemCode, Is.EqualTo("A"));
    }

    [Test]
    public void Constructor_WithEmptyItemCode_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PricingRule("", 50));
    }

    [Test]
    public void Constructor_WithNullItemCode_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PricingRule(null!, 50));
    }

    [Test]
    public void Constructor_WithNegativePrice_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PricingRule("A", -10));
    }

    [Test]
    public void CalculatePrice_WithNoSpecialOffer_ReturnsUnitPriceTimesQuantity()
    {
        var rule = new PricingRule("A", 50);

        Assert.That(rule.CalculatePrice(1), Is.EqualTo(50));
        Assert.That(rule.CalculatePrice(2), Is.EqualTo(100));
        Assert.That(rule.CalculatePrice(5), Is.EqualTo(250));
    }

    [Test]
    public void CalculatePrice_WithZeroQuantity_ReturnsZero()
    {
        var rule = new PricingRule("A", 50);

        Assert.That(rule.CalculatePrice(0), Is.EqualTo(0));
    }

    [Test]
    public void CalculatePrice_WithNegativeQuantity_ThrowsArgumentException()
    {
        var rule = new PricingRule("A", 50);

        Assert.Throws<ArgumentException>(() => rule.CalculatePrice(-1));
    }

    [Test]
    public void CalculatePrice_WithSpecialOffer_ExactMultiple_AppliesOffer()
    {
        var offer = new SpecialOffer(3, 130);
        var rule = new PricingRule("A", 50, offer);

        // 3 items = special offer price
        Assert.That(rule.CalculatePrice(3), Is.EqualTo(130));

        // 6 items = 2 special offers
        Assert.That(rule.CalculatePrice(6), Is.EqualTo(260));
    }

    [Test]
    public void CalculatePrice_WithSpecialOffer_WithRemainder_AppliesOfferAndUnitPrice()
    {
        var offer = new SpecialOffer(3, 130);
        var rule = new PricingRule("A", 50, offer);

        // 4 items = 1 offer (130) + 1 unit (50) = 180
        Assert.That(rule.CalculatePrice(4), Is.EqualTo(180));

        // 5 items = 1 offer (130) + 2 units (100) = 230
        Assert.That(rule.CalculatePrice(5), Is.EqualTo(230));

        // 7 items = 2 offers (260) + 1 unit (50) = 310
        Assert.That(rule.CalculatePrice(7), Is.EqualTo(310));
    }

    [Test]
    public void CalculatePrice_WithSpecialOffer_BelowOfferQuantity_UsesUnitPrice()
    {
        var offer = new SpecialOffer(3, 130);
        var rule = new PricingRule("A", 50, offer);

        // 1 item = unit price
        Assert.That(rule.CalculatePrice(1), Is.EqualTo(50));

        // 2 items = 2 × unit price
        Assert.That(rule.CalculatePrice(2), Is.EqualTo(100));
    }

    [Test]
    public void CalculatePrice_WithLargeQuantity_CalculatesCorrectly()
    {
        var offer = new SpecialOffer(3, 130);
        var rule = new PricingRule("A", 50, offer);

        // 100 items = 33 offers (4290) + 1 unit (50) = 4340
        Assert.That(rule.CalculatePrice(100), Is.EqualTo(4340));
    }

    [Test]
    public void CalculatePrice_WithDifferentSpecialOffer_CalculatesCorrectly()
    {
        var offer = new SpecialOffer(2, 45);
        var rule = new PricingRule("B", 30, offer);

        // 2 items = 45
        Assert.That(rule.CalculatePrice(2), Is.EqualTo(45));

        // 3 items = 1 offer (45) + 1 unit (30) = 75
        Assert.That(rule.CalculatePrice(3), Is.EqualTo(75));

        // 4 items = 2 offers = 90
        Assert.That(rule.CalculatePrice(4), Is.EqualTo(90));
    }
}
