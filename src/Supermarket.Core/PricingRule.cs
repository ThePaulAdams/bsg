namespace Supermarket.Core;

public class PricingRule
{
    public string ItemCode { get; init; }
    public int UnitPrice { get; init; }
    public SpecialOffer? SpecialOffer { get; init; }

    public PricingRule(string itemCode, int unitPrice, SpecialOffer? specialOffer = null)
    {
        if (string.IsNullOrWhiteSpace(itemCode))
            throw new ArgumentException("Item code cannot be empty", nameof(itemCode));
        if (unitPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(unitPrice));

        ItemCode = itemCode.ToUpper();
        UnitPrice = unitPrice;
        SpecialOffer = specialOffer;
    }

    public int CalculatePrice(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        if (quantity == 0)
            return 0;

        if (SpecialOffer != null)
        {
            int specialOfferSets = quantity / SpecialOffer.Quantity;
            int remainder = quantity % SpecialOffer.Quantity;
            return (specialOfferSets * SpecialOffer.SpecialPrice) + (remainder * UnitPrice);
        }

        return quantity * UnitPrice;
    }
}
