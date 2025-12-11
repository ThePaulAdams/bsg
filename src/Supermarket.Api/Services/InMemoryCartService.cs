using Supermarket.Core;

namespace Supermarket.Api.Services;

public class InMemoryCartService : ICartService
{
    private static readonly Dictionary<string, Checkout> _carts = new();
    private static readonly object _lock = new();

    public string CreateCart()
    {
        var cartId = Guid.NewGuid().ToString();
        lock (_lock)
        {
            _carts[cartId] = new Checkout(GetDefaultPricingRules());
        }
        return cartId;
    }

    public Checkout? GetCart(string cartId)
    {
        lock (_lock)
        {
            return _carts.GetValueOrDefault(cartId);
        }
    }

    public bool DeleteCart(string cartId)
    {
        lock (_lock)
        {
            return _carts.Remove(cartId);
        }
    }

    public IEnumerable<PricingRule> GetDefaultPricingRules()
    {
        return new List<PricingRule>
        {
            new PricingRule("A", 50, new SpecialOffer(3, 130)),
            new PricingRule("B", 30, new SpecialOffer(2, 45)),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };
    }
}
