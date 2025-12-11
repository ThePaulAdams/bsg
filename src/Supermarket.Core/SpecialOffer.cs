namespace Supermarket.Core;

public class SpecialOffer
{
    public int Quantity { get; init; }
    public int SpecialPrice { get; init; }

    public SpecialOffer(int quantity, int specialPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (specialPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(specialPrice));

        Quantity = quantity;
        SpecialPrice = specialPrice;
    }
}
