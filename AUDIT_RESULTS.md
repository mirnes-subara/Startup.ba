# Startup.ba — Compliance Audit vs. Razvoj softvera II Requirements

Audit of [Startup.ba](file:///c:/Users/Administrator/Desktop/Startup.ba) project against every requirement in the seminar document.

---

## Summary Score

| Area | Status | Issues |
|---|---|---|
| Authentication & Authorization | ⚠️ Partial | JWT done; register still accepts `roleIds` from client; some `[AllowAnonymous]` GETs remain |
| Service Lifetime (DI) | ✅ **FIXED** | All 21 services registered as `Scoped` in `Program.cs` |
| Logging | ✅ **FIXED** | Replaced `Console.WriteLine` everywhere with `ILogger<T>` |
| DateTime Consistency | ✅ **FIXED** | Replaced all `DateTime.Now` with `DateTime.UtcNow` across database models & services |
| Pagination — `RetrieveAll` | ⚠️ Major | All list endpoints expose `RetrieveAll` flag — guideline says this is a rejection-worthy error |
| RabbitMQ Connection | ✅ **FIXED** | Converted to singleton `IRabbitMqPublisher` service with DI |
| Recommender Documentation | ⚠️ Missing | `recommender-dokumentacija.md` file not found in repo |
| API URL Config | ⚠️ Minor | Uses `baseUrl` instead of `API_BASE_URL` as specified in the doc |
| Docker Image Tags | ✅ OK | SQL Server `2022-latest`, RabbitMQ `3-management` — acceptable |
| Microservice Architecture | ✅ OK | Separate Subscriber worker, RabbitMQ, docker-compose present |
| Stripe Integration | ✅ Good | Real sandbox, server-side verification, refund implemented |
| State Machine | ✅ Good | Centralized status transitions with validation in `StartupService` |
| Configuration | ✅ Good | `.env` file used, docker-compose references env vars |
| Password Hashing | ✅ OK | PBKDF2 with `RandomNumberGenerator` salt |

---

## 🔴 Critical Issues (Likely Rejection)

### 1. Services Registered as `Transient` — Must Be `Scoped` — ✅ FIXED
> **Requirement §3.4**: *"Servisi koji koriste DbContext moraju biti registrovani kao Scoped, a ne kao Transient."*

- **Status:** **COMPLETED**
- **Changes Made:** Updated [Program.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.WebAPI/Program.cs#L51-L71) to register all 21 service interfaces as `AddScoped`.

---

### 2. `Console.WriteLine` Instead of `ILogger<T>` — ✅ FIXED
> **Requirement §8.1**: *"Za logiranje koristiti ILogger\<T\> umjesto Console.WriteLine."*

- **Status:** **COMPLETED**
- **Changes Made:** Injected `ILogger<T>` across 7 services (`StartupService`, `UserService`, `DonationService`, `CommentService`, `ReportService`, `SupportTicketService`, `AnnouncementService`) and `RabbitMqPublisher`, replacing all `Console.WriteLine` calls with `_logger.LogError(...)`.

---

### 3. RabbitMQ — New Connection on Every Publish — ✅ FIXED
> **Requirement §A.1**: *"Ne kreirati novu RabbitMQ konekciju pri svakom publish pozivu; koristiti singleton konekciju ili connection pool."*

- **Status:** **COMPLETED**
- **Changes Made:**
  - Converted `RabbitMqPublisher` from a static class to an injectable service implementing [IRabbitMqPublisher](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Services/Interfaces/IRabbitMqPublisher.cs).
  - `IBus` connection is now created **once** in the constructor and reused for the application lifetime.
  - Environment variables are read **once** at startup instead of per-call.
  - Registered as `Singleton` in [Program.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.WebAPI/Program.cs).
  - Updated `StartupService`, `DonationService`, and `SupportTicketService` to inject `IRabbitMqPublisher` via DI.

---

### 4. Authentication: Basic Auth Instead of JWT — ✅ FIXED
> **Requirement §2.4**: *"autentifikaciju i autorizaciju korisnika zasnovanu na JWT-u"*
> **Requirement §5**: *"JWT parsiranje mora uključivati validaciju potpisa. Logout mora invalidirati token na serveru..."*

- **Status:** **COMPLETED**
- **Changes Made:**
  - Replaced Basic Auth with `JwtBearer` in [Program.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.WebAPI/Program.cs) (issuer/audience/lifetime/signing-key validation; `RoleClaimType = ClaimTypes.Role`).
  - Added `RefreshToken` entity + migration; `IJwtTokenService` issues access (60m) + DB-backed refresh (7d).
  - `POST Users/authenticate` returns `LoginResponse`; `POST Users/refresh` rotates refresh; `POST Users/logout` revokes refresh server-side; password change revokes all user refresh tokens and returns new tokens.
  - Deleted `BasicAuthenticationHandler.cs`; Swagger uses Bearer JWT.
  - Mobile/desktop clients send `Authorization: Bearer` and refresh once on 401.

---

### 5. `RetrieveAll` — Unbounded List Endpoints
> **Requirement §8.2**: *"Endpointi tipa RetrieveAll bez limita smatraju se greškom za neprihvatanje."*

[BaseSearchObject.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Model/SearchObjects/BaseSearchObject.cs#L13) exposes `RetrieveAll = false` and it's used in **14 services** to skip pagination entirely. There's no `PageSize` max limit either.

**Fix**: Remove `RetrieveAll` property entirely. Enforce a `PageSize` max (e.g., 100).

---

## ⚠️ Major Issues

### 6. Register Endpoint Accepts `RoleIds` from Client
> **Requirement §5**: *"Register endpoint ne smije primati role/isAdmin vrijednosti od klijenta."*

[UserService.CreateAsync](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Services/Services/UserService.cs#L155-L157) reads `request.RoleIds`:
```csharp
var roleIdsToAssign = (request.RoleIds != null && request.RoleIds.Any())
    ? request.RoleIds
    : new List<int> { 2 };
```

And the [POST Users endpoint](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.WebAPI/Controllers/UsersController.cs#L42-L49) is `[AllowAnonymous]`, meaning anyone can register with any role (including Admin).

**Fix**: Ignore `RoleIds` on anonymous registration. Only allow admin to assign roles.

---

### 7. Mixed `DateTime.Now` / `DateTime.UtcNow` — ✅ FIXED
> **Requirement §A.4**: *"Standardizovati DateTime.UtcNow u cijeloj aplikaciji."*

- **Status:** **COMPLETED**
- **Changes Made:** Replaced all 43 occurrences of `DateTime.Now` in service methods and EF entity model default initializers across the solution with `DateTime.UtcNow`. Verified zero `DateTime.Now` calls remain.

---

### 8. Excessive `[AllowAnonymous]` on Read Endpoints
> **Requirement §5**: *"[AllowAnonymous] je dozvoljen isključivo na login/register endpointima."*

**26+ endpoints** are `[AllowAnonymous]` for read operations (GET):
- Category, City, Country, Gender, Role, PlatformSetting, BlogPost, Comment, Announcement, StartupImage, StartupStatus — all have anonymous GET + GET by ID

While some public reads may be justified (e.g., browsing startups), endpoints like **Role**, **PlatformSetting**, and **Comment** should not be publicly accessible.

**Fix**: Remove `[AllowAnonymous]` from non-public endpoints. Keep it only on login, register, and genuinely public browsing endpoints.

---

### 9. Like/Favorite Endpoints Accept `userId` from Query String — ✅ FIXED
> **Requirement §5**: *"userId se nikada ne prima iz rute ili body-ja za operacije vezane za trenutnog korisnika; uvijek se preuzima iz JWT tokena."*

- **Status:** **COMPLETED**
- **Changes Made:** Startup like / unlike / favorite / remove-favorite endpoints now take `userId` from `ClaimTypes.NameIdentifier` (no query `userId`). Mobile clients updated accordingly. (BlogPost like still uses query `userId` — separate follow-up.)

---

### 10. `recommender-dokumentacija.md` Missing
> **Requirement §9.2**: *"Postaviti dokument sa opisom sistema preporuke na git repozitorij seminarskog rada (recommender-dokumentacija.md)."*

No `recommender-dokumentacija.md` found in the repository.

**Fix**: Create the document describing the content-based recommendation algorithm used in [StartupService.GetRecommendedStartupsAsync](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Services/Services/StartupService.cs#L607-L706).

---

### 11. Recommendation Endpoint Is Anonymous — ✅ PARTIALLY FIXED
> **Requirement §5**: Write endpoints must be protected, and recommendations rely on user data.

- **Status:** **PARTIALLY COMPLETED** (this endpoint)
- **Changes Made:** `GET Startup/recommended` now requires auth and uses JWT `NameIdentifier` (route `{userId}` removed). Broader `[AllowAnonymous]` cleanup on other GETs remains as priority item 11.

---

### 12. Multiple `SaveChangesAsync` Without Transaction
> **Requirement §3.4**: *"Višestruki SaveChangesAsync() pozivi u jednoj operaciji moraju biti unutar eksplicitne transakcije."*

[UserService.CreateAsync](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Services/Services/UserService.cs#L152-L169) calls `SaveChangesAsync()` twice (once for user, once for roles) without wrapping in a transaction.

**Fix**: Wrap in `using var transaction = await _context.Database.BeginTransactionAsync()`.

---

### 13. Recommender Lacks Explainability
> **Requirement §2.4**: *"Recommender mora korisniku objašnjavati zbog čega se određeni sadržaj preporučuje - objašnjive preporuke."*

[GetRecommendedStartupsAsync](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Services/Services/StartupService.cs#L607-L706) returns a `List<StartupResponse>` — there's no explanation text telling the user *why* each startup was recommended.

**Fix**: Add a `RecommendationReason` field to the response (e.g., "Based on your interest in Technology startups" or "Popular in your area").

---

## ⚠️ Minor Issues

### 14. `UserException` vs Custom Exception Types
> **Requirement §3.4**: *"Koristiti custom exception tipove (npr. BusinessException, NotFoundException)"*

The code uses `UserException` and `InvalidOperationException`. There's no `NotFoundException` — 404 cases just return `null` in services. Consider adding `NotFoundException` for consistency.

### 15. `Rfc2898DeriveBytes` Missing HashAlgorithm Parameter
[PasswordGenerator.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.Services/Helpers/PasswordGenerator.cs#L38) uses the deprecated 2-parameter constructor:
```csharp
new Rfc2898DeriveBytes(password, saltBytes, Iterations)
```
Should specify `HashAlgorithmName.SHA256` to avoid the SHA1 default.

### 16. API URL Environment Variable Naming
> **Requirement §3.3**: *"flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000"*

Mobile app uses `baseUrl` instead of `API_BASE_URL`:
```dart
baseUrl = const String.fromEnvironment("baseUrl", defaultValue: "");
```

**Fix**: Rename to `API_BASE_URL` to match the spec.

### 17. `ISystemClock` Deprecated — ✅ FIXED (handler removed)
Resolved by deleting `BasicAuthenticationHandler` as part of the JWT migration.

### 18. README Uses "eRent" Title Instead of Project Name
[README.md](file:///c:/Users/Administrator/Desktop/Startup.ba/README.md#L1) starts with `# eRent` and references "eRent" throughout, but the project is **Startup.ba**. This appears to be a copy-paste remnant.

> **Requirement §8.1**: *"Potrebno je ispraviti greške u nazivima klasa i datoteka; ne ostavljati copy-paste ostatke iz drugih projekata."*

### 19. CORS Not Configured
> **Requirement §3.4**: *"CORS konfiguraciju treba definisati jednom i eksplicitno navesti dozvoljene origine."*

[Program.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.WebAPI/Program.cs) has no CORS configuration at all.

### 20. Swagger Exposed in Production
[Program.cs](file:///c:/Users/Administrator/Desktop/Startup.ba/Startupba/Startupba.WebAPI/Program.cs#L133-L137) has Swagger enabled unconditionally (the `if` check is commented out).

### 21. PDF Reports Missing
> **Requirement §2.2**: *"Potrebno je omogućiti minimalno dva izvještaja u .pdf formatu."*

No PDF generation library or report endpoints found in the codebase.

---

## Priority Fix Order

| Priority | Issue | Status | Effort |
|---|---|---|---|
| 1 | Switch services to `Scoped` | ✅ **DONE** | 5 min |
| 2 | Replace `Console.WriteLine` with `ILogger<T>` | ✅ **DONE** | 30 min |
| 3 | Replace Basic Auth with JWT | ✅ **DONE** | 2-4 hours |
| 4 | Remove `RetrieveAll`, enforce PageSize max | ⏳ Next | 30 min |
| 5 | Fix register endpoint (ignore client `RoleIds`) | ⏳ Next | 15 min |
| 6 | Standardize `DateTime.UtcNow` | ✅ **DONE** | 30 min |
| 7 | Fix userId from JWT in like/favorite endpoints | ✅ **DONE** | 20 min |
| 8 | Singleton RabbitMQ connection | ✅ **DONE** | 30 min |
| 9 | Add recommendation explainability | ⏳ Pending | 1 hour |
| 10 | Create `recommender-dokumentacija.md` | ⏳ Pending | 1 hour |
| 11 | Remove excessive `[AllowAnonymous]` | ⏳ Pending | 30 min |
| 12 | Add transaction to multi-SaveChanges operations | ⏳ Pending | 20 min |
| 13 | Rename README from eRent → Startup.ba | ⏳ Pending | 10 min |
| 14 | Add CORS configuration | ⏳ Pending | 10 min |
| 15 | Rename `baseUrl` → `API_BASE_URL` | ⏳ Pending | 10 min |
| 16 | Implement PDF reports | ⏳ Pending | 2-3 hours |
