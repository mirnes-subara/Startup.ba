using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 0=StartupSubmitted, 1=StartupApproved, 2=StartupRejected, 3=StartupPaused,
        /// 4=DonationReceived, 5=NewComment, 6=TicketAnswered, 7=ReportResolved, 8=Announcement
        /// </summary>
        [Required]
        public int Type { get; set; }

        /// <summary>
        /// The ID of the related entity (StartupId, DonationId, BlogPostId, SupportTicketId, ReportId or AnnouncementId)
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// "Startup", "Donation", "BlogPost", "SupportTicket", "Report" or "Announcement"
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Well-known notification type IDs.
    /// </summary>
    public static class NotificationTypes
    {
        public const int StartupSubmitted = 0;
        public const int StartupApproved = 1;
        public const int StartupRejected = 2;
        public const int StartupPaused = 3;
        public const int DonationReceived = 4;
        public const int NewComment = 5;
        public const int TicketAnswered = 6;
        public const int ReportResolved = 7;
        public const int Announcement = 8;
        public const int VerificationRequested = 9;
    }
}
