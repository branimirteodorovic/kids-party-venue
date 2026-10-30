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
| Analytics | Anonymous search and engagement events, aggregated without provider IDs | No names, phone numbers or IP addresses. Other providers' data is never used for a provider's suggestions, and suggestions never include prices. |
| Market | Serbia, city by city; venues first | Every city and municipality is seeded, and all are hidden. The admin opens a category in a city once enough providers there are published. Venues come first; catering and cakes follow after the MVP (P2.11). |
| Public visibility | Published, and the provider's category is open in its city | One shared SQL condition used by every public query. Closing a city again hides it and its profiles from parents; existing inquiries and bookings carry on. |
| Languages | Serbian (Latin, default), English and Russian | The frontend translates the interface. The API returns lookup names (cities, districts, amenities) in the request language (`Accept-Language`) and sends each notification in its recipient's language. Texts written by providers are shown as written; invoices are Serbian only. |
| AI features | Anthropic API behind Application interfaces | Model output is untrusted input: structured, validated like a request, and mapped to lookup data on the server. The model never gets database access or provider data, every AI feature falls back to the non-AI path, and prompt or model changes are checked against an eval set first. |

## Delivery conventions

- Trunk-based: work happens directly on `main`, in small, focused commits.
- Each change comes with tests: unit tests for domain logic, integration tests (Testcontainers, `postgis/postgis`) for SQL, constraints and handlers.
- CI runs on every push to `main`. Once staging exists (8.0), a push to `main` also deploys there.
- Servers are added only when they're needed: development and tests run locally until the release milestone.
- Schema changes go through EF Core migrations. Each vertical slice follows `.claude/rules/backend-architecture.md` (at the repository root).
- Each step gets a one-page spec in [docs/specs](specs/README.md) before it's built. This plan stays a roadmap; the detail lives in the specs, written just in time so they match the code.

## Working with an AI coding agent

Most backend code is written by an AI coding agent (Claude Code). The owner sets the direction, approves each spec and reviews every change.

- **Standing context**, written once and kept up to date: the root and backend `CLAUDE.md`, the backend architecture and coding-style rules in `.claude/rules/`, and skills for repeated tasks (`backend-write-spec`, `backend-add-vertical-slice`, `backend-db-migration`).
- **Per step**: the agent drafts the spec from this plan and the current code, with its decisions and open questions. The owner edits and approves it, and only then is code written. Anything that changes during the build goes back into the spec.
- **Checks**: tests and CI on every push, the `backend-arch-reviewer` agent before every commit, a hook that blocks hand edits to migrations, and the owner's review of every diff.

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
- Moved to 8.0, so no server runs before one is needed.

### 0.9 Languages
- Supported languages `sr` (default), `en` and `ru`, read from `Accept-Language` by the request localization middleware. Anything else falls back to `sr`.
- `ILanguageContext` gives handlers and queries the current language.
- A `LocalizedText` value object for lookup data: Serbian is required, English and Russian fall back to Serbian when missing.
- `PreferredLanguage` on users, set at registration or claim and changeable later. It decides the language of the user's notifications.

---

## Milestone 1 — Providers and offerings

### 1.1 Cache tags
- Tag support in `ICacheService` (`RemoveByTagAsync`), backed by HybridCache tags.

### 1.2 Lookup data
- `City`: every city and municipality in Serbia, seeded from the official register, with a slug, names in three languages, the locative form used in phrases such as "u Novom Sadu", and a center point. All cities start hidden.
- `District` belongs to a city. The Novi Sad districts and nearby places are seeded; other cities get theirs when they open, and a city may have none.
- `Amenity` with names in three languages, with seed data.
- `ProviderCategory` enum, and `CityCategory`: which categories are open in which city. Nothing is open at first; the development seed opens venues in Novi Sad.
- `GET /catalogs` (the open cities with their open categories and districts, plus amenities) and `GET /cities` (all cities, for registration and the profile editor), with names in the request language.

### 1.3 Provider entity
- `Provider` with slug, name, category, description, address, city, district, location, status, claimed flag and activity timestamps. The district is required only in cities that have districts.
- Repository, EF configuration (geography column + GiST index, unique slug) and migration.

### 1.4 Slug generation
- Serbian Latin transliteration (Cyrillic names included) and uniqueness.
- Words used by the public URL structure (`kvart`) are never generated as a slug.

### 1.5 BookingPolicy
- A value object with deposit type and value, deposit due days, free cancellation days and quantity change days.

### 1.6 VenueDetails
- Capacities, age range, outside catering and cake rules, included food, own animator or photographer, smoking area.
- `ServiceDetails` for catering and cakes moved after the MVP; see P2.11.

### 1.7 Provider amenities
- The many-to-many relation between providers and amenities.

### 1.8 Admin: create provider
- `POST /admin/providers` creates an unclaimed Draft profile in any city. Until P2.11, only the `Venue` category is accepted.

### 1.9 Update provider
- Commands and endpoints for basic info, details, booking policy and amenities.

### 1.10 Provider membership
- `ProviderMember (Owner | Staff)` inside the `Provider` aggregate, with last-owner protection.
- Resource-based authorization policies (`ProviderMember`, `ProviderOwner`) for routes under `providers/{providerId}/...`, with cached memberships.

### 1.11 Public provider page
- `GET /providers/{slug}`: Dapper, cached with the tag `provider:{id}`.
- Only publicly visible providers are returned: `Published`, with their category open in their city. The condition lives in one shared SQL fragment that every public query uses.
- The response includes the city and category, so the frontend can redirect to the canonical URL.

### 1.12 Self-registration
- A logged-in user creates a Draft provider and becomes its Owner.
- Any city can be chosen. A venue in a city that isn't open yet can still be reviewed and published; parents see it once its city opens. Until P2.11, only venues can register.

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
- Each notification is rendered in its recipient's language.

### 2.6 Email templates
- Templates in Serbian (Latin), English and Russian for invitations and review results.
- An invitation's language is chosen when the admin sends it; after the claim, the owner's preferred language applies.

### 2.7 Provider invitations
- Hashed invitation tokens valid for 7 days, sent through the outbox, with revoke and resend.

### 2.8 Claim
- `POST /claim` validates the token, creates or links the user, adds them as Owner and marks the provider claimed.

---

## Milestone 3 — Media and search

### 3.1 File storage
- `IFileStorage` with a local implementation, immutable keys and `Cache-Control: immutable` serving.
- The API image creates a media folder writable by its non-root user.

### 3.2 Image pipeline
- Uploads up to 10 MB, with JPEG, PNG, WebP or HEIC detected from the content.
- Auto-orient, strip EXIF, and produce WebP variants at 400, 800 and 1600 px. The original isn't kept.

### 3.3 Provider media
- Upload, delete and reorder; moderation on published profiles; storage keys `providers/{providerId}/{mediaId}/{size}.webp`.

### 3.4 R2 storage
- An S3-compatible implementation for Cloudflare R2, served from a custom media domain through the Cloudflare CDN.
- Used only on servers, so it's built together with staging (8.0).

### 3.5 Search with filters
- Dapper search within one city, by category, district, capacity, age, price and amenities, with PostGIS radius and distance ordering (from the parent's point, or the city center), and paging.
- Only publicly visible providers are searched (1.11). District names come back in the request language.

### 3.6 Full-text search
- A generated `tsvector` column with a diacritics-insensitive normalization function (`unaccent` and đ→dj; Cyrillic letters are transliterated to Latin first), a GIN index and `websearch_to_tsquery`.

### 3.7 Ranking, sponsored results, caching and search log
- Ranking by relevance, paid plan, profile completeness, recent activity and rating.
- Sponsored results flagged `IsSponsored`.
- Short-lived cached result pages, keyed by the request language too, and invalidated when a category opens or closes in a city.
- An asynchronous, anonymous search log: city, category, requested date and part of day, days ahead, districts, guest counts, child age, filters, interface language and result count, with zero-result searches flagged.
- Searches are logged only when a parent runs a search, not on every filter change or page.
- A random session ID that isn't linked to a user or inquiry counts repeated searches once. Whether to keep it, and how to store it, is decided during implementation.

### 3.8 Engagement events
- Anonymous events per provider: search impression (with position), profile view, slot click, inquiry started and inquiry sent.
- Recorded asynchronously, with bot traffic filtered out at request time. Used for provider statistics (P2.7) and provider suggestions (P3.7).

### 3.9 Natural-language search
- A parent describes the party in their own words, for example "rođendan za 15 dece od 6 godina, Liman, subota popodne, do 20.000 din". `POST /search/parse` turns the text into the filter set of 3.5. It returns filters only, never providers: the frontend shows them as editable chips and then runs the normal search, so ranking and visibility rules stay in one place.
- **3.9a** Parser: `IVenueQueryParser` in Application, an Anthropic implementation in Infrastructure (a small, fast model, set in configuration). The text is limited to 300 characters and the city comes from the page, not the text. The prompt gets the open city's district and amenity names, the request language and today's date in Belgrade (to resolve "subota" or "next Saturday"), and nothing else: no provider data and no database access.
- **3.9b** Validation: structured output (a JSON schema) mapped to the 3.5 filters and checked by the same validator. District and amenity names are matched to lookup data on the server; anything unmatched is dropped. The response also lists the parts of the text that weren't understood, so the frontend can show them.
- **3.9c** Guardrails: a short timeout. If the model fails, times out or returns invalid output, the endpoint returns an error and the frontend falls back to full-text search (3.6) with the same text. Rate limits per IP, and results cached for a day by normalized text, city, language and date.
- **3.9d** Observability and privacy: model, tokens, latency, estimated cost and outcome are logged to Seq. The raw text is never logged or stored, because parents may type names or phone numbers. The search log (3.7) records the parsed filters with the source `natural_language`.
- **3.9e** Evals: a hand-written set of 50 or more cases in Serbian (Latin and Cyrillic), English and Russian, each with the expected filters, including cases where the right answer is to leave a filter empty. A scorer reports accuracy per filter and per language. Evals run against the real model on demand and before any prompt or model change, not on every push; the results are committed so changes can be compared. Handler tests use a fake parser and run in CI as usual.

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
- Moved after the MVP with the service categories; see P2.11.

---

## Milestone 5 — Inquiries and bookings

### 5.1 Phone verification and anti-abuse
- SMS one-time codes (HMAC-hashed, with expiry and limited attempts) in the request language, and single-use verification tokens.
- Rate limits per IP and per phone number, a honeypot field, and an allow-list of country codes.

### 5.2 Price quote and booking terms
- Pure calculators for price (pricing models, minimum quantity, extras) and booking terms: deposit amount, deposit due date, free-cancellation and quantity-change deadlines.
- A public quote endpoint.

### 5.3 Create inquiry
- Anonymous inquiries to publicly visible providers with a verified phone, with capacity and age checks, a policy snapshot, a quoted price and the parent's language. Lead-time and daily-capacity checks come with the service categories (P2.11).

### 5.4 Inquiry tracking
- Private tracking links for parents (hashed tokens), opening in the parent's language.
- An inquiry list and detail for providers, with seen tracking and the parent's language.

### 5.5 Accept and decline
- Acceptance computes the terms and holds the slot in one transaction, protected against concurrent acceptance. Competing inquiries are declined automatically.

### 5.6 Booking payments
- Recording the deposit, the balance and refunds. The slot becomes Booked once the deposit is recorded.

### 5.7 Cancellation, expiry and completion
- Cancellation by the parent or the provider, refunded or forfeited depending on the deadline, with a manual refund override.
- Jobs that expire unpaid holds and complete past bookings.

### 5.8 Inquiry notifications and reminders
- Notifications on every status change, plus reminders before the deposit deadline, the cancellation deadline and the party, each in its recipient's language.

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
- PDF invoices generated with QuestPDF, in Serbian.

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
- **7.6** Lookup data management (districts per city, amenities, plans, promo codes), with names in three languages.
- **7.7** City launches: every city with its providers per category and status; opening or closing a category in a city (opening needs the city's locative names); published providers there are notified when their category opens.

---

## Milestone 8 — Phase 1 release

### 8.0 Staging deployment
- Set up before production, or earlier once someone else needs a running environment.
- A hardened VPS with Docker Compose: API, PostGIS, Seq and Caddy (automatic HTTPS).
- The CI publishes the API and migrator images to GHCR, then deploys over SSH and runs migrations as a separate step.
- Forwarded headers, persisted Data Protection keys, and R2 media storage (3.4).

### 8.1 Data retention
- Anonymize parent contact data on old inquiries.
- Delete individual search and engagement events after 18 months; only aggregates are kept.

### 8.2 Production environment
- A separate production server.
- Manual deployments of image tags already verified on staging.
- Nightly `pg_dump` to R2 with retention, and a documented restore procedure.
- Uptime, error, disk and backup-heartbeat alerts.

### 8.3 Pre-release review
- A security review, and rate limits on every anonymous endpoint.
- Email and SMS templates checked in all three languages.

---

## Phase 2

Phase 1 (Milestones 0–8) is the MVP. Everything below comes after it.

| Step | Content |
|---|---|
| P2.1 | Search by free slot in a time window |
| P2.2 | Party request: one parent request creates inquiries to several providers in one transaction (after P2.11) |
| P2.3 | Quantity change requests with the minimum-quantity floor and price recalculation |
| P2.4 | Verified reviews through a one-time link after a completed booking, admin moderation, public provider replies, aggregate rating |
| P2.5 | SMS and Viber Business notification channels |
| P2.6 | Embeddable provider calendar |
| P2.7 | Provider statistics |
| P2.8 | SEO data endpoints: city × category and district listings, and a sitemap with all three languages, for open city categories only |
| P2.9 | Social sign-in: Google (ID token) and Facebook (access token checked through the Graph API), with account linking, pre-account-takeover protection and Facebook's data deletion callback |
| P2.10 | Refresh token cleanup: a recurring Hangfire job that deletes refresh tokens that expired or were revoked more than a retention period ago (for example 30 days) |
| P2.11 | Service categories, catering and cakes: `ServiceDetails` (service-area districts, delivery, lead time, maximum orders per day), daily capacity (moved from 4.8), lead-time and daily-capacity checks on inquiries and acceptance, opened city by city like venues |
| P2.12 | City wishes: a parent can pick a city that isn't open yet; the anonymous count per city appears on the admin's launch screen (7.7) |
| P2.13 | Optional English and Russian versions of texts written by providers (description, package names, rules), falling back to Serbian |
| P2.14 | MCP server, so AI assistants can search venues for parents: a Streamable HTTP endpoint `/mcp` in the API (official C# MCP SDK) with read-only tools over public data only: `list_cities` (1.2 catalogs), `search_venues` (the 3.5 filters), `get_venue` (1.11) and `get_free_slots` (4.7), each taking a language. Tools are thin adapters in Presentation that call the existing queries, so the visibility rule and caching apply unchanged. No tool sends an inquiry: results link to the venue page, where the parent sends it with a verified phone. Anonymous, with rate limits per IP; searches are logged with the source `mcp`. Tool names, descriptions and parameter docs are written for a model to read. Integration tests call the tools through an MCP client against the test host, and the README shows how to connect Claude Desktop or Claude Code. Can be pulled forward any time after 4.7. |

## Phase 3

| Step | Content |
|---|---|
| P3.0 | Portion guidelines, and the animator, photographer and decoration categories, opened city by city |
| P3.1 | Deterministic planner: candidates per category, compatibility rules, quantities, budget-constrained top-3 combinations, alternatives |
| P3.2 | LLM-based party request parsing, extending the 3.9 parser and its eval set, and explanations over computed results only |
| P3.3 | Natural-language schedule parsing into availability rules, always previewed and confirmed |
| P3.4 | Party invitations and RSVPs, with order updates based on confirmed guests |
| P3.5 | Card payments: tokenization, recurring charges, idempotent webhooks, charge retries |
| P3.6 | Birthday reminders |
| P3.7 | Provider suggestions (paid plans only): a weekly job aggregates anonymous searches and chosen slots by district, weekday, part of day and start time, with no provider ID in the aggregate. Rule classes compare demand with each provider's own schedule and profile (new slots, full days, start times, booking horizon, age range, guest count, unmarked amenities and rules, profile views). Suggestions are template text about high demand, never prices, and only appear above a minimum-data threshold. A provider only sees them, there is no accept or dismiss. A unique `(provider, type, subject key)` index ensures the same suggestion is never created twice. Outcomes are measured when the provider's schedule changes to match a suggestion. |

---

## Dependencies

```
0.0 ─┬─> 0.1 / 0.2 tests ──> 0.4 CI ──> 8.0 staging (+ 3.4 R2) ──> 8.2 production
     ├─> 0.3 local environment
     ├─> 0.5 background jobs ──> M2 outbox and email ──> 2.7 invitations
     └─> 0.6 identity ──> 1.8 admin create
                      └─> 1.10 membership ──> 1.12 self-registration, 2.8 claim
0.9 languages ──> 1.2 cities and lookups ──> 1.11 public visibility ──> 3.5 search
                                         └─> 7.7 city launches
M1 providers ──> M3 media and search
3.5 search, 3.6 full-text ──> 3.9 natural-language search ──> P3.2 request parsing, P3.3 schedule parsing
1.2 catalogs, 1.11 provider page, 3.5 search, 4.7 free slots ──> P2.14 MCP server
3.7 search log, 3.8 engagement events ──> P2.7 statistics, P3.7 provider suggestions
M1 providers ──> M4 availability ──> M5 inquiries ──> 5.8 notifications
6.1 subscriptions ─┬─> 6.2 feature access ──> 3.7 ranking with real plan data
                   └─> 6.6 / 6.8 billing jobs
P2.11 service categories ──> P2.2 party request, P3.1 planner
```
