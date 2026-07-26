using Startupba.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using System;

namespace Startupba.Services.Database
{
    /// <summary>
    /// Seeds the database with test data (all content in English).
    ///
    /// Test logins (password "test" for all):
    ///   desktop   - Administrator (desktop app)
    ///   mobile    - Regular user, founder + investor (mobile app)
    ///   founder2, founder3, founder4 - founders
    ///   investor1, investor2, investor3 - investors
    ///
    /// Startup cover images are read from Startupba.WebAPI/Assets as
    /// "startup1.jpg" ... "startup10.jpg". If a file is missing, the seeder
    /// falls back to the existing "1.jpg" ... "9.jpg" so seeding never fails.
    /// Drop your own startup1..10.jpg files into the Assets folder and
    /// regenerate the migration to use them.
    /// </summary>
    public static class DataSeeder
    {
        private const string DefaultPhoneNumber = "+387 61 111 111";

        private static byte[] StartupCover(int index)
        {
            // Preferred file "startup{index}.jpg", fallback to the existing template images
            var fallback = $"{((index - 1) % 9) + 1}.jpg";
            return ImageConversion.ConvertImageToByteArrayWithFallback("Assets", $"startup{index}.jpg", fallback);
        }

        public static void SeedData(this ModelBuilder modelBuilder)
        {
            // Use a fixed date for all "current" timestamps
            var fixedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

            #region Roles

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "Administrator",
                    Description = "Full system access and administrative privileges",
                    CreatedAt = fixedDate,
                    IsActive = true
                },
                new Role
                {
                    Id = 2,
                    Name = "User",
                    Description = "Standard user - can create startups, invest, write posts and chat",
                    CreatedAt = fixedDate,
                    IsActive = true
                }
            );

            #endregion

            #region Genders

            modelBuilder.Entity<Gender>().HasData(
                new Gender { Id = 1, Name = "Male" },
                new Gender { Id = 2, Name = "Female" }
            );

            #endregion

            #region Countries & Cities

            modelBuilder.Entity<Country>().HasData(
                new Country { Id = 1, Name = "Bosnia and Herzegovina", Code = "BA", IsActive = true },
                new Country { Id = 2, Name = "Croatia", Code = "HR", IsActive = true },
                new Country { Id = 3, Name = "Serbia", Code = "RS", IsActive = true },
                new Country { Id = 4, Name = "Germany", Code = "DE", IsActive = true },
                new Country { Id = 5, Name = "Austria", Code = "AT", IsActive = true }
            );

            modelBuilder.Entity<City>().HasData(
                new City { Id = 1, Name = "Sarajevo", CountryId = 1, IsActive = true },
                new City { Id = 2, Name = "Mostar", CountryId = 1, IsActive = true },
                new City { Id = 3, Name = "Banja Luka", CountryId = 1, IsActive = true },
                new City { Id = 4, Name = "Tuzla", CountryId = 1, IsActive = true },
                new City { Id = 5, Name = "Zenica", CountryId = 1, IsActive = true },
                new City { Id = 6, Name = "Zagreb", CountryId = 2, IsActive = true },
                new City { Id = 7, Name = "Split", CountryId = 2, IsActive = true },
                new City { Id = 8, Name = "Rijeka", CountryId = 2, IsActive = true },
                new City { Id = 9, Name = "Belgrade", CountryId = 3, IsActive = true },
                new City { Id = 10, Name = "Novi Sad", CountryId = 3, IsActive = true },
                new City { Id = 11, Name = "Berlin", CountryId = 4, IsActive = true },
                new City { Id = 12, Name = "Munich", CountryId = 4, IsActive = true },
                new City { Id = 13, Name = "Frankfurt", CountryId = 4, IsActive = true },
                new City { Id = 14, Name = "Vienna", CountryId = 5, IsActive = true },
                new City { Id = 15, Name = "Graz", CountryId = 5, IsActive = true }
            );

            #endregion

            #region Categories

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Ecology", Description = "Green technology, recycling and sustainability projects", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 2, Name = "FinTech", Description = "Financial technology, payments and banking innovation", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 3, Name = "HealthTech", Description = "Digital health, medical devices and wellness solutions", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 4, Name = "EdTech", Description = "Education technology and online learning platforms", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 5, Name = "AgroTech", Description = "Agriculture technology and smart farming", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 6, Name = "Gaming", Description = "Video games, esports and interactive entertainment", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 7, Name = "Tourism", Description = "Travel, hospitality and local experiences", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 8, Name = "Food & Beverage", Description = "Food production, delivery and culinary innovation", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 9, Name = "Artificial Intelligence", Description = "AI, machine learning and data-driven products", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 10, Name = "E-commerce", Description = "Online retail, marketplaces and logistics", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 11, Name = "Mobility", Description = "Transportation, ride sharing and urban mobility", CreatedAt = fixedDate, IsActive = true },
                new Category { Id = 12, Name = "Social Impact", Description = "Non-profit initiatives and community-driven projects", CreatedAt = fixedDate, IsActive = true }
            );

            #endregion

            #region Startup statuses

            modelBuilder.Entity<StartupStatus>().HasData(
                new StartupStatus { Id = 1, Name = "Draft", Description = "Startup is being prepared and is not yet submitted", IsActive = true },
                new StartupStatus { Id = 2, Name = "Pending", Description = "Startup is awaiting administrator review", IsActive = true },
                new StartupStatus { Id = 3, Name = "Approved", Description = "Startup is approved and visible to investors", IsActive = true },
                new StartupStatus { Id = 4, Name = "Rejected", Description = "Startup was rejected by the administrator", IsActive = true },
                new StartupStatus { Id = 5, Name = "Paused", Description = "Startup is temporarily paused by the administrator", IsActive = true },
                new StartupStatus { Id = 6, Name = "Completed", Description = "Startup reached its funding target", IsActive = true }
            );

            #endregion

            #region Users

            const string defaultPassword = "test";

            var desktopSalt = PasswordGenerator.GenerateDeterministicSalt("desktop");
            var desktopHash = PasswordGenerator.GenerateHash(defaultPassword, desktopSalt);
            var mobileSalt = PasswordGenerator.GenerateDeterministicSalt("mobile");
            var mobileHash = PasswordGenerator.GenerateHash(defaultPassword, mobileSalt);
            var founder2Salt = PasswordGenerator.GenerateDeterministicSalt("founder2");
            var founder2Hash = PasswordGenerator.GenerateHash(defaultPassword, founder2Salt);
            var founder3Salt = PasswordGenerator.GenerateDeterministicSalt("founder3");
            var founder3Hash = PasswordGenerator.GenerateHash(defaultPassword, founder3Salt);
            var founder4Salt = PasswordGenerator.GenerateDeterministicSalt("founder4");
            var founder4Hash = PasswordGenerator.GenerateHash(defaultPassword, founder4Salt);
            var investor1Salt = PasswordGenerator.GenerateDeterministicSalt("investor1");
            var investor1Hash = PasswordGenerator.GenerateHash(defaultPassword, investor1Salt);
            var investor2Salt = PasswordGenerator.GenerateDeterministicSalt("investor2");
            var investor2Hash = PasswordGenerator.GenerateHash(defaultPassword, investor2Salt);
            var investor3Salt = PasswordGenerator.GenerateDeterministicSalt("investor3");
            var investor3Hash = PasswordGenerator.GenerateHash(defaultPassword, investor3Salt);

            modelBuilder.Entity<User>().HasData(
                // Administrator (desktop app)
                new User
                {
                    Id = 1,
                    FirstName = "James",
                    LastName = "Anderson",
                    Email = "admin@startupba.com",
                    Username = "desktop",
                    PasswordHash = desktopHash,
                    PasswordSalt = desktopSalt,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = new DateTime(2025, 6, 1),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 1,
                    CityId = 1
                },
                // Regular user - founder + investor (mobile app)
                new User
                {
                    Id = 2,
                    FirstName = "Adam",
                    LastName = "Foster",
                    Email = "mobile@startupba.com",
                    Username = "mobile",
                    PasswordHash = mobileHash,
                    PasswordSalt = mobileSalt,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = new DateTime(2025, 6, 15),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 1,
                    CityId = 1,
                    Picture = ImageConversion.ConvertImageToByteArray("Assets", "adil.png")
                },
                new User
                {
                    Id = 3,
                    FirstName = "Emma",
                    LastName = "Clark",
                    Email = "founder2@startupba.com",
                    Username = "founder2",
                    PasswordHash = founder2Hash,
                    PasswordSalt = founder2Salt,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = new DateTime(2025, 7, 1),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 2,
                    CityId = 2,
                    Picture = ImageConversion.ConvertImageToByteArray("Assets", "amel.png")
                },
                new User
                {
                    Id = 4,
                    FirstName = "David",
                    LastName = "Novak",
                    Email = "founder3@startupba.com",
                    Username = "founder3",
                    PasswordHash = founder3Hash,
                    PasswordSalt = founder3Salt,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = new DateTime(2025, 7, 10),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 1,
                    CityId = 6,
                    Picture = ImageConversion.ConvertImageToByteArray("Assets", "denis.png")
                },
                new User
                {
                    Id = 5,
                    FirstName = "Sarah",
                    LastName = "Miller",
                    Email = "investor1@startupba.com",
                    Username = "investor1",
                    PasswordHash = investor1Hash,
                    PasswordSalt = investor1Salt,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = new DateTime(2025, 8, 1),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 2,
                    CityId = 1
                },
                new User
                {
                    Id = 6,
                    FirstName = "Mark",
                    LastName = "Johnson",
                    Email = "investor2@startupba.com",
                    Username = "investor2",
                    PasswordHash = investor2Hash,
                    PasswordSalt = investor2Salt,
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = new DateTime(2025, 8, 20),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 1,
                    CityId = 9
                },
                new User
                {
                    Id = 7,
                    FirstName = "Lena",
                    LastName = "Weber",
                    Email = "investor3@startupba.com",
                    Username = "investor3",
                    PasswordHash = investor3Hash,
                    PasswordSalt = investor3Salt,
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = new DateTime(2025, 9, 5),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 2,
                    CityId = 14
                },
                new User
                {
                    Id = 8,
                    FirstName = "Tom",
                    LastName = "Becker",
                    Email = "founder4@startupba.com",
                    Username = "founder4",
                    PasswordHash = founder4Hash,
                    PasswordSalt = founder4Salt,
                    IsActive = true,
                    IsVerified = false,
                    CreatedAt = new DateTime(2025, 9, 15),
                    PhoneNumber = DefaultPhoneNumber,
                    GenderId = 1,
                    CityId = 11
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1, DateAssigned = fixedDate },
                new UserRole { Id = 2, UserId = 2, RoleId = 2, DateAssigned = fixedDate },
                new UserRole { Id = 3, UserId = 3, RoleId = 2, DateAssigned = fixedDate },
                new UserRole { Id = 4, UserId = 4, RoleId = 2, DateAssigned = fixedDate },
                new UserRole { Id = 5, UserId = 5, RoleId = 2, DateAssigned = fixedDate },
                new UserRole { Id = 6, UserId = 6, RoleId = 2, DateAssigned = fixedDate },
                new UserRole { Id = 7, UserId = 7, RoleId = 2, DateAssigned = fixedDate },
                new UserRole { Id = 8, UserId = 8, RoleId = 2, DateAssigned = fixedDate }
            );

            #endregion

            #region Platform settings

            modelBuilder.Entity<PlatformSetting>().HasData(
                new PlatformSetting
                {
                    Id = 1,
                    Key = PlatformSettingKeys.PlatformFeePercent,
                    Value = "5",
                    Description = "Percentage the platform keeps when a startup reaches its funding target",
                    UpdatedAt = fixedDate
                },
                new PlatformSetting
                {
                    Id = 2,
                    Key = PlatformSettingKeys.TermsOfUse,
                    Value = "Welcome to Startup.ba. By using this platform you agree to the following terms: " +
                            "1) All startup submissions are reviewed by the administrator before publication. " +
                            "2) The platform keeps a fee (see PlatformFeePercent) from the collected amount once a startup reaches its funding target. " +
                            "3) Donations are voluntary contributions and do not represent equity or ownership. " +
                            "4) Content that violates community standards may be removed and repeated violations may lead to account suspension. " +
                            "5) Users are responsible for the accuracy of the information they publish.",
                    Description = "Terms of use displayed to users",
                    UpdatedAt = fixedDate
                },
                new PlatformSetting
                {
                    Id = 3,
                    Key = PlatformSettingKeys.ContactEmail,
                    Value = "support@startupba.com",
                    Description = "Contact email displayed to users",
                    UpdatedAt = fixedDate
                }
            );

            #endregion

            #region Startups

            modelBuilder.Entity<Startup>().HasData(
                new Startup
                {
                    Id = 1,
                    Name = "GreenCycle",
                    Description = "GreenCycle is a smart recycling platform that rewards households for properly sorted waste. " +
                                  "Our smart bins weigh and classify recyclables, and users collect points they can exchange " +
                                  "for discounts at local shops. We are looking for funding to produce the first 500 smart bins " +
                                  "and launch a pilot program in Sarajevo.",
                    FounderId = 2,
                    CategoryId = 1,
                    CityId = 1,
                    TargetAmount = 50000m,
                    AmountRaised = 12500m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Approved,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 9, 1),
                    ApprovedAt = new DateTime(2025, 9, 3)
                },
                new Startup
                {
                    Id = 2,
                    Name = "PayLink",
                    Description = "PayLink makes instant peer-to-peer payments simple across the Balkans. One app, one QR code, " +
                                  "zero hidden fees. We already have a working prototype and partnerships with two regional banks. " +
                                  "The funding will be used for security certification and public launch.",
                    FounderId = 3,
                    CategoryId = 2,
                    CityId = 2,
                    TargetAmount = 80000m,
                    AmountRaised = 24300m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Approved,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 9, 10),
                    ApprovedAt = new DateTime(2025, 9, 12)
                },
                new Startup
                {
                    Id = 3,
                    Name = "MediTrack",
                    Description = "MediTrack is a digital health companion for chronic patients. It tracks therapy schedules, " +
                                  "connects patients with their doctors and sends alerts when measurements go out of range. " +
                                  "The funds will cover clinical validation and integration with hospital systems.",
                    FounderId = 4,
                    CategoryId = 3,
                    CityId = 6,
                    TargetAmount = 120000m,
                    AmountRaised = 45500m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Approved,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 9, 20),
                    ApprovedAt = new DateTime(2025, 9, 22)
                },
                new Startup
                {
                    Id = 4,
                    Name = "LearnHub",
                    Description = "LearnHub is an online learning platform focused on practical IT skills for the local market. " +
                                  "Short, project-based courses in local languages with mentorship from industry professionals. " +
                                  "Funding goes towards producing 20 new courses and a mobile app.",
                    FounderId = 2,
                    CategoryId = 4,
                    CityId = 1,
                    TargetAmount = 30000m,
                    AmountRaised = 9800m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Approved,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 1),
                    ApprovedAt = new DateTime(2025, 10, 2)
                },
                new Startup
                {
                    Id = 5,
                    Name = "FarmSense",
                    Description = "FarmSense builds affordable soil and weather sensors for small family farms. Our dashboard " +
                                  "tells farmers exactly when to irrigate and fertilize, cutting water usage by up to 40%. " +
                                  "We need funding for the second generation of sensors and field testing.",
                    FounderId = 8,
                    CategoryId = 5,
                    CityId = 11,
                    TargetAmount = 60000m,
                    AmountRaised = 15200m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Approved,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 10),
                    ApprovedAt = new DateTime(2025, 10, 12)
                },
                new Startup
                {
                    Id = 6,
                    Name = "QuestForge",
                    Description = "QuestForge is an indie game studio working on a story-driven adventure game inspired by " +
                                  "Balkan mythology. A playable demo is already available. The funding will finance full " +
                                  "production, voice acting and a Steam release.",
                    FounderId = 3,
                    CategoryId = 6,
                    CityId = 2,
                    TargetAmount = 40000m,
                    AmountRaised = 7400m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Approved,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 20),
                    ApprovedAt = new DateTime(2025, 10, 22)
                },
                new Startup
                {
                    Id = 7,
                    Name = "StayLocal",
                    Description = "StayLocal connects travelers with authentic local experiences hosted by families - cooking " +
                                  "classes, guided hikes and traditional crafts workshops. We reached our funding target and " +
                                  "are launching in three cities this spring. Thank you to all our supporters!",
                    FounderId = 4,
                    CategoryId = 7,
                    CityId = 7,
                    TargetAmount = 25000m,
                    AmountRaised = 26000m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Completed,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 8, 15),
                    ApprovedAt = new DateTime(2025, 8, 17),
                    CompletedAt = new DateTime(2025, 12, 20)
                },
                new Startup
                {
                    Id = 8,
                    Name = "SnackWise",
                    Description = "SnackWise delivers healthy snack boxes to offices on a weekly subscription. Locally sourced, " +
                                  "nutritionist-approved and plastic-free packaging. We are raising funds for our first " +
                                  "delivery van and a small packing facility.",
                    FounderId = 8,
                    CategoryId = 8,
                    CityId = 11,
                    TargetAmount = 20000m,
                    AmountRaised = 0m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Pending,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 12, 28)
                },
                new Startup
                {
                    Id = 9,
                    Name = "CryptoBoost",
                    Description = "CryptoBoost promises guaranteed returns through automated cryptocurrency trading strategies " +
                                  "powered by proprietary algorithms.",
                    FounderId = 2,
                    CategoryId = 2,
                    CityId = 1,
                    TargetAmount = 500000m,
                    AmountRaised = 0m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Rejected,
                    RejectionReason = "Unrealistic funding target and insufficient business plan details. Claims of guaranteed returns violate platform standards.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 11, 5)
                },
                new Startup
                {
                    Id = 10,
                    Name = "RideShare BiH",
                    Description = "RideShare BiH is a carpooling platform for daily commuters between Bosnian cities. Drivers " +
                                  "share fuel costs, passengers travel cheaper, and everyone reduces their carbon footprint. " +
                                  "Currently paused while we resolve licensing requirements.",
                    FounderId = 3,
                    CategoryId = 11,
                    CityId = 3,
                    TargetAmount = 70000m,
                    AmountRaised = 5000m,
                    PlatformFeePercent = 5m,
                    StatusId = StartupStatuses.Paused,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 11, 15),
                    ApprovedAt = new DateTime(2025, 11, 17)
                }
            );

            #endregion

            #region Startup images

            modelBuilder.Entity<StartupImage>().HasData(
                new StartupImage { Id = 1, StartupId = 1, ImageData = StartupCover(1), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 2, StartupId = 2, ImageData = StartupCover(2), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 3, StartupId = 3, ImageData = StartupCover(3), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 4, StartupId = 4, ImageData = StartupCover(4), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 5, StartupId = 5, ImageData = StartupCover(5), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 6, StartupId = 6, ImageData = StartupCover(6), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 7, StartupId = 7, ImageData = StartupCover(7), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 8, StartupId = 8, ImageData = StartupCover(8), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 9, StartupId = 9, ImageData = StartupCover(9), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 10, StartupId = 10, ImageData = StartupCover(10), DisplayOrder = 1, IsCover = true, IsActive = true, CreatedAt = fixedDate },
                // A couple of extra gallery images for the most funded startups
                new StartupImage { Id = 11, StartupId = 1, ImageData = ImageConversion.ConvertImageToByteArrayWithFallback("Assets", "2.jpg"), DisplayOrder = 2, IsCover = false, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 12, StartupId = 2, ImageData = ImageConversion.ConvertImageToByteArrayWithFallback("Assets", "5.jpg"), DisplayOrder = 2, IsCover = false, IsActive = true, CreatedAt = fixedDate },
                new StartupImage { Id = 13, StartupId = 3, ImageData = ImageConversion.ConvertImageToByteArrayWithFallback("Assets", "8.jpg"), DisplayOrder = 2, IsCover = false, IsActive = true, CreatedAt = fixedDate }
            );

            #endregion

            #region Donations

            // Sums per startup match Startup.AmountRaised:
            //   1: 5000+4500+3000            = 12500
            //   2: 10000+8000+6300           = 24300
            //   3: 20000+15500+10000         = 45500
            //   4: 5800+4000                 = 9800
            //   5: 7200+8000                 = 15200
            //   6: 3400+4000                 = 7400
            //   7: 10000+9000+7000           = 26000 (completed startup)
            //  10: 5000                      = 5000
            modelBuilder.Entity<Donation>().HasData(
                new Donation { Id = 1, StartupId = 1, UserId = 5, Amount = 5000m, Message = "Great idea, good luck!", Status = "Completed", CreatedAt = new DateTime(2025, 9, 10), CompletedAt = new DateTime(2025, 9, 10) },
                new Donation { Id = 2, StartupId = 1, UserId = 6, Amount = 4500m, Message = "Recycling done right.", Status = "Completed", CreatedAt = new DateTime(2025, 10, 5), CompletedAt = new DateTime(2025, 10, 5) },
                new Donation { Id = 3, StartupId = 1, UserId = 7, Amount = 3000m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 11, 12), CompletedAt = new DateTime(2025, 11, 12) },
                new Donation { Id = 4, StartupId = 2, UserId = 6, Amount = 10000m, Message = "The region needs this!", Status = "Completed", CreatedAt = new DateTime(2025, 9, 25), CompletedAt = new DateTime(2025, 9, 25) },
                new Donation { Id = 5, StartupId = 2, UserId = 7, Amount = 8000m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 10, 15), CompletedAt = new DateTime(2025, 10, 15) },
                new Donation { Id = 6, StartupId = 2, UserId = 5, Amount = 6300m, Message = "Looking forward to the launch.", Status = "Completed", CreatedAt = new DateTime(2025, 12, 1), CompletedAt = new DateTime(2025, 12, 1) },
                new Donation { Id = 7, StartupId = 3, UserId = 5, Amount = 20000m, Message = "Digital health is the future.", Status = "Completed", CreatedAt = new DateTime(2025, 10, 1), CompletedAt = new DateTime(2025, 10, 1) },
                new Donation { Id = 8, StartupId = 3, UserId = 7, Amount = 15500m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 11, 3), CompletedAt = new DateTime(2025, 11, 3) },
                new Donation { Id = 9, StartupId = 3, UserId = 2, Amount = 10000m, Message = "Proud to support this.", Status = "Completed", CreatedAt = new DateTime(2025, 12, 15), CompletedAt = new DateTime(2025, 12, 15) },
                new Donation { Id = 10, StartupId = 4, UserId = 6, Amount = 5800m, Message = "Education matters.", Status = "Completed", CreatedAt = new DateTime(2025, 10, 20), CompletedAt = new DateTime(2025, 10, 20) },
                new Donation { Id = 11, StartupId = 4, UserId = 7, Amount = 4000m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 11, 25), CompletedAt = new DateTime(2025, 11, 25) },
                new Donation { Id = 12, StartupId = 5, UserId = 5, Amount = 7200m, Message = "My parents are farmers - they need this.", Status = "Completed", CreatedAt = new DateTime(2025, 11, 1), CompletedAt = new DateTime(2025, 11, 1) },
                new Donation { Id = 13, StartupId = 5, UserId = 6, Amount = 8000m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 12, 10), CompletedAt = new DateTime(2025, 12, 10) },
                new Donation { Id = 14, StartupId = 6, UserId = 7, Amount = 3400m, Message = "The demo was amazing!", Status = "Completed", CreatedAt = new DateTime(2025, 11, 8), CompletedAt = new DateTime(2025, 11, 8) },
                new Donation { Id = 15, StartupId = 6, UserId = 5, Amount = 4000m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 12, 22), CompletedAt = new DateTime(2025, 12, 22) },
                new Donation { Id = 16, StartupId = 7, UserId = 5, Amount = 10000m, Message = "Tourism with a soul.", Status = "Completed", CreatedAt = new DateTime(2025, 9, 5), CompletedAt = new DateTime(2025, 9, 5) },
                new Donation { Id = 17, StartupId = 7, UserId = 6, Amount = 9000m, Message = null, Status = "Completed", CreatedAt = new DateTime(2025, 10, 25), CompletedAt = new DateTime(2025, 10, 25) },
                new Donation { Id = 18, StartupId = 7, UserId = 7, Amount = 7000m, Message = "Can't wait to book an experience.", Status = "Completed", CreatedAt = new DateTime(2025, 12, 18), CompletedAt = new DateTime(2025, 12, 18) },
                new Donation { Id = 19, StartupId = 10, UserId = 6, Amount = 5000m, Message = "Commuting between cities is painful - good luck.", Status = "Completed", CreatedAt = new DateTime(2025, 11, 20), CompletedAt = new DateTime(2025, 11, 20) },
                // A pending donation (payment not confirmed yet)
                new Donation { Id = 20, StartupId = 1, UserId = 5, Amount = 500m, Message = "Another small boost.", Status = "Pending", CreatedAt = new DateTime(2025, 12, 30) }
            );

            #endregion

            #region Payments

            modelBuilder.Entity<Payment>().HasData(
                new Payment
                {
                    Id = 1,
                    DonationId = 1,
                    StripePaymentIntentId = "pi_seed_0000000001",
                    StripeCustomerId = "cus_seed_0000000001",
                    Amount = 5000m,
                    Currency = "EUR",
                    Status = "succeeded",
                    PaymentMethod = "card",
                    CustomerName = "Sarah Miller",
                    CustomerEmail = "investor1@startupba.com",
                    BillingAddress = "Ferhadija 12",
                    BillingCity = "Sarajevo",
                    BillingCountry = "Bosnia and Herzegovina",
                    BillingZipCode = "71000",
                    CreatedAt = new DateTime(2025, 9, 10),
                    UpdatedAt = new DateTime(2025, 9, 10)
                },
                new Payment
                {
                    Id = 2,
                    DonationId = 4,
                    StripePaymentIntentId = "pi_seed_0000000002",
                    StripeCustomerId = "cus_seed_0000000002",
                    Amount = 10000m,
                    Currency = "EUR",
                    Status = "succeeded",
                    PaymentMethod = "card",
                    CustomerName = "Mark Johnson",
                    CustomerEmail = "investor2@startupba.com",
                    BillingAddress = "Knez Mihailova 5",
                    BillingCity = "Belgrade",
                    BillingCountry = "Serbia",
                    BillingZipCode = "11000",
                    CreatedAt = new DateTime(2025, 9, 25),
                    UpdatedAt = new DateTime(2025, 9, 25)
                },
                new Payment
                {
                    Id = 3,
                    DonationId = 7,
                    StripePaymentIntentId = "pi_seed_0000000003",
                    StripeCustomerId = "cus_seed_0000000003",
                    Amount = 20000m,
                    Currency = "EUR",
                    Status = "succeeded",
                    PaymentMethod = "card",
                    CustomerName = "Sarah Miller",
                    CustomerEmail = "investor1@startupba.com",
                    BillingAddress = "Ferhadija 12",
                    BillingCity = "Sarajevo",
                    BillingCountry = "Bosnia and Herzegovina",
                    BillingZipCode = "71000",
                    CreatedAt = new DateTime(2025, 10, 1),
                    UpdatedAt = new DateTime(2025, 10, 1)
                },
                new Payment
                {
                    Id = 4,
                    DonationId = null, // pending payment not yet linked to a donation
                    StripePaymentIntentId = "pi_seed_0000000004",
                    StripeCustomerId = "cus_seed_0000000004",
                    Amount = 500m,
                    Currency = "EUR",
                    Status = "pending",
                    PaymentMethod = "card",
                    CustomerName = "Sarah Miller",
                    CustomerEmail = "investor1@startupba.com",
                    BillingAddress = "Ferhadija 12",
                    BillingCity = "Sarajevo",
                    BillingCountry = "Bosnia and Herzegovina",
                    BillingZipCode = "71000",
                    CreatedAt = new DateTime(2025, 12, 30)
                }
            );

            #endregion

            #region Likes & favorites

            modelBuilder.Entity<StartupLike>().HasData(
                new StartupLike { Id = 1, StartupId = 1, UserId = 5, CreatedAt = new DateTime(2025, 9, 8) },
                new StartupLike { Id = 2, StartupId = 1, UserId = 6, CreatedAt = new DateTime(2025, 9, 15) },
                new StartupLike { Id = 3, StartupId = 1, UserId = 7, CreatedAt = new DateTime(2025, 10, 2) },
                new StartupLike { Id = 4, StartupId = 2, UserId = 5, CreatedAt = new DateTime(2025, 9, 28) },
                new StartupLike { Id = 5, StartupId = 2, UserId = 6, CreatedAt = new DateTime(2025, 9, 26) },
                new StartupLike { Id = 6, StartupId = 3, UserId = 5, CreatedAt = new DateTime(2025, 10, 3) },
                new StartupLike { Id = 7, StartupId = 3, UserId = 2, CreatedAt = new DateTime(2025, 10, 10) },
                new StartupLike { Id = 8, StartupId = 4, UserId = 6, CreatedAt = new DateTime(2025, 10, 22) },
                new StartupLike { Id = 9, StartupId = 5, UserId = 5, CreatedAt = new DateTime(2025, 11, 2) },
                new StartupLike { Id = 10, StartupId = 6, UserId = 7, CreatedAt = new DateTime(2025, 11, 10) },
                new StartupLike { Id = 11, StartupId = 7, UserId = 5, CreatedAt = new DateTime(2025, 9, 6) },
                new StartupLike { Id = 12, StartupId = 7, UserId = 6, CreatedAt = new DateTime(2025, 10, 26) },
                new StartupLike { Id = 13, StartupId = 7, UserId = 7, CreatedAt = new DateTime(2025, 12, 19) },
                new StartupLike { Id = 14, StartupId = 7, UserId = 2, CreatedAt = new DateTime(2025, 12, 21) }
            );

            modelBuilder.Entity<Favorite>().HasData(
                new Favorite { Id = 1, StartupId = 1, UserId = 5, CreatedAt = new DateTime(2025, 9, 8) },
                new Favorite { Id = 2, StartupId = 2, UserId = 5, CreatedAt = new DateTime(2025, 9, 28) },
                new Favorite { Id = 3, StartupId = 3, UserId = 5, CreatedAt = new DateTime(2025, 10, 3) },
                new Favorite { Id = 4, StartupId = 1, UserId = 6, CreatedAt = new DateTime(2025, 9, 15) },
                new Favorite { Id = 5, StartupId = 5, UserId = 6, CreatedAt = new DateTime(2025, 12, 8) },
                new Favorite { Id = 6, StartupId = 2, UserId = 7, CreatedAt = new DateTime(2025, 10, 14) },
                new Favorite { Id = 7, StartupId = 6, UserId = 7, CreatedAt = new DateTime(2025, 11, 9) },
                new Favorite { Id = 8, StartupId = 3, UserId = 2, CreatedAt = new DateTime(2025, 10, 10) },
                new Favorite { Id = 9, StartupId = 7, UserId = 2, CreatedAt = new DateTime(2025, 12, 21) },
                new Favorite { Id = 10, StartupId = 4, UserId = 7, CreatedAt = new DateTime(2025, 11, 24) }
            );

            #endregion

            #region Blog posts, comments & post likes

            modelBuilder.Entity<BlogPost>().HasData(
                new BlogPost
                {
                    Id = 1,
                    AuthorId = 2,
                    StartupId = 1,
                    Title = "How GreenCycle Started",
                    Content = "It all began when I realized my building had no way to recycle properly. After months of " +
                              "prototyping smart bins in my garage, GreenCycle was born. In this post I want to share the " +
                              "journey so far and what we plan to do with the funding - from producing the first batch of " +
                              "bins to signing up local shops for the rewards program.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 9, 5)
                },
                new BlogPost
                {
                    Id = 2,
                    AuthorId = 3,
                    StartupId = 2,
                    Title = "Why We Built PayLink",
                    Content = "Sending money to a friend across the border should not take three days and cost ten euros. " +
                              "PayLink was born out of that frustration. Here is how our instant payment network works and " +
                              "why we believe the region is ready for it.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 9, 14)
                },
                new BlogPost
                {
                    Id = 3,
                    AuthorId = 4,
                    StartupId = 3,
                    Title = "MediTrack: Digital Health for Everyone",
                    Content = "Chronic patients juggle therapies, appointments and measurements every single day. MediTrack " +
                              "puts all of that in one app connected to the doctor's office. We are sharing our clinical " +
                              "pilot results and the roadmap for the next six months.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 9, 25)
                },
                new BlogPost
                {
                    Id = 4,
                    AuthorId = 5,
                    StartupId = null,
                    Title = "What I Look for Before Investing in a Startup",
                    Content = "After supporting a dozen projects on this platform, here are the five things I always check " +
                              "before donating: a clear problem statement, a realistic funding target, a concrete plan for " +
                              "the money, an active founder who answers questions, and community engagement on the startup page.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 8)
                },
                new BlogPost
                {
                    Id = 5,
                    AuthorId = 2,
                    StartupId = 4,
                    Title = "LearnHub Reaches Its First 100 Students",
                    Content = "A month after our beta launch, 100 students have completed their first course on LearnHub. " +
                              "Here is what we learned from their feedback and how the funding will help us produce twenty " +
                              "new project-based courses.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 11, 2)
                },
                new BlogPost
                {
                    Id = 6,
                    AuthorId = 6,
                    StartupId = null,
                    Title = "The Startup Scene in the Balkans in 2026",
                    Content = "More founders, more capital and more exits than ever before. In this overview I look at the " +
                              "trends shaping the regional startup ecosystem this year and where crowdfunding platforms " +
                              "like Startup.ba fit into the picture.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 11, 18)
                },
                new BlogPost
                {
                    Id = 7,
                    AuthorId = 4,
                    StartupId = 7,
                    Title = "StayLocal Is Fully Funded - Thank You!",
                    Content = "We did it! StayLocal reached its funding target thanks to this amazing community. Launching " +
                              "in three cities this spring. In this post: the full breakdown of how we will spend every euro " +
                              "and how early supporters can claim their free first experience.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 12, 21)
                },
                new BlogPost
                {
                    Id = 8,
                    AuthorId = 8,
                    StartupId = 5,
                    Title = "FarmSense: Smart Sensors for Small Farms",
                    Content = "Big agriculture has had precision farming for a decade. FarmSense brings the same technology " +
                              "to small family farms at a fraction of the cost. Here is how our soil sensors work and the " +
                              "results from our first pilot field.",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 12, 5)
                }
            );

            modelBuilder.Entity<Comment>().HasData(
                new Comment { Id = 1, BlogPostId = 1, UserId = 5, Content = "Love this idea! When is the pilot starting in Sarajevo?", IsActive = true, CreatedAt = new DateTime(2025, 9, 6) },
                new Comment { Id = 2, BlogPostId = 1, UserId = 6, Content = "Just donated. The rewards program is a smart touch.", IsActive = true, CreatedAt = new DateTime(2025, 9, 16) },
                new Comment { Id = 3, BlogPostId = 2, UserId = 7, Content = "Finally! Cross-border payments here are a nightmare.", IsActive = true, CreatedAt = new DateTime(2025, 9, 15) },
                new Comment { Id = 4, BlogPostId = 2, UserId = 5, Content = "Which banks are you partnering with?", IsActive = true, CreatedAt = new DateTime(2025, 9, 20) },
                new Comment { Id = 5, BlogPostId = 3, UserId = 5, Content = "As a nurse, I can confirm patients desperately need this.", IsActive = true, CreatedAt = new DateTime(2025, 9, 26) },
                new Comment { Id = 6, BlogPostId = 4, UserId = 2, Content = "Great checklist, saving this for my next campaign.", IsActive = true, CreatedAt = new DateTime(2025, 10, 9) },
                new Comment { Id = 7, BlogPostId = 4, UserId = 3, Content = "Point four is so true - founders who reply build trust.", IsActive = true, CreatedAt = new DateTime(2025, 10, 10) },
                new Comment { Id = 8, BlogPostId = 5, UserId = 7, Content = "Congrats on the milestone! Any plans for design courses?", IsActive = true, CreatedAt = new DateTime(2025, 11, 3) },
                new Comment { Id = 9, BlogPostId = 6, UserId = 4, Content = "Nice overview. The exit numbers surprised me.", IsActive = true, CreatedAt = new DateTime(2025, 11, 19) },
                new Comment { Id = 10, BlogPostId = 7, UserId = 5, Content = "So happy for you! Claiming my free experience for sure.", IsActive = true, CreatedAt = new DateTime(2025, 12, 21) },
                new Comment { Id = 11, BlogPostId = 7, UserId = 6, Content = "Well deserved, the campaign was transparent from day one.", IsActive = true, CreatedAt = new DateTime(2025, 12, 22) },
                new Comment { Id = 12, BlogPostId = 8, UserId = 5, Content = "What is the price per sensor for a 2-hectare farm?", IsActive = true, CreatedAt = new DateTime(2025, 12, 6) }
            );

            modelBuilder.Entity<BlogPostLike>().HasData(
                new BlogPostLike { Id = 1, BlogPostId = 1, UserId = 5, CreatedAt = new DateTime(2025, 9, 6) },
                new BlogPostLike { Id = 2, BlogPostId = 1, UserId = 6, CreatedAt = new DateTime(2025, 9, 16) },
                new BlogPostLike { Id = 3, BlogPostId = 1, UserId = 7, CreatedAt = new DateTime(2025, 10, 1) },
                new BlogPostLike { Id = 4, BlogPostId = 2, UserId = 5, CreatedAt = new DateTime(2025, 9, 20) },
                new BlogPostLike { Id = 5, BlogPostId = 3, UserId = 5, CreatedAt = new DateTime(2025, 9, 26) },
                new BlogPostLike { Id = 6, BlogPostId = 4, UserId = 2, CreatedAt = new DateTime(2025, 10, 9) },
                new BlogPostLike { Id = 7, BlogPostId = 4, UserId = 3, CreatedAt = new DateTime(2025, 10, 10) },
                new BlogPostLike { Id = 8, BlogPostId = 7, UserId = 5, CreatedAt = new DateTime(2025, 12, 21) },
                new BlogPostLike { Id = 9, BlogPostId = 7, UserId = 6, CreatedAt = new DateTime(2025, 12, 22) },
                new BlogPostLike { Id = 10, BlogPostId = 7, UserId = 7, CreatedAt = new DateTime(2025, 12, 23) }
            );

            #endregion

            #region Chats

            modelBuilder.Entity<Chat>().HasData(
                new Chat { Id = 1, SenderId = 5, ReceiverId = 2, Message = "Hi Adam! I saw GreenCycle and I love the concept. How durable are the smart bins?", CreatedAt = new DateTime(2025, 9, 9, 10, 0, 0), IsRead = true, ReadAt = new DateTime(2025, 9, 9, 10, 15, 0) },
                new Chat { Id = 2, SenderId = 2, ReceiverId = 5, Message = "Hi Sarah, thanks! The casing is recycled aluminum, rated for 10 years outdoors.", CreatedAt = new DateTime(2025, 9, 9, 10, 20, 0), IsRead = true, ReadAt = new DateTime(2025, 9, 9, 10, 25, 0) },
                new Chat { Id = 3, SenderId = 5, ReceiverId = 2, Message = "Impressive. Just sent a donation - good luck with the pilot!", CreatedAt = new DateTime(2025, 9, 10, 9, 0, 0), IsRead = true, ReadAt = new DateTime(2025, 9, 10, 9, 30, 0) },
                new Chat { Id = 4, SenderId = 2, ReceiverId = 5, Message = "Thank you so much! I'll keep you posted on the progress.", CreatedAt = new DateTime(2025, 9, 10, 9, 45, 0), IsRead = true, ReadAt = new DateTime(2025, 9, 10, 10, 0, 0) },
                new Chat { Id = 5, SenderId = 6, ReceiverId = 3, Message = "Hello Emma, does PayLink plan to support business accounts?", CreatedAt = new DateTime(2025, 9, 26, 14, 0, 0), IsRead = true, ReadAt = new DateTime(2025, 9, 26, 15, 0, 0) },
                new Chat { Id = 6, SenderId = 3, ReceiverId = 6, Message = "Hi Mark! Yes, business accounts are on the roadmap for the second release.", CreatedAt = new DateTime(2025, 9, 26, 15, 10, 0), IsRead = true, ReadAt = new DateTime(2025, 9, 26, 16, 0, 0) },
                new Chat { Id = 7, SenderId = 7, ReceiverId = 4, Message = "Hi David, is MediTrack available for testing? My father has diabetes and this would help a lot.", CreatedAt = new DateTime(2025, 11, 4, 11, 0, 0), IsRead = true, ReadAt = new DateTime(2025, 11, 4, 12, 0, 0) },
                new Chat { Id = 8, SenderId = 4, ReceiverId = 7, Message = "Hi Lena, we open the beta next month - I'll add you to the waiting list!", CreatedAt = new DateTime(2025, 11, 4, 12, 30, 0), IsRead = true, ReadAt = new DateTime(2025, 11, 4, 13, 0, 0) },
                new Chat { Id = 9, SenderId = 7, ReceiverId = 4, Message = "That would be amazing, thank you!", CreatedAt = new DateTime(2025, 11, 4, 13, 5, 0), IsRead = false },
                new Chat { Id = 10, SenderId = 6, ReceiverId = 2, Message = "Hey, any update on the GreenCycle pilot locations?", CreatedAt = new DateTime(2025, 12, 29, 18, 0, 0), IsRead = false }
            );

            #endregion

            #region Support tickets

            modelBuilder.Entity<SupportTicket>().HasData(
                new SupportTicket
                {
                    Id = 1,
                    UserId = 5,
                    Subject = "How do I verify my profile?",
                    Message = "I would like to get the verified badge on my profile. What are the requirements and how do I apply?",
                    Status = 1, // Answered
                    AdminResponse = "Hi Sarah! Profile verification requires a completed profile with a real name and a confirmed email address. I have verified your profile - the badge should be visible now.",
                    CreatedAt = new DateTime(2025, 10, 12),
                    AnsweredAt = new DateTime(2025, 10, 13)
                },
                new SupportTicket
                {
                    Id = 2,
                    UserId = 2,
                    Subject = "Problem uploading startup images",
                    Message = "When I try to upload a second image for my startup, the app shows an error. The first image worked fine.",
                    Status = 0, // Open
                    CreatedAt = new DateTime(2025, 12, 27)
                },
                new SupportTicket
                {
                    Id = 3,
                    UserId = 6,
                    Subject = "Payment failed but money was deducted",
                    Message = "I tried to donate 100 EUR to FarmSense yesterday. The payment failed with an error, but my bank shows the amount as reserved.",
                    Status = 1, // Answered
                    AdminResponse = "Hi Mark, the reservation is released automatically by your bank within 3-5 business days since the payment did not complete on our side. No amount was captured. Please try again and contact us if the problem repeats.",
                    CreatedAt = new DateTime(2025, 12, 11),
                    AnsweredAt = new DateTime(2025, 12, 12)
                },
                new SupportTicket
                {
                    Id = 4,
                    UserId = 3,
                    Subject = "Suggestion: category filter for the blog",
                    Message = "It would be great if blog posts could be filtered by startup category, the same way startups can.",
                    Status = 2, // Closed
                    AdminResponse = "Thanks for the suggestion Emma! We added it to our product backlog. Closing this ticket for now.",
                    CreatedAt = new DateTime(2025, 11, 5),
                    AnsweredAt = new DateTime(2025, 11, 6),
                    ClosedAt = new DateTime(2025, 11, 7)
                },
                new SupportTicket
                {
                    Id = 5,
                    UserId = 7,
                    Subject = "How is the platform fee calculated?",
                    Message = "Before I add my own startup I want to understand the fee. Is it charged on every donation or only when the target is reached?",
                    Status = 0, // Open
                    CreatedAt = new DateTime(2025, 12, 30)
                }
            );

            #endregion

            #region Reports

            modelBuilder.Entity<Report>().HasData(
                new Report
                {
                    Id = 1,
                    ReporterId = 5,
                    TargetType = 0, // Startup
                    StartupId = 9,
                    Reason = "Suspicious funding claims",
                    Description = "This startup promises guaranteed returns from crypto trading, which sounds like a scam.",
                    Status = 3, // ActionTaken
                    AdminNote = "Confirmed. The startup was rejected for violating platform standards.",
                    CreatedAt = new DateTime(2025, 11, 6),
                    ResolvedAt = new DateTime(2025, 11, 7)
                },
                new Report
                {
                    Id = 2,
                    ReporterId = 6,
                    TargetType = 1, // BlogPost
                    BlogPostId = 6,
                    Reason = "Inappropriate content",
                    Description = "The statistics in this post look made up and could mislead investors.",
                    Status = 0, // Pending
                    CreatedAt = new DateTime(2025, 12, 20)
                },
                new Report
                {
                    Id = 3,
                    ReporterId = 2,
                    TargetType = 2, // User
                    ReportedUserId = 6,
                    Reason = "Spam messages in chat",
                    Description = "This user keeps sending me the same promotional message over and over.",
                    Status = 1, // Reviewed
                    AdminNote = "Reviewed the chat history. Issued a warning to the reported user.",
                    CreatedAt = new DateTime(2025, 12, 1),
                    ResolvedAt = new DateTime(2025, 12, 2)
                },
                new Report
                {
                    Id = 4,
                    ReporterId = 7,
                    TargetType = 0, // Startup
                    StartupId = 6,
                    Reason = "Misleading description",
                    Description = "The startup claims the demo is 'already available' but I could not find any link to it.",
                    Status = 2, // Dismissed
                    AdminNote = "The demo link is available on the startup's detail page. No violation found.",
                    CreatedAt = new DateTime(2025, 11, 12),
                    ResolvedAt = new DateTime(2025, 11, 13)
                },
                new Report
                {
                    Id = 5,
                    ReporterId = 4,
                    TargetType = 1, // BlogPost
                    BlogPostId = 4,
                    Reason = "Off-topic promotion",
                    Description = "This post seems to promote external investment services rather than platform startups.",
                    Status = 0, // Pending
                    CreatedAt = new DateTime(2025, 12, 29)
                }
            );

            #endregion

            #region Announcements

            modelBuilder.Entity<Announcement>().HasData(
                new Announcement
                {
                    Id = 1,
                    Title = "Welcome to Startup.ba!",
                    Content = "We are excited to launch Startup.ba - the crowdfunding platform for the regional startup community. " +
                              "Browse startups, support the ideas you believe in, share your own project and connect with other founders and investors. " +
                              "Every new startup is reviewed by our team before going live to keep the platform safe and trustworthy.",
                    CreatedByUserId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 8, 1)
                },
                new Announcement
                {
                    Id = 2,
                    Title = "New Categories Added",
                    Content = "Based on your feedback we added three new categories: Artificial Intelligence, Mobility and Social Impact. " +
                              "If your startup fits one of them better, you can update the category from your startup's edit page.",
                    CreatedByUserId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 10, 15)
                },
                new Announcement
                {
                    Id = 3,
                    Title = "Scheduled Maintenance on February 15",
                    Content = "Startup.ba will be unavailable on February 15 from 02:00 to 04:00 CET due to planned infrastructure maintenance. " +
                              "No action is required - all data and running campaigns remain unaffected.",
                    CreatedByUserId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 12, 28)
                }
            );

            #endregion

            #region Notifications

            modelBuilder.Entity<Notification>().HasData(
                new Notification { Id = 1, UserId = 2, Title = "Startup Approved", Message = "Congratulations! Your startup \"GreenCycle\" has been approved and is now visible to investors.", Type = 1, ReferenceId = 1, ReferenceType = "Startup", IsRead = true, CreatedAt = new DateTime(2025, 9, 3) },
                new Notification { Id = 2, UserId = 3, Title = "Startup Approved", Message = "Congratulations! Your startup \"PayLink\" has been approved and is now visible to investors.", Type = 1, ReferenceId = 2, ReferenceType = "Startup", IsRead = true, CreatedAt = new DateTime(2025, 9, 12) },
                new Notification { Id = 3, UserId = 2, Title = "Donation Received", Message = "Sarah Miller donated 5000.00 EUR to \"GreenCycle\".", Type = 4, ReferenceId = 1, ReferenceType = "Donation", IsRead = true, CreatedAt = new DateTime(2025, 9, 10) },
                new Notification { Id = 4, UserId = 2, Title = "Startup Rejected", Message = "Your startup \"CryptoBoost\" has been rejected. Reason: Unrealistic funding target and insufficient business plan details.", Type = 2, ReferenceId = 9, ReferenceType = "Startup", IsRead = true, CreatedAt = new DateTime(2025, 11, 7) },
                new Notification { Id = 5, UserId = 4, Title = "Funding Target Reached", Message = "Congratulations! \"StayLocal\" has reached its funding target of 25000.00 EUR.", Type = 4, ReferenceId = 7, ReferenceType = "Startup", IsRead = true, CreatedAt = new DateTime(2025, 12, 20) },
                new Notification { Id = 6, UserId = 3, Title = "Startup Paused", Message = "Your startup \"RideShare BiH\" has been paused by the administrator.", Type = 3, ReferenceId = 10, ReferenceType = "Startup", IsRead = false, CreatedAt = new DateTime(2025, 12, 10) },
                new Notification { Id = 7, UserId = 2, Title = "New Comment", Message = "Sarah Miller commented on your post \"How GreenCycle Started\".", Type = 5, ReferenceId = 1, ReferenceType = "BlogPost", IsRead = true, CreatedAt = new DateTime(2025, 9, 6) },
                new Notification { Id = 8, UserId = 5, Title = "Support Ticket Answered", Message = "Support has answered your ticket \"How do I verify my profile?\".", Type = 6, ReferenceId = 1, ReferenceType = "SupportTicket", IsRead = true, CreatedAt = new DateTime(2025, 10, 13) },
                new Notification { Id = 9, UserId = 5, Title = "Report Resolved", Message = "Your report \"Suspicious funding claims\" has been reviewed. Outcome: ActionTaken.", Type = 7, ReferenceId = 1, ReferenceType = "Report", IsRead = false, CreatedAt = new DateTime(2025, 11, 7) },
                new Notification { Id = 10, UserId = 1, Title = "New Startup Submitted", Message = "\"SnackWise\" has been submitted and is awaiting your review.", Type = 0, ReferenceId = 8, ReferenceType = "Startup", IsRead = false, CreatedAt = new DateTime(2025, 12, 28) },
                new Notification { Id = 11, UserId = 8, Title = "New Announcement", Message = "Scheduled Maintenance on February 15", Type = 8, ReferenceId = 3, ReferenceType = "Announcement", IsRead = false, CreatedAt = new DateTime(2025, 12, 28) },
                new Notification { Id = 12, UserId = 7, Title = "New Announcement", Message = "Scheduled Maintenance on February 15", Type = 8, ReferenceId = 3, ReferenceType = "Announcement", IsRead = false, CreatedAt = new DateTime(2025, 12, 28) }
            );

            #endregion
        }
    }
}
