namespace Supermarket.Core;

public class Checkout
{
    private readonly Dictionary<string, PricingRule> _pricingRules;
    private readonly Dictionary<string, int> _scannedItems;

    public IReadOnlyDictionary<string, int> ScannedItems => _scannedItems;
    public IReadOnlyCollection<PricingRule> PricingRules => _pricingRules.Values;

    public Checkout(IEnumerable<PricingRule> pricingRules)
    {
        _pricingRules = pricingRules.ToDictionary(r => r.ItemCode, r => r);
        _scannedItems = new Dictionary<string, int>();
    }

    public void Scan(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException("Item code cannot be empty", nameof(item));

        string itemCode = item.ToUpper();

        if (!_pricingRules.ContainsKey(itemCode))
            throw new InvalidOperationException($"Unknown item: {itemCode}");

        _scannedItems[itemCode] = _scannedItems.GetValueOrDefault(itemCode, 0) + 1;
    }

    public int GetTotalPrice()
    {
        return _scannedItems.Sum(kvp =>
        {
            var rule = _pricingRules[kvp.Key];
            return rule.CalculatePrice(kvp.Value);
        });
    }

    public void Clear()
    {
        _scannedItems.Clear();
    }
}
