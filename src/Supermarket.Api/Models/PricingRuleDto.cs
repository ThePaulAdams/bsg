namespace Supermarket.Api.Models;

public record PricingRuleDto(
    string ItemCode,
    int UnitPrice,
    SpecialOfferDto? SpecialOffer
);

public record SpecialOfferDto(int Quantity, int SpecialPrice);

public record CreatePricingRuleRequest(
    string ItemCode,
    int UnitPrice,
    SpecialOfferDto? SpecialOffer
);

public record UpdatePricingRuleRequest(
    int UnitPrice,
    SpecialOfferDto? SpecialOffer
);
