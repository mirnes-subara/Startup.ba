using System;

namespace Startupba.Model.Responses
{
    public class DonationResponse
    {
        public int Id { get; set; }

        public int StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string? Message { get; set; }

        /// <summary>
        /// "Pending", "Completed", "Failed"
        /// </summary>
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
