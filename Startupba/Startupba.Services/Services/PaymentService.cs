using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    /// <summary>
    /// Stripe PaymentIntent + refund integration. Uses Stripe test/sandbox keys
    /// (sk_test_*) from configuration for real sandbox charges and refunds.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly StartupbaDbContext _context;
        private readonly IDonationService _donationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _stripeSecretKey;

        public PaymentService(
            StartupbaDbContext context,
            IConfiguration configuration,
            IDonationService donationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _donationService = donationService;
            _httpContextAccessor = httpContextAccessor;
            _stripeSecretKey = configuration["STRIPE:SECRET_KEY"]
                ?? configuration["STRIPE__SECRET_KEY"]
                ?? Environment.GetEnvironmentVariable("STRIPE__SECRET_KEY")
                ?? throw new InvalidOperationException("STRIPE__SECRET_KEY configuration is missing. Please set it in .env file or environment variables.");
        }

        public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;

            // 1. Create a pending donation (validated for approved startup / existing user)
            var donation = await _donationService.CreateAsync(new DonationUpsertRequest
            {
                StartupId = request.StartupId,
                UserId = request.UserId,
                Amount = request.Amount,
                Message = request.Message,
            });

            // 2. Create Stripe customer
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Name = request.CustomerName,
                Email = request.CustomerEmail ?? "",
                Metadata = new Dictionary<string, string>
                {
                    { "address", request.BillingAddress ?? "" },
                    { "city", request.BillingCity ?? "" },
                    { "state", request.BillingState ?? "" },
                    { "country", request.BillingCountry ?? "" },
                    { "donation_id", donation.Id.ToString() },
                    { "startup_id", request.StartupId.ToString() },
                }
            });

            // 3. Create payment intent (Payment Sheet only needs clientSecret)
            var amountInCents = (long)(request.Amount * 100);
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = request.Currency.ToLower(),
                Customer = customer.Id,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Description = "Startup.ba Donation Payment",
                Metadata = new Dictionary<string, string>
                {
                    { "customer_name", request.CustomerName },
                    { "donation_id", donation.Id.ToString() },
                    { "startup_id", request.StartupId.ToString() },
                    { "user_id", request.UserId.ToString() },
                }
            });

            // 4. Save payment record linked to the pending donation
            var payment = new Database.Payment
            {
                DonationId = donation.Id,
                StripePaymentIntentId = paymentIntent.Id,
                StripeCustomerId = customer.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = "pending",
                PaymentMethod = "card",
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                BillingAddress = request.BillingAddress,
                BillingCity = request.BillingCity,
                BillingState = request.BillingState,
                BillingCountry = request.BillingCountry,
                BillingZipCode = request.BillingZipCode,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentIntentResponse
            {
                PaymentId = payment.Id,
                DonationId = donation.Id,
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret ?? string.Empty,
                EphemeralKey = string.Empty,
                CustomerId = customer.Id,
            };
        }

        public async Task<PaymentResponse> ConfirmPaymentAsync(int paymentId, ConfirmPaymentRequest request)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                throw new NotFoundException($"Payment with ID {paymentId} not found.");
            }

            if (!payment.DonationId.HasValue)
            {
                throw new UserException("Payment is not linked to a donation.");
            }

            var donation = await _context.Donations.FindAsync(payment.DonationId.Value);
            if (donation == null)
            {
                throw new NotFoundException("Linked donation was not found.");
            }

            if (!_httpContextAccessor.IsAdministrator())
            {
                _httpContextAccessor.EnsureOwnerOrAdmin(
                    donation.UserId,
                    "You are not authorized to confirm this payment.");
            }

            if (string.Equals(payment.Status, "succeeded", StringComparison.OrdinalIgnoreCase)
                && string.Equals(donation.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return await LoadPaymentResponseAsync(paymentId);
            }

            // Stripe stays outside the SQL transaction.
            StripeConfiguration.ApiKey = _stripeSecretKey;
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(payment.StripePaymentIntentId);
            if (paymentIntent.Status != "succeeded")
            {
                throw new UserException(
                    $"Stripe payment is not succeeded (status: {paymentIntent.Status}).");
            }

            DonationBookingResult booking;
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                payment.Status = "succeeded";
                payment.UpdatedAt = DateTime.UtcNow;

                booking = await _donationService.ApplyCompletionAsync(donation.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            if (booking.Applied)
                await _donationService.NotifyCompletionAsync(donation.Id, booking.TargetReached);

            return await LoadPaymentResponseAsync(paymentId);
        }

        public async Task<PaymentResponse> RefundPaymentAsync(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Donation)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new NotFoundException($"Payment with ID {paymentId} not found.");
            }

            if (string.Equals(payment.Status, "refunded", StringComparison.OrdinalIgnoreCase))
            {
                return await LoadPaymentResponseAsync(paymentId);
            }

            if (!string.Equals(payment.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("Only succeeded payments can be refunded.");
            }

            if (payment.StripePaymentIntentId.StartsWith("pi_seed_", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException(
                    "Seeded demo payments cannot be refunded through Stripe. Use a real sandbox donation from the mobile app.");
            }

            if (payment.DonationId == null)
            {
                throw new UserException("Payment is not linked to a donation.");
            }

            var donation = payment.Donation
                ?? await _context.Donations.FindAsync(payment.DonationId.Value);
            if (donation == null)
            {
                throw new NotFoundException("Linked donation was not found.");
            }

            if (!string.Equals(donation.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("Only payments with a completed donation can be refunded.");
            }

            // Stripe refund stays outside the SQL transaction.
            StripeConfiguration.ApiKey = _stripeSecretKey;
            Stripe.Refund refund;
            try
            {
                var refundService = new RefundService();
                refund = await refundService.CreateAsync(new RefundCreateOptions
                {
                    PaymentIntent = payment.StripePaymentIntentId,
                });
            }
            catch (StripeException ex)
            {
                throw new UserException($"Stripe refund failed: {ex.Message}");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                payment.StripeRefundId = refund.Id;
                payment.Status = "refunded";
                payment.UpdatedAt = DateTime.UtcNow;

                await _donationService.ApplyRefundAsync(donation.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await LoadPaymentResponseAsync(paymentId);
        }

        private async Task<PaymentResponse> LoadPaymentResponseAsync(int paymentId)
        {
            var reloaded = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Startup)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.User)
                .FirstAsync(p => p.Id == paymentId);

            return MapToResponse(reloaded);
        }

        public async Task<PaymentResponse?> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Startup)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return null;

            var ownerId = payment.Donation?.UserId;
            var currentUserId = _httpContextAccessor.GetUserId();
            if (!_httpContextAccessor.IsAdministrator()
                && (ownerId == null || ownerId != currentUserId))
            {
                return null;
            }

            return MapToResponse(payment);
        }

        public async Task<PagedResult<PaymentResponse>> GetAsync(PaymentSearchObject search)
        {
            search ??= new PaymentSearchObject();

            // Non-admins can only list their own payments; ignore client UserId override.
            if (!_httpContextAccessor.IsAdministrator())
            {
                search.UserId = _httpContextAccessor.RequireUserId();
            }

            var query = _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Startup)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.User)
                .AsQueryable();

            // Filter by UserId (through the Donation -> User relationship)
            if (search.UserId.HasValue)
            {
                query = query.Where(p => p.Donation != null && p.Donation.UserId == search.UserId.Value);
            }

            if (search.DonationId.HasValue)
            {
                query = query.Where(p => p.DonationId == search.DonationId.Value);
            }

            if (search.StartupId.HasValue)
            {
                query = query.Where(p => p.Donation != null && p.Donation.StartupId == search.StartupId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query = query.Where(p => p.Status == search.Status);
            }

            if (search.DateFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= search.DateFrom.Value);
            }

            if (search.DateTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= search.DateTo.Value);
            }

            if (search.MinAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= search.MinAmount.Value);
            }

            if (search.MaxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= search.MaxAmount.Value);
            }

            // Full text search on customer name or startup name
            if (!string.IsNullOrWhiteSpace(search.FTS))
            {
                var fts = search.FTS.ToLower();
                query = query.Where(p =>
                    (p.CustomerName != null && p.CustomerName.ToLower().Contains(fts)) ||
                    (p.Donation != null && p.Donation.Startup.Name.ToLower().Contains(fts)));
            }

            // Default order by newest first
            query = query.OrderByDescending(p => p.CreatedAt);

            var result = new PagedResult<PaymentResponse>();

            if (search.IncludeTotalCount)
            {
                result.TotalCount = await query.CountAsync();
            }

            query = PagingHelper.ApplyPaging(query, search);

            var payments = await query.ToListAsync();
            result.Items = payments.Select(MapToResponse).ToList();

            return result;
        }

        private static PaymentResponse MapToResponse(Database.Payment entity)
        {
            return new PaymentResponse
            {
                Id = entity.Id,
                DonationId = entity.DonationId,
                StripePaymentIntentId = entity.StripePaymentIntentId,
                StripeCustomerId = entity.StripeCustomerId,
                StripeRefundId = entity.StripeRefundId,
                Amount = entity.Amount,
                Currency = entity.Currency,
                Status = entity.Status,
                PaymentMethod = entity.PaymentMethod,
                CustomerName = entity.CustomerName,
                CustomerEmail = entity.CustomerEmail,
                BillingAddress = entity.BillingAddress,
                BillingCity = entity.BillingCity,
                BillingState = entity.BillingState,
                BillingCountry = entity.BillingCountry,
                BillingZipCode = entity.BillingZipCode,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                // Enriched fields
                StartupName = entity.Donation?.Startup?.Name ?? "N/A",
                UserId = entity.Donation?.UserId,
                UserName = entity.Donation?.User != null
                    ? $"{entity.Donation.User.FirstName} {entity.Donation.User.LastName}"
                    : entity.CustomerName ?? "N/A",
            };
        }
    }
}
