using Microsoft.AspNetCore.Mvc;
using Socios.Dtos;
using Socios.Services;

namespace Socios.Controllers;

[ApiController]
[Route("subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(SubscriptionService subscriptionService, ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionResponse>> Create([FromBody] CreateSubscriptionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var response = await _subscriptionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid configuration when creating subscription");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubscriptionResponse>> GetById(Guid id)
    {
        var subscription = await _subscriptionService.GetAsync(id);
        if (subscription is null)
        {
            return NotFound();
        }

        return Ok(subscription);
    }
}
