using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StartupbaDbContext _context;
        private readonly IDonationService _donationService;
        private readonly string _stripeSecretKey;

        public PaymentService(StartupbaDbContext context, IConfiguration configuration, IDonationService donationService)
        {
            _context = context;
            _donationService = donationService;
            _stripeSecretKey = configuration["STRIPE:SECRET_KEY"]
                ?? configuration["STRIPE__SECRET_KEY"]
                ?? Environment.GetEnvironmentVariable("STRIPE__SECRET_KEY")
                ?? throw new InvalidOperationException("STRIPE__SECRET_KEY configuration is missing. Please set it in .env file or environment variables.");
        }

        public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;

            // 1. Create Stripe customer
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
                }
            });

            // 2. Create ephemeral key for the customer
            var ephemeralKeyService = new EphemeralKeyService();
            var ephemeralKey = await ephemeralKeyService.CreateAsync(new EphemeralKeyCreateOptions
            {
                Customer = customer.Id,
            });

            // 3. Create payment intent
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
                    { "billing_address", request.BillingAddress ?? "" },
                    { "billing_city", request.BillingCity ?? "" },
                    { "billing_state", request.BillingState ?? "" },
                    { "billing_country", request.BillingCountry ?? "" },
                }
            });

            // 4. Save payment record to database
            var payment = new Database.Payment
            {
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
                ClientSecret = paymentIntent.ClientSecret,
                EphemeralKey = ephemeralKey.Secret,
                CustomerId = customer.Id,
            };
        }

        public async Task<PaymentResponse> ConfirmPaymentAsync(int paymentId, ConfirmPaymentRequest request)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {paymentId} not found.");
            }

            // Verify the donation exists
            var donation = await _context.Donations.FindAsync(request.DonationId);
            if (donation == null)
            {
                throw new InvalidOperationException($"Donation with ID {request.DonationId} not found.");
            }

            payment.DonationId = request.DonationId;
            payment.Status = "succeeded";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Complete the donation: updates the startup's raised amount,
            // notifies the founder and auto-completes the startup if the target is reached
            if (donation.Status != "Completed")
            {
                await _donationService.CompleteAsync(donation.Id);
            }

            // Reload with relationships for the response
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

            return MapToResponse(payment);
        }

        public async Task<PagedResult<PaymentResponse>> GetAsync(PaymentSearchObject search)
        {
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

            if (!search.RetrieveAll && search.Page.HasValue && search.PageSize.HasValue)
            {
                query = query.Skip(search.Page.Value * search.PageSize.Value)
                             .Take(search.PageSize.Value);
            }

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
