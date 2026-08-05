using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Startupba.Services.Database
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public byte[]? Picture { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        public string PasswordHash { get; set; } = string.Empty;
        
        public string PasswordSalt { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether the profile has been verified by the platform.
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// Whether the user has submitted a verification request awaiting admin review.
        /// </summary>
        public bool IsVerificationRequested { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? LastLoginAt { get; set; }
        
        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        // Foreign keys for Gender and City
        public int GenderId { get; set; }
        public int CityId { get; set; }
        
        // Navigation properties
        public Gender Gender { get; set; } = null!;
        public City City { get; set; } = null!;
        
        // Navigation property for the many-to-many relationship with Role
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Startups founded by this user
        public ICollection<Startup> Startups { get; set; } = new List<Startup>();

        // Donations made by this user
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();

        // Blog posts written by this user
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
