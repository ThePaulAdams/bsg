using NUnit.Framework;
using Supermarket.Core;

namespace Supermarket.Tests;

[TestFixture]
public class CheckoutTests
{
    private List<PricingRule> GetStandardRules()
    {
        return new List<PricingRule>
        {
            new PricingRule("A", 50, new SpecialOffer(3, 130)),
            new PricingRule("B", 30, new SpecialOffer(2, 45)),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };
    }

    [Test]
    public void EmptyBasket_ReturnsZero()
    {
        var checkout = new Checkout(GetStandardRules());

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(0));
    }

    [Test]
    public void SingleA_Returns50()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(50));
    }

    [Test]
    public void SingleB_Returns30()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("B");

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(30));
    }

    [Test]
    public void SingleC_Returns20()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("C");

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(20));
    }

    [Test]
    public void SingleD_Returns15()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("D");

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(15));
    }

    [Test]
    public void ABCD_Returns115()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("C");
        checkout.Scan("D");

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(115));
    }

    [Test]
    public void ThreeA_Returns130()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        // Special offer applies: 3 for 130 (not 150)
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(130));
    }

    [Test]
    public void FourA_Returns180()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");

        // 3 for 130 + 1 for 50 = 180
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(180));
    }

    [Test]
    public void TwoB_Returns45()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("B");
        checkout.Scan("B");

        // Special offer: 2 for 45 (not 60)
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(45));
    }

    [Test]
    public void ThreeB_Returns75()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("B");
        checkout.Scan("B");
        checkout.Scan("B");

        // 2 for 45 + 1 for 30 = 75
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(75));
    }

    [Test]
    public void AAB_Returns130()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("B");

        // 2 A's = 100, 1 B = 30, total = 130
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(130));
    }

    [Test]
    public void AAABB_Returns175()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("B");

        // 3 A's = 130, 2 B's = 45, total = 175
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(175));
    }

    [Test]
    public void ABCDABA_Returns215()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("C");
        checkout.Scan("D");
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("A");

        // 3 A's = 130, 2 B's = 45, 1 C = 20, 1 D = 15, total = 210
        // Wait, let me recalculate: A, B, C, D, A, B, A
        // That's 3 A's, 2 B's, 1 C, 1 D
        // 3 A's (special) = 130, 2 B's (special) = 45, 1 C = 20, 1 D = 15
        // Total = 130 + 45 + 20 + 15 = 210
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(210));
    }

    [Test]
    public void ScanningUnknownItem_ThrowsInvalidOperationException()
    {
        var checkout = new Checkout(GetStandardRules());

        Assert.Throws<InvalidOperationException>(() => checkout.Scan("Z"));
    }

    [Test]
    public void ScanningNullItem_ThrowsArgumentException()
    {
        var checkout = new Checkout(GetStandardRules());

        Assert.Throws<ArgumentException>(() => checkout.Scan(null!));
    }

    [Test]
    public void ScanningEmptyString_ThrowsArgumentException()
    {
        var checkout = new Checkout(GetStandardRules());

        Assert.Throws<ArgumentException>(() => checkout.Scan(""));
    }

    [Test]
    public void ScanningWhitespace_ThrowsArgumentException()
    {
        var checkout = new Checkout(GetStandardRules());

        Assert.Throws<ArgumentException>(() => checkout.Scan("   "));
    }

    [Test]
    public void CaseInsensitiveScanning_Works()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("a");
        checkout.Scan("A");
        checkout.Scan("a");

        // 3 A's = 130 (special offer applies)
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(130));
    }

    [Test]
    public void Clear_ResetsBasket()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("C");

        Assert.That(checkout.GetTotalPrice(), Is.GreaterThan(0));

        checkout.Clear();

        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(0));
    }

    [Test]
    public void ScannedItems_ReturnsCorrectCounts()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("B");
        checkout.Scan("A");

        Assert.That(checkout.ScannedItems["A"], Is.EqualTo(3));
        Assert.That(checkout.ScannedItems["B"], Is.EqualTo(1));
        Assert.That(checkout.ScannedItems.Count, Is.EqualTo(2));
    }

    [Test]
    public void ScannedItems_AfterClear_IsEmpty()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("B");

        checkout.Clear();

        Assert.That(checkout.ScannedItems.Count, Is.EqualTo(0));
    }

    [Test]
    public void MultipleSpecialOffers_ApplyCorrectly()
    {
        var checkout = new Checkout(GetStandardRules());
        // Scan 6 A's and 4 B's
        for (int i = 0; i < 6; i++)
            checkout.Scan("A");
        for (int i = 0; i < 4; i++)
            checkout.Scan("B");

        // 6 A's = 2 special offers = 2 × 130 = 260
        // 4 B's = 2 special offers = 2 × 45 = 90
        // Total = 350
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(350));
    }

    [Test]
    public void LargeQuantities_CalculateCorrectly()
    {
        var checkout = new Checkout(GetStandardRules());
        // Scan 20 A's
        for (int i = 0; i < 20; i++)
            checkout.Scan("A");

        // 20 A's = 6 special offers (18 items = 780) + 2 units (100) = 880
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(880));
    }

    [Test]
    public void MixedItems_WithPartialOffers_CalculateCorrectly()
    {
        var checkout = new Checkout(GetStandardRules());
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A");
        checkout.Scan("A"); // 4 A's
        checkout.Scan("B"); // 1 B

        // 4 A's = 1 special (130) + 1 unit (50) = 180
        // 1 B = 30
        // Total = 210
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(210));
    }

    [Test]
    public void PricingRules_Property_ReturnsAllRules()
    {
        var checkout = new Checkout(GetStandardRules());

        Assert.That(checkout.PricingRules.Count, Is.EqualTo(4));
    }

    [Test]
    public void ReuseAfterClear_WorksCorrectly()
    {
        var checkout = new Checkout(GetStandardRules());

        // First use
        checkout.Scan("A");
        checkout.Scan("B");
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(80));

        checkout.Clear();

        // Second use
        checkout.Scan("C");
        checkout.Scan("D");
        Assert.That(checkout.GetTotalPrice(), Is.EqualTo(35));
    }
}
