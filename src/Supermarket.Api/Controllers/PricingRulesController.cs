using Microsoft.AspNetCore.Mvc;
using Supermarket.Api.Models;
using Supermarket.Api.Services;
using Supermarket.Core;

namespace Supermarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricingRulesController : ControllerBase
{
    private readonly IPricingRuleService _pricingRuleService;

    public PricingRulesController(IPricingRuleService pricingRuleService)
    {
        _pricingRuleService = pricingRuleService;
    }

    [HttpGet]
    public IActionResult GetAllRules()
    {
        var rules = _pricingRuleService.GetAllRules()
            .Select(r => new PricingRuleDto(
                r.ItemCode,
                r.UnitPrice,
                r.SpecialOffer != null
                    ? new SpecialOfferDto(r.SpecialOffer.Quantity, r.SpecialOffer.SpecialPrice)
                    : null
            ));
        return Ok(rules);
    }

    [HttpGet("{itemCode}")]
    public IActionResult GetRule(string itemCode)
    {
        var rule = _pricingRuleService.GetRule(itemCode);
        if (rule == null)
            return NotFound(new { Error = $"Rule for item {itemCode} not found" });

        return Ok(new PricingRuleDto(
            rule.ItemCode,
            rule.UnitPrice,
            rule.SpecialOffer != null
                ? new SpecialOfferDto(rule.SpecialOffer.Quantity, rule.SpecialOffer.SpecialPrice)
                : null
        ));
    }

    [HttpPost]
    public IActionResult CreateRule([FromBody] CreatePricingRuleRequest request)
    {
        try
        {
            var specialOffer = request.SpecialOffer != null
                ? new SpecialOffer(request.SpecialOffer.Quantity, request.SpecialOffer.SpecialPrice)
                : null;

            var rule = _pricingRuleService.CreateRule(request.ItemCode, request.UnitPrice, specialOffer);

            return CreatedAtAction(
                nameof(GetRule),
                new { itemCode = rule.ItemCode },
                new PricingRuleDto(
                    rule.ItemCode,
                    rule.UnitPrice,
                    rule.SpecialOffer != null
                        ? new SpecialOfferDto(rule.SpecialOffer.Quantity, rule.SpecialOffer.SpecialPrice)
                        : null
                )
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{itemCode}")]
    public IActionResult UpdateRule(string itemCode, [FromBody] UpdatePricingRuleRequest request)
    {
        try
        {
            var specialOffer = request.SpecialOffer != null
                ? new SpecialOffer(request.SpecialOffer.Quantity, request.SpecialOffer.SpecialPrice)
                : null;

            var rule = _pricingRuleService.UpdateRule(itemCode, request.UnitPrice, specialOffer);
            if (rule == null)
                return NotFound(new { Error = $"Rule for item {itemCode} not found" });

            return Ok(new PricingRuleDto(
                rule.ItemCode,
                rule.UnitPrice,
                rule.SpecialOffer != null
                    ? new SpecialOfferDto(rule.SpecialOffer.Quantity, rule.SpecialOffer.SpecialPrice)
                    : null
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{itemCode}")]
    public IActionResult DeleteRule(string itemCode)
    {
        var deleted = _pricingRuleService.DeleteRule(itemCode);
        if (!deleted)
            return NotFound(new { Error = $"Rule for item {itemCode} not found" });

        return NoContent();
    }

    [HttpPost("reset")]
    public IActionResult ResetToDefaults()
    {
        _pricingRuleService.ResetToDefaults();
        return Ok(new { Message = "Pricing rules reset to defaults" });
    }
}
