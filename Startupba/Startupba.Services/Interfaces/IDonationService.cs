using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IDonationService : ICRUDService<DonationResponse, DonationSearchObject, DonationUpsertRequest, DonationUpsertRequest>
    {
        /// <summary>
        /// Marks a pending donation as completed, updates the startup's raised amount,
        /// notifies the founder (in-app + email) and auto-completes the startup
        /// when the target is reached.
        /// </summary>
        Task<DonationResponse?> CompleteAsync(int id);

        /// <summary>
        /// Applies donation completion + AmountRaised in memory. Caller must SaveChanges
        /// inside an EF transaction. Does not send notifications.
        /// </summary>
        Task<DonationBookingResult> ApplyCompletionAsync(int id);

        /// <summary>
        /// Marks a completed donation as refunded and subtracts the amount from the startup.
        /// Startup status is left unchanged (e.g. Completed stays Completed).
        /// </summary>
        Task<DonationResponse?> RefundAsync(int id);

        /// <summary>
        /// Applies refund + AmountRaised rollback in memory. Caller must SaveChanges
        /// inside an EF transaction.
        /// </summary>
        Task ApplyRefundAsync(int id);

        /// <summary>
        /// In-app + email notifications after a successful local commit.
        /// </summary>
        Task NotifyCompletionAsync(int donationId, bool targetReached);
    }

    public class DonationBookingResult
    {
        public bool Applied { get; init; }
        public bool TargetReached { get; init; }
    }
}
