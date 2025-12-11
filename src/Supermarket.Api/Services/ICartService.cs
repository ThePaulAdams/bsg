using Supermarket.Core;

namespace Supermarket.Api.Services;

public interface ICartService
{
    string CreateCart();
    Checkout? GetCart(string cartId);
    bool DeleteCart(string cartId);
    IEnumerable<PricingRule> GetDefaultPricingRules();
}
