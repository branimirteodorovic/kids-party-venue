# Implementation plan

The roadmap for the KidsPartyVenue backend, organized into milestones and small, independently shippable steps. The frontend (Angular SSR for the public site, SPA panels for providers and admins) is planned separately.

## Key decisions

| Topic | Decision | Notes |
|---|---|---|
| Database | PostgreSQL 18 + PostGIS 3.6 | `geography(Point, 4326)` with a GiST index; distances in meters. Domain keeps a plain `GeoCoordinate` value object, mapped to `Point` in Infrastructure. |
| Background jobs | Hangfire with PostgreSQL storage | Its own `hangfire` schema; jobs live in Infrastructure and call Application handlers. |
| Side effects | Transactional outbox | Domain events and outbox rows are saved in one transaction; a job dispatches them with retries to idempotent consumers. |
| Files | `IFileStorage` | Local disk in development, Cloudflare R2 (S3 API) on staging and production. |
| Auth | ASP.NET Core Identity + JWT access tokens + rotating refresh tokens | Permissions resolved per request from roles; provider access checked by membership. Google and Facebook sign-in come after the MVP (P2.9). |
| Hosting | One VPS per environment: Docker Compose + Caddy | Images in GHCR, deployed over SSH. Migrations run as a separate step (EF Core migration bundle). |
| Money and time | `decimal` in RSD; UTC `timestamptz` in the database | Local dates and times in `Europe/Belgrade` for party dates and availability rules. |
| API contracts | Paging, ProblemDetails, stable DTOs | Times are returned both as UTC instants and as local date and time. |

## Delivery conventions

- Trunk-based: work happens directly on `main`, in small, focused commits.
- Each change comes with tests: unit tests for domain logic, integration tests (Testcontainers, `postgis/postgis`) for SQL, constraints and handlers.
- CI runs on every push to `main`. A push to `main` deploys to staging.
- Schema changes go through EF Core migrations. Each vertical slice follows `.claude/rules/architecture.md`.

---

## Milestone 0 — Foundation

### 0.0 GitHub repository
- Publish the repository with `main` as the default branch.

### 0.1 Unit test project
- `tests/KidsPartyVenue.UnitTests` (xUnit, AwesomeAssertions, Bogus, NSubstitute).

### 0.2 Integration test infrastructure
- `tests/KidsPartyVenue.IntegrationTests` with a `WebApplicationFactory` and a shared `postgis/postgis:18-3.6` Testcontainer.
- Migrations are applied once, and Respawn resets data between tests.

### 0.3 Local development environment
- Add Seq (structured logs) and Mailpit (dev SMTP) to Docker Compose, and pgAdmin behind a `tools` profile.
- Add the Serilog Seq sink.
- Move secrets to `.env` and commit an `.env.example`.

### 0.4 CI pipeline
- A GitHub Actions workflow that restores, builds, runs all tests including the Testcontainers tests, and builds the Docker image, on every push to `main`.

### 0.5 Background jobs
- Hangfire with PostgreSQL storage (`hangfire` schema), with an explicit registration of recurring jobs in Belgrade time.
- The dashboard at `/jobs`: open in Development; on servers it sits behind Caddy basic auth.
- A heartbeat job as the first recurring job.

### 0.6 Identity, JWT and refresh tokens
- **0.6a** ASP.NET Core Identity with `Guid` keys in an `identity` schema, seeding the roles `Parent`, `ProviderMember` and `Admin`.
- **0.6b** Register and login; short-lived HS256 JWT access tokens.
- **0.6c** Refresh tokens: stored hashed, rotated on every use, with reuse detection that revokes the whole token family. They're delivered in an HttpOnly cookie.
- **0.6d** `IPermissionService` (role → permissions, cached), `IUserContext` and `GET /users/me`.
- **0.6e** Rate limiting on the auth endpoints.

### 0.7 Social sign-in
- Moved after the MVP; see P2.9.

### 0.8 Staging deployment
- A hardened VPS with Docker Compose: API, PostGIS, Seq and Caddy (automatic HTTPS).
- The CI publishes the API and migrator images to GHCR, then deploys over SSH and runs migrations as a separate step.
- Forwarded headers and persisted Data Protection keys.

---

## Milestone 1 — Providers and offerings

### 1.1 Cache tags
- Tag support in `ICacheService` (`RemoveByTagAsync`), backed by HybridCache tags.

### 1.2 Lookup data
- `District` (Novi Sad districts and nearby places) and `Amenity`, with seed data.
- `ProviderCategory` enum.
- `GET /districts` and `GET /amenities`.

### 1.3 Provider entity
- `Provider` with slug, name, category, description, address, district, location, status, claimed flag and activity timestamps.
- Repository, EF configuration (geography column + GiST index, unique slug) and migration.

### 1.4 Slug generation
- Serbian Latin transliteration and uniqueness.

### 1.5 BookingPolicy
- A value object with deposit type and value, deposit due days, free cancellation days and quantity change days.

### 1.6 VenueDetails and ServiceDetails
- **Venues:** capacities, age range, outside catering and cake rules, included food, own animator or photographer, smoking area.
- **Services:** service-area districts, delivery, lead time, maximum orders per day.

### 1.7 Provider amenities
- The many-to-many relation between providers and amenities.

### 1.8 Admin: create provider
- `POST /admin/providers` creates an unclaimed Draft profile.

### 1.9 Update provider
- Commands and endpoints for basic info, details, booking policy and amenities.

### 1.10 Provider membership
- `ProviderMember (Owner | Staff)` inside the `Provider` aggregate, with last-owner protection.
- Resource-based authorization policies (`ProviderMember`, `ProviderOwner`) for routes under `providers/{providerId}/...`, with cached memberships.

### 1.11 Public provider page
- `GET /providers/{slug}`: Dapper, cached with the tag `provider:{id}`; only `Published` providers are returned.

### 1.12 Self-registration
- A logged-in user creates a Draft provider and becomes its Owner.

### 1.13 Review workflow
- `Draft → PendingReview → Published | ChangesRequested`, with review notes, plus suspend and unsuspend.

### 1.14 Audit log
- A SaveChanges interceptor writes entity diffs as `jsonb`. Moderation status is set for name changes and new media on published profiles.

### 1.15 Offerings
- The pricing models (per child, person, adult, kg, piece, hour, flat), minimum quantity, duration and included items.

### 1.16 Offering extras
- Paid add-ons, and a per-offering booking policy override.

---

## Milestone 2 — Domain events, outbox and email

### 2.1 Domain events
- Domain events raised on entities.

### 2.2 Outbox
- An outbox table, filled by a SaveChanges interceptor in the same transaction.

### 2.3 Outbox processor
- A recurring job with `FOR UPDATE SKIP LOCKED`, retries and error recording.

### 2.4 Idempotent consumers
- Records of the handled `(message, handler)` pairs.

### 2.5 Notifications
- `INotificationChannel`, an email channel (MailKit) and a notification log.

### 2.6 Email templates
- Serbian Latin templates for invitations and review results.

### 2.7 Provider invitations
- Hashed invitation tokens valid for 7 days, sent through the outbox, with revoke and resend.

### 2.8 Claim
- `POST /claim` validates the token, creates or links the user, adds them as Owner and marks the provider claimed.

---

## Milestone 3 — Media and search

### 3.1 File storage
- `IFileStorage` with a local implementation, immutable keys and `Cache-Control: immutable` serving.

### 3.2 Image pipeline
- Uploads up to 10 MB, with JPEG, PNG, WebP or HEIC detected from the content.
- Auto-orient, strip EXIF, and produce WebP variants at 400, 800 and 1600 px. The original isn't kept.

### 3.3 Provider media
- Upload, delete and reorder; moderation on published profiles; storage keys `providers/{providerId}/{mediaId}/{size}.webp`.

### 3.4 R2 storage
- An S3-compatible implementation for Cloudflare R2, served from a custom media domain through the Cloudflare CDN.

### 3.5 Search with filters
- Dapper search by category, district or service area, capacity, age, price and amenities, with PostGIS radius and distance ordering, and paging.

### 3.6 Full-text search
- A generated `tsvector` column with a diacritics-insensitive normalization function (`unaccent` and đ→dj), a GIN index and `websearch_to_tsquery`.

### 3.7 Ranking, sponsored results, caching and search log
- Ranking by relevance, paid plan, profile completeness, recent activity and rating.
- Sponsored results flagged `IsSponsored`.
- Short-lived cached result pages.
- An asynchronous search log, with zero-result searches flagged.

---

## Milestone 4 — Availability

### 4.1 Availability rules
- An `AvailabilitySchedule` aggregate holding the rules and blocked periods.
- Rules are `Regular` or `OnlyOpenBefore`, with validity dates, weekdays and local time slots.
- Cross-rule overlap validation.

### 4.2 SlotGenerator
- A pure domain service that turns rules into UTC slot instants per local date, with correct DST handling. `OnlyOpenBefore` takes precedence over `Regular` on the dates it covers.

### 4.3 Slots
- One row per bookable slot, with the statuses Open, Held, Booked and Blocked.
- A unique start per provider, a `btree_gist` exclusion constraint against overlaps, and `xmin` optimistic concurrency.

### 4.4 Slot regeneration
- Idempotent regeneration on rule changes and nightly for the next 9 months.
- Held, Booked and Blocked slots are never touched; mismatches are reported as conflicts.

### 4.5 Availability preview
- The calendar for unsaved rules, without persisting anything, with conflicts marked.

### 4.6 Initial fill, schedule editing and QuickBlock
- "Booked until date X except these slots, then this weekly schedule" in one call.
- Replacing the whole schedule later.
- Bulk blocking and unblocking of slots.

### 4.7 Free slots
- A public query for a provider's open slots, in UTC and local time.

### 4.8 Daily capacity
- A maximum number of orders per day for services, enforced with optimistic concurrency.

---

## Milestone 5 — Inquiries and bookings

### 5.1 Phone verification and anti-abuse
- SMS one-time codes (HMAC-hashed, with expiry and limited attempts) and single-use verification tokens.
- Rate limits per IP and per phone number, a honeypot field, and an allow-list of country codes.

### 5.2 Price quote and booking terms
- Pure calculators for price (pricing models, minimum quantity, extras) and booking terms: deposit amount, deposit due date, free-cancellation and quantity-change deadlines.
- A public quote endpoint.

### 5.3 Create inquiry
- Anonymous inquiries with a verified phone, with capacity, age, lead-time and daily-capacity checks, a policy snapshot and a quoted price.

### 5.4 Inquiry tracking
- Private tracking links for parents (hashed tokens).
- An inquiry list and detail for providers, with seen tracking.

### 5.5 Accept and decline
- Acceptance computes the terms and holds the slot (or reserves daily capacity) in one transaction, protected against concurrent acceptance. Competing inquiries are declined automatically.

### 5.6 Booking payments
- Recording the deposit, the balance and refunds. The slot becomes Booked once the deposit is recorded.

### 5.7 Cancellation, expiry and completion
- Cancellation by the parent or the provider, refunded or forfeited depending on the deadline, with a manual refund override.
- Jobs that expire unpaid holds and complete past bookings.

### 5.8 Inquiry notifications and reminders
- Notifications on every status change, plus reminders before the deposit deadline, the cancellation deadline and the party.

### 5.9 Inquiry messages
- A parent ↔ provider message thread on each inquiry.

---

## Milestone 6 — Billing

### 6.1 Plans and subscriptions
- Plans with features.
- The subscription lifecycle `Trial → Active → PastDue → Expired` with a 7-day grace period; cancellation means no renewal.
- Promo codes, and at most one current subscription per provider.

### 6.2 Feature access
- `IFeatureAccess`, cached and invalidated on subscription changes, and used in search ranking.

### 6.3 Invoices
- Configurable issuer settings, an issuer snapshot on each invoice, and gap-free yearly invoice numbering.

### 6.4 IPS QR
- An NBS IPS QR payload and image.

### 6.5 Invoice PDF
- PDF invoices generated with QuestPDF.

### 6.6 Manual payments
- An admin records a payment, the invoice is marked paid and the subscription is extended.

### 6.7 Renewal invoices
- A daily job that issues the invoice 7 days before the period ends.

### 6.8 Subscription expiry
- A daily job that moves subscriptions to past due and then expired, with notifications.

---

## Milestone 7 — Admin API

- **7.1** Review queue.
- **7.2** Provider list with filters.
- **7.3** Invitations overview.
- **7.4** Subscriptions, invoices and payments.
- **7.5** Audit history.
- **7.6** Lookup data management (districts, amenities, plans, promo codes).

---

## Milestone 8 — Phase 1 release

### 8.1 Data retention
- Anonymize parent contact data on old inquiries.

### 8.2 Production environment
- A separate production server.
- Manual deployments of image tags already verified on staging.
- Nightly `pg_dump` to R2 with retention, and a documented restore procedure.
- Uptime, error, disk and backup-heartbeat alerts.

### 8.3 Pre-release review
- A security review, and rate limits on every anonymous endpoint.

---

## Phase 2

Phase 1 (Milestones 0–8) is the MVP. Everything below comes after it.

| Step | Content |
|---|---|
| P2.1 | Search by free slot in a time window |
| P2.2 | Party request: one parent request creates inquiries to several providers in one transaction |
| P2.3 | Quantity change requests with the minimum-quantity floor and price recalculation |
| P2.4 | Verified reviews through a one-time link after a completed booking, admin moderation, public provider replies, aggregate rating |
| P2.5 | SMS and Viber Business notification channels |
| P2.6 | Embeddable provider calendar |
| P2.7 | Provider statistics |
| P2.8 | SEO data endpoints (district × category listings, sitemap) |
| P2.9 | Social sign-in: Google (ID token) and Facebook (access token checked through the Graph API), with account linking, pre-account-takeover protection and Facebook's data deletion callback |
| P2.10 | Refresh token cleanup: a recurring Hangfire job that deletes refresh tokens that expired or were revoked more than a retention period ago (for example 30 days) |

## Phase 3

| Step | Content |
|---|---|
| P3.0 | Portion guidelines, and the animator, photographer and decoration categories |
| P3.1 | Deterministic planner: candidates per category, compatibility rules, quantities, budget-constrained top-3 combinations, alternatives |
| P3.2 | LLM-based request parsing (structured output, validated) and explanations over computed results only |
| P3.3 | Natural-language schedule parsing into availability rules, always previewed and confirmed |
| P3.4 | Party invitations and RSVPs, with order updates based on confirmed guests |
| P3.5 | Card payments: tokenization, recurring charges, idempotent webhooks, charge retries |
| P3.6 | Birthday reminders |

---

## Dependencies

```
0.0 ─┬─> 0.1 / 0.2 tests ──> 0.4 CI ──> 0.8 staging
     ├─> 0.3 local environment
     ├─> 0.5 background jobs ──> M2 outbox and email ──> 2.7 invitations
     └─> 0.6 identity ──> 1.8 admin create
                      └─> 1.10 membership ──> 1.12 self-registration, 2.8 claim
M1 providers ──> M3 media and search
M1 providers ──> M4 availability ──> M5 inquiries ──> 5.8 notifications
6.1 subscriptions ─┬─> 6.2 feature access ──> 3.7 ranking with real plan data
                   └─> 6.6 / 6.8 billing jobs
```
