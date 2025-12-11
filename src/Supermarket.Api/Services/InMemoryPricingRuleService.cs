using Supermarket.Core;

namespace Supermarket.Api.Services;

public class InMemoryPricingRuleService : IPricingRuleService
{
    private static Dictionary<string, PricingRule> _rules = new();
    private static readonly object _lock = new();

    public InMemoryPricingRuleService()
    {
        if (_rules.Count == 0)
        {
            ResetToDefaults();
        }
    }

    public IEnumerable<PricingRule> GetAllRules()
    {
        lock (_lock)
        {
            return _rules.Values.ToList();
        }
    }

    public PricingRule? GetRule(string itemCode)
    {
        lock (_lock)
        {
            return _rules.GetValueOrDefault(itemCode.ToUpper());
        }
    }

    public PricingRule CreateRule(string itemCode, int unitPrice, SpecialOffer? specialOffer)
    {
        var rule = new PricingRule(itemCode, unitPrice, specialOffer);
        lock (_lock)
        {
            if (_rules.ContainsKey(rule.ItemCode))
                throw new InvalidOperationException($"Rule for {rule.ItemCode} already exists");
            _rules[rule.ItemCode] = rule;
        }
        return rule;
    }

    public PricingRule? UpdateRule(string itemCode, int unitPrice, SpecialOffer? specialOffer)
    {
        var rule = new PricingRule(itemCode, unitPrice, specialOffer);
        lock (_lock)
        {
            if (!_rules.ContainsKey(rule.ItemCode))
                return null;
            _rules[rule.ItemCode] = rule;
        }
        return rule;
    }

    public bool DeleteRule(string itemCode)
    {
        lock (_lock)
        {
            return _rules.Remove(itemCode.ToUpper());
        }
    }

    public void ResetToDefaults()
    {
        lock (_lock)
        {
            _rules = new Dictionary<string, PricingRule>
            {
                ["A"] = new PricingRule("A", 50, new SpecialOffer(3, 130)),
                ["B"] = new PricingRule("B", 30, new SpecialOffer(2, 45)),
                ["C"] = new PricingRule("C", 20),
                ["D"] = new PricingRule("D", 15)
            };
        }
    }
}
