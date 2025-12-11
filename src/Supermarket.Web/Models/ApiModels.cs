namespace Supermarket.Web.Models;

public record CheckoutResponse(string CartId);

public record ScanResponse(int Total, Dictionary<string, int> Items);

public record TotalResponse(int Total, Dictionary<string, int> Items);

public record PricingRuleDto(string ItemCode, int UnitPrice, SpecialOfferDto? SpecialOffer);

public record SpecialOfferDto(int Quantity, int SpecialPrice);

public record CreatePricingRuleRequest(string ItemCode, int UnitPrice, SpecialOfferDto? SpecialOffer);

public record UpdatePricingRuleRequest(int UnitPrice, SpecialOfferDto? SpecialOffer);
