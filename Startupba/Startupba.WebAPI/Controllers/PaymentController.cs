using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    /// <summary>
    /// Handles Stripe payment operations for startup donations.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets a list of payments with optional filtering.
        /// Non-admins are scoped to their own payments (service enforces UserId).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<PaymentResponse>>> Get([FromQuery] PaymentSearchObject search)
        {
            search ??= new PaymentSearchObject();
            if (!this.IsAdministrator())
                search.UserId = this.RequireUserId();

            var result = await _service.GetAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// Creates a Stripe PaymentIntent server-side and returns the client secret for the mobile app.
        /// </summary>
        [HttpPost("create-payment-intent")]
        public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            request.UserId = this.RequireUserId();
            var result = await _service.CreatePaymentIntentAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Confirms a payment after successful Stripe payment, links it to a donation
        /// and completes the donation (updates the startup's raised amount).
        /// </summary>
        [HttpPut("{id}/confirm")]
        public async Task<ActionResult<PaymentResponse>> ConfirmPayment(int id, [FromBody] ConfirmPaymentRequest? request)
        {
            var result = await _service.ConfirmPaymentAsync(id, request ?? new ConfirmPaymentRequest());
            return Ok(result);
        }

        /// <summary>
        /// Gets a payment record by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Full refund via Stripe Refund API. Admin only. Updates payment + donation and rolls back AmountRaised.
        /// </summary>
        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PaymentResponse>> Refund(int id)
        {
            var result = await _service.RefundPaymentAsync(id);
            return Ok(result);
        }
    }
}
