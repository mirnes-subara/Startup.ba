# Startup.ba

Crowdfunding platform for startups — ASP.NET Core Web API, RabbitMQ email subscriber, Flutter admin desktop app, and Flutter mobile app.

## Applications

| App | Path | Purpose |
|-----|------|---------|
| Desktop (admin) | `Startupba/UI/startupba_desktop` | Moderation, users, payments, analytics, PDF reports |
| Mobile | `Startupba/UI/startupba_mobile` | Browse startups, donate (Stripe), blog, recommendations, profile |

## Test logins

All seeded users use password **`test`**:

| Username | Role / use |
|----------|------------|
| `desktop` | Administrator (desktop app) |
| `mobile` | Standard user (founder + investor; mobile app) |
| `founder2`, `founder3`, `founder4` | Founders |
| `investor1`, `investor2`, `investor3` | Investors |

## Project structure

```
Startup.ba/
├── Startupba/
│   ├── Startupba.WebAPI/       # ASP.NET Core 8 API + Swagger
│   ├── Startupba.Services/     # EF Core, business logic, JWT, Stripe, RabbitMQ publisher
│   ├── Startupba.Model/        # Requests / responses / search objects
│   ├── Startupba.Subscriber/   # Email worker (RabbitMQ → SMTP)
│   ├── UI/
│   │   ├── startupba_desktop/
│   │   └── startupba_mobile/
│   ├── docker-compose.yml
│   ├── Dockerfile
│   ├── Dockerfile.notifications
│   └── .env                    # Local/secrets config (not committed secrets in docs)
├── recommender-dokumentacija.md
└── README.md
```

## Technology stack

- **Backend:** .NET 8, Entity Framework Core, SQL Server, Mapster, Swagger
- **Auth:** JWT Bearer access tokens + DB-backed refresh tokens
- **Payments:** Stripe (sandbox / test keys via env)
- **Messaging:** RabbitMQ + Subscriber worker for email
- **Frontend:** Flutter (Dart)
- **Infra:** Docker Compose (API, SQL Server, RabbitMQ, subscriber)

## Features

- Startup CRUD, status workflow (pending → approve/reject/pause/resume)
- Donations with Stripe PaymentIntent + admin refunds
- Blog posts and comments
- Content-based startup recommendations (with explainability)
- Likes, favorites, chat, support tickets, reports, announcements
- Admin analytics and **two PDF reports** (dashboard + category)
- In-app notifications and email via RabbitMQ

## Getting started

### Prerequisites

- Docker Desktop (recommended) **or** .NET 8 SDK + SQL Server + RabbitMQ
- Flutter SDK (for UI apps)

### Run API stack with Docker

1. Configure [`Startupba/.env`](Startupba/.env) (SQL, RabbitMQ, SMTP, Stripe, JWT).
2. From the `Startupba` folder:

```bash
docker-compose up --build
```

API default: `http://localhost:5130` (Swagger UI available in Development).

### Run Flutter apps

- **Desktop:** `cd Startupba/UI/startupba_desktop` → `flutter run -d windows` (API `http://localhost:5130/`)
- **Mobile (emulator):** `cd Startupba/UI/startupba_mobile` → `flutter run` (API default `http://10.0.2.2:5130/`)

## Security

- JWT signature validation (issuer, audience, lifetime, signing key)
- Refresh-token logout / password-change revocation
- Password hashing with salt (PBKDF2)
- Role-based authorization (`Administrator`, `User`)
- Register ignores client-supplied admin roles; only admins may assign roles

## License

See LICENSE file for details (if present).
