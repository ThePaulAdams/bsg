using NUnit.Framework;
using Supermarket.Api.Controllers;
using Supermarket.Api.Models;
using Supermarket.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Supermarket.Tests;

[TestFixture]
public class PricingRulesControllerTests
{
    private IPricingRuleService _pricingRuleService = null!;
    private PricingRulesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _pricingRuleService = new InMemoryPricingRuleService();
        _pricingRuleService.ResetToDefaults();
        _controller = new PricingRulesController(_pricingRuleService);
    }

    [Test]
    public void GetAllRules_ReturnsDefaultRules()
    {
        var result = _controller.GetAllRules() as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var rules = result!.Value as IEnumerable<PricingRuleDto>;
        Assert.That(rules, Is.Not.Null);
        Assert.That(rules!.Count(), Is.EqualTo(4)); // A, B, C, D
    }

    [Test]
    public void GetRule_WithValidCode_ReturnsRule()
    {
        var result = _controller.GetRule("A") as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var rule = result!.Value as PricingRuleDto;
        Assert.That(rule, Is.Not.Null);
        Assert.That(rule!.ItemCode, Is.EqualTo("A"));
        Assert.That(rule.UnitPrice, Is.EqualTo(50));
        Assert.That(rule.SpecialOffer, Is.Not.Null);
        Assert.That(rule.SpecialOffer!.Quantity, Is.EqualTo(3));
        Assert.That(rule.SpecialOffer.SpecialPrice, Is.EqualTo(130));
    }

    [Test]
    public void GetRule_WithInvalidCode_ReturnsNotFound()
    {
        var result = _controller.GetRule("Z") as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void GetRule_CaseInsensitive_Works()
    {
        var result = _controller.GetRule("a") as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var rule = result!.Value as PricingRuleDto;
        Assert.That(rule!.ItemCode, Is.EqualTo("A"));
    }

    [Test]
    public void CreateRule_WithValidData_CreatesRule()
    {
        var request = new CreatePricingRuleRequest(
            "E",
            25,
            new SpecialOfferDto(4, 90)
        );

        var result = _controller.CreateRule(request) as CreatedAtActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(201));

        var rule = result.Value as PricingRuleDto;
        Assert.That(rule, Is.Not.Null);
        Assert.That(rule!.ItemCode, Is.EqualTo("E"));
        Assert.That(rule.UnitPrice, Is.EqualTo(25));
    }

    [Test]
    public void CreateRule_WithNoSpecialOffer_CreatesRule()
    {
        var request = new CreatePricingRuleRequest("F", 10, null);

        var result = _controller.CreateRule(request) as CreatedAtActionResult;

        Assert.That(result, Is.Not.Null);
        var rule = result!.Value as PricingRuleDto;
        Assert.That(rule!.SpecialOffer, Is.Null);
    }

    [Test]
    public void CreateRule_WithDuplicateCode_ReturnsBadRequest()
    {
        var request = new CreatePricingRuleRequest("A", 60, null); // A already exists

        var result = _controller.CreateRule(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void CreateRule_WithInvalidData_ReturnsBadRequest()
    {
        var request = new CreatePricingRuleRequest("", 50, null); // Empty code

        var result = _controller.CreateRule(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void UpdateRule_WithValidData_UpdatesRule()
    {
        var request = new UpdatePricingRuleRequest(
            60, // New price for A
            new SpecialOfferDto(3, 150) // New special offer
        );

        var result = _controller.UpdateRule("A", request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var rule = result!.Value as PricingRuleDto;
        Assert.That(rule, Is.Not.Null);
        Assert.That(rule!.UnitPrice, Is.EqualTo(60));
        Assert.That(rule.SpecialOffer!.SpecialPrice, Is.EqualTo(150));
    }

    [Test]
    public void UpdateRule_RemovingSpecialOffer_Works()
    {
        var request = new UpdatePricingRuleRequest(50, null); // Remove special offer

        var result = _controller.UpdateRule("A", request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var rule = result!.Value as PricingRuleDto;
        Assert.That(rule!.SpecialOffer, Is.Null);
    }

    [Test]
    public void UpdateRule_AddingSpecialOffer_Works()
    {
        var request = new UpdatePricingRuleRequest(
            20,
            new SpecialOfferDto(5, 80)
        );

        var result = _controller.UpdateRule("C", request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        var rule = result!.Value as PricingRuleDto;
        Assert.That(rule!.SpecialOffer, Is.Not.Null);
        Assert.That(rule.SpecialOffer!.Quantity, Is.EqualTo(5));
    }

    [Test]
    public void UpdateRule_WithInvalidCode_ReturnsNotFound()
    {
        var request = new UpdatePricingRuleRequest(50, null);

        var result = _controller.UpdateRule("Z", request) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void UpdateRule_WithInvalidData_ReturnsBadRequest()
    {
        var request = new UpdatePricingRuleRequest(-10, null); // Negative price

        var result = _controller.UpdateRule("A", request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void DeleteRule_RemovesRule()
    {
        var deleteResult = _controller.DeleteRule("D") as NoContentResult;

        Assert.That(deleteResult, Is.Not.Null);
        Assert.That(deleteResult!.StatusCode, Is.EqualTo(204));

        // Verify rule is gone
        var getResult = _controller.GetRule("D") as NotFoundObjectResult;
        Assert.That(getResult, Is.Not.Null);
    }

    [Test]
    public void DeleteRule_WithInvalidCode_ReturnsNotFound()
    {
        var result = _controller.DeleteRule("Z") as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void ResetToDefaults_RestoresDefaultRules()
    {
        // First, modify a rule
        _controller.UpdateRule("A", new UpdatePricingRuleRequest(100, null));

        // Reset
        var resetResult = _controller.ResetToDefaults() as OkObjectResult;
        Assert.That(resetResult, Is.Not.Null);

        // Verify A is back to default
        var getResult = _controller.GetRule("A") as OkObjectResult;
        var rule = getResult!.Value as PricingRuleDto;
        Assert.That(rule!.UnitPrice, Is.EqualTo(50)); // Original price
        Assert.That(rule.SpecialOffer, Is.Not.Null); // Special offer restored
    }

    [Test]
    public void ResetToDefaults_AfterDeletion_RestoresAll()
    {
        // Delete all rules
        _controller.DeleteRule("A");
        _controller.DeleteRule("B");
        _controller.DeleteRule("C");
        _controller.DeleteRule("D");

        // Reset
        _controller.ResetToDefaults();

        // Verify all 4 rules are back
        var result = _controller.GetAllRules() as OkObjectResult;
        var rules = result!.Value as IEnumerable<PricingRuleDto>;
        Assert.That(rules!.Count(), Is.EqualTo(4));
    }

    [Test]
    public void CreateAndRetrieve_NewRule_Works()
    {
        var createRequest = new CreatePricingRuleRequest(
            "X",
            99,
            new SpecialOfferDto(10, 900)
        );

        _controller.CreateRule(createRequest);

        var getResult = _controller.GetRule("X") as OkObjectResult;
        var rule = getResult!.Value as PricingRuleDto;

        Assert.That(rule, Is.Not.Null);
        Assert.That(rule!.ItemCode, Is.EqualTo("X"));
        Assert.That(rule.UnitPrice, Is.EqualTo(99));
        Assert.That(rule.SpecialOffer!.Quantity, Is.EqualTo(10));
    }
}
