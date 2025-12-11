using Microsoft.AspNetCore.Mvc;
using Supermarket.Api.Models;
using Supermarket.Api.Services;

namespace Supermarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ICartService _cartService;

    public CheckoutController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("start")]
    public IActionResult StartCart()
    {
        var cartId = _cartService.CreateCart();
        return Ok(new CheckoutResponse(cartId));
    }

    [HttpPost("{cartId}/scan/{item}")]
    public IActionResult Scan(string cartId, string item)
    {
        var cart = _cartService.GetCart(cartId);
        if (cart == null)
            return NotFound(new { Error = "Cart not found" });

        try
        {
            cart.Scan(item);
            var total = cart.GetTotalPrice();
            return Ok(new ScanResponse(total, new Dictionary<string, int>(cart.ScannedItems)));
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{cartId}/total")]
    public IActionResult GetTotal(string cartId)
    {
        var cart = _cartService.GetCart(cartId);
        if (cart == null)
            return NotFound(new { Error = "Cart not found" });

        var total = cart.GetTotalPrice();
        return Ok(new TotalResponse(total, new Dictionary<string, int>(cart.ScannedItems)));
    }

    [HttpDelete("{cartId}")]
    public IActionResult DeleteCart(string cartId)
    {
        var deleted = _cartService.DeleteCart(cartId);
        if (!deleted)
            return NotFound(new { Error = "Cart not found" });

        return NoContent();
    }

    [HttpPost("{cartId}/clear")]
    public IActionResult ClearCart(string cartId)
    {
        var cart = _cartService.GetCart(cartId);
        if (cart == null)
            return NotFound(new { Error = "Cart not found" });

        cart.Clear();
        return Ok(new TotalResponse(0, new Dictionary<string, int>()));
    }
}
