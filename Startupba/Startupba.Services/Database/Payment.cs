using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// A Stripe payment record. Linked to a Donation once the payment is confirmed.
    /// </summary>
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int? DonationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? StripeCustomerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "EUR";

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(255)]
        public string? CustomerName { get; set; }

        [MaxLength(255)]
        public string? CustomerEmail { get; set; }

        [MaxLength(500)]
        public string? BillingAddress { get; set; }

        [MaxLength(100)]
        public string? BillingCity { get; set; }

        [MaxLength(100)]
        public string? BillingState { get; set; }

        [MaxLength(100)]
        public string? BillingCountry { get; set; }

        [MaxLength(20)]
        public string? BillingZipCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("DonationId")]
        public Donation? Donation { get; set; }
    }
}
