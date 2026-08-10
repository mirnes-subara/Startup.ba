using Microsoft.EntityFrameworkCore;

namespace Startupba.Services.Database
{
    public class StartupbaDbContext : DbContext
    {
        public StartupbaDbContext(DbContextOptions<StartupbaDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<StartupStatus> StartupStatuses { get; set; }
        public DbSet<Startup> Startups { get; set; }
        public DbSet<StartupImage> StartupImages { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<StartupLike> StartupLikes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BlogPostLike> BlogPostLikes { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PlatformSetting> PlatformSettings { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(t => t.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure Role entity
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // Configure UserRole join entity
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            // Configure Gender entity
            modelBuilder.Entity<Gender>()
                .HasIndex(g => g.Name)
                .IsUnique();

            // Configure Country entity
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Configure City entity
            modelBuilder.Entity<City>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(co => co.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Gender)
                .WithMany()
                .HasForeignKey(u => u.GenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.City)
                .WithMany()
                .HasForeignKey(u => u.CityId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Category entity
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Configure StartupStatus entity
            modelBuilder.Entity<StartupStatus>()
                .HasIndex(s => s.Name)
                .IsUnique();

            // Configure Startup entity relationships
            modelBuilder.Entity<Startup>()
                .HasOne(s => s.Founder)
                .WithMany(u => u.Startups)
                .HasForeignKey(s => s.FounderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Startup>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Startups)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Startup>()
                .HasOne(s => s.City)
                .WithMany()
                .HasForeignKey(s => s.CityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Startup>()
                .HasOne(s => s.Status)
                .WithMany(st => st.Startups)
                .HasForeignKey(s => s.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure StartupImage entity relationship
            modelBuilder.Entity<StartupImage>()
                .HasOne(si => si.Startup)
                .WithMany(s => s.StartupImages)
                .HasForeignKey(si => si.StartupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Donation entity relationships
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.Startup)
                .WithMany(s => s.Donations)
                .HasForeignKey(d => d.StartupId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany(u => u.Donations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Payment entity relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Donation)
                .WithMany()
                .HasForeignKey(p => p.DonationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure StartupLike join entity (many-to-many)
            modelBuilder.Entity<StartupLike>()
                .HasOne(sl => sl.Startup)
                .WithMany(s => s.StartupLikes)
                .HasForeignKey(sl => sl.StartupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StartupLike>()
                .HasOne(sl => sl.User)
                .WithMany()
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StartupLike>()
                .HasIndex(sl => new { sl.StartupId, sl.UserId })
                .IsUnique();

            // Configure Favorite join entity (many-to-many)
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Startup)
                .WithMany(s => s.Favorites)
                .HasForeignKey(f => f.StartupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.StartupId, f.UserId })
                .IsUnique();

            // Configure BlogPost entity relationships
            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.Author)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(bp => bp.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.Startup)
                .WithMany(s => s.BlogPosts)
                .HasForeignKey(bp => bp.StartupId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.SharedFromBlogPost)
                .WithMany()
                .HasForeignKey(bp => bp.SharedFromBlogPostId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Comment entity relationships
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.BlogPost)
                .WithMany(bp => bp.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure BlogPostLike join entity (many-to-many)
            modelBuilder.Entity<BlogPostLike>()
                .HasOne(bpl => bpl.BlogPost)
                .WithMany(bp => bp.BlogPostLikes)
                .HasForeignKey(bpl => bpl.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlogPostLike>()
                .HasOne(bpl => bpl.User)
                .WithMany()
                .HasForeignKey(bpl => bpl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BlogPostLike>()
                .HasIndex(bpl => new { bpl.BlogPostId, bpl.UserId })
                .IsUnique();

            // Configure Chat entity relationships
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure SupportTicket entity relationships
            modelBuilder.Entity<SupportTicket>()
                .HasOne(st => st.User)
                .WithMany()
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Report entity relationships
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Startup)
                .WithMany()
                .HasForeignKey(r => r.StartupId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.BlogPost)
                .WithMany()
                .HasForeignKey(r => r.BlogPostId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Announcement entity relationships
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Notification entity relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            // Configure PlatformSetting entity
            modelBuilder.Entity<PlatformSetting>()
                .HasIndex(ps => ps.Key)
                .IsUnique();

            // Seed initial data
            modelBuilder.SeedData();
        }
    }
}
