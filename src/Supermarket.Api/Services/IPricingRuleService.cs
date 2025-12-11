using Supermarket.Core;

namespace Supermarket.Api.Services;

public interface IPricingRuleService
{
    IEnumerable<PricingRule> GetAllRules();
    PricingRule? GetRule(string itemCode);
    PricingRule CreateRule(string itemCode, int unitPrice, SpecialOffer? specialOffer);
    PricingRule? UpdateRule(string itemCode, int unitPrice, SpecialOffer? specialOffer);
    bool DeleteRule(string itemCode);
    void ResetToDefaults();
}
