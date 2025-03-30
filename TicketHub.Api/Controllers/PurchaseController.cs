using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using TicketHub.Api.Models;
using TicketHub.Api.Services;
using TicketHub.Api.Validators;

namespace TicketHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly QueueService _queueService;
        private readonly IValidator<PurchaseRequest> _validator;

        public PurchaseController(QueueService queueService, IValidator<PurchaseRequest> validator)
        {
            _queueService = queueService;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseRequest purchaseRequest)
        {
            var validationResult = await _validator.ValidateAsync(purchaseRequest);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _queueService.SendMessageAsync(purchaseRequest);
            return Ok();
        }
    }
}