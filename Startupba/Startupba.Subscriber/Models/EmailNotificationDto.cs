using System;

namespace Startupba.Subscriber.Models
{
    public class EmailNotificationDto
    {
        /// <summary>
        /// "StartupApproved", "StartupRejected", "DonationReceived", "TicketAnswered"
        /// </summary>
        public string NotificationType { get; set; } = null!;

        // Recipient information
        public string RecipientEmail { get; set; } = null!;
        public string RecipientFullName { get; set; } = null!;

        // Startup information (approval / rejection / donation emails)
        public int? StartupId { get; set; }
        public string? StartupName { get; set; }
        public string? CategoryName { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public decimal? TargetAmount { get; set; }
        public decimal? AmountRaised { get; set; }
        public decimal? PlatformFeePercent { get; set; }
        public string? RejectionReason { get; set; }

        // Donation information (donation emails)
        public string? DonorFullName { get; set; }
        public decimal? DonationAmount { get; set; }
        public string? DonationMessage { get; set; }

        // Support ticket information (ticket emails)
        public string? TicketSubject { get; set; }
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
