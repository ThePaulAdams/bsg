namespace Supermarket.Api.Models;

public record CheckoutResponse(string CartId);

public record ScanResponse(int Total, Dictionary<string, int> Items);

public record TotalResponse(int Total, Dictionary<string, int> Items);
