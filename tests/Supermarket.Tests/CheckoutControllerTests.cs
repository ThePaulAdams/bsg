using NUnit.Framework;
using Supermarket.Api.Controllers;
using Supermarket.Api.Models;
using Supermarket.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Supermarket.Tests;

[TestFixture]
public class CheckoutControllerTests
{
    private ICartService _cartService = null!;
    private CheckoutController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _cartService = new InMemoryCartService();
        _controller = new CheckoutController(_cartService);
    }

    [Test]
    public void StartCart_ReturnsCartId()
    {
        var result = _controller.StartCart() as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));

        var response = result.Value as CheckoutResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.CartId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Scan_WithValidCart_ReturnsTotal()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        var scanResult = _controller.Scan(cartId, "A") as OkObjectResult;

        Assert.That(scanResult, Is.Not.Null);
        Assert.That(scanResult!.StatusCode, Is.EqualTo(200));

        var response = scanResult.Value as ScanResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Total, Is.EqualTo(50));
        Assert.That(response.Items["A"], Is.EqualTo(1));
    }

    [Test]
    public void Scan_WithInvalidCart_ReturnsNotFound()
    {
        var result = _controller.Scan("invalid-cart-id", "A") as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void Scan_WithInvalidItem_ReturnsBadRequest()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        var result = _controller.Scan(cartId, "Z") as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void GetTotal_WithValidCart_ReturnsTotal()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        _controller.Scan(cartId, "A");
        _controller.Scan(cartId, "B");

        var result = _controller.GetTotal(cartId) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var response = result!.Value as TotalResponse;
        Assert.That(response!.Total, Is.EqualTo(80)); // A=50, B=30
    }

    [Test]
    public void GetTotal_WithInvalidCart_ReturnsNotFound()
    {
        var result = _controller.GetTotal("invalid-cart-id") as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void ClearCart_ResetsTotal()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        _controller.Scan(cartId, "A");
        _controller.Scan(cartId, "B");

        var clearResult = _controller.ClearCart(cartId) as OkObjectResult;
        Assert.That(clearResult, Is.Not.Null);

        var response = clearResult!.Value as TotalResponse;
        Assert.That(response!.Total, Is.EqualTo(0));
        Assert.That(response.Items.Count, Is.EqualTo(0));
    }

    [Test]
    public void DeleteCart_RemovesCart()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        var deleteResult = _controller.DeleteCart(cartId) as NoContentResult;
        Assert.That(deleteResult, Is.Not.Null);
        Assert.That(deleteResult!.StatusCode, Is.EqualTo(204));

        // Verify cart is gone
        var getResult = _controller.GetTotal(cartId) as NotFoundObjectResult;
        Assert.That(getResult, Is.Not.Null);
    }

    [Test]
    public void DeleteCart_WithInvalidCart_ReturnsNotFound()
    {
        var result = _controller.DeleteCart("invalid-cart-id") as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void MultipleScans_AccumulateCorrectly()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        _controller.Scan(cartId, "A");
        _controller.Scan(cartId, "A");
        _controller.Scan(cartId, "A"); // 3 A's = special offer

        var result = _controller.GetTotal(cartId) as OkObjectResult;
        var response = result!.Value as TotalResponse;

        Assert.That(response!.Total, Is.EqualTo(130)); // Special offer applied
    }

    [Test]
    public void CaseInsensitiveScanning_Works()
    {
        var startResult = _controller.StartCart() as OkObjectResult;
        var startResponse = startResult!.Value as CheckoutResponse;
        var cartId = startResponse!.CartId;

        _controller.Scan(cartId, "a");
        _controller.Scan(cartId, "A");

        var result = _controller.GetTotal(cartId) as OkObjectResult;
        var response = result!.Value as TotalResponse;

        Assert.That(response!.Items["A"], Is.EqualTo(2));
        Assert.That(response.Total, Is.EqualTo(100)); // 2 × 50
    }
}
