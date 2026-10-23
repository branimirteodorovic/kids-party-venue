# Frontend implementation plan

**Project:** KidsPartyVenue
**Status:** draft · **Last updated:** 2026-10-22

Related documents:

- [Backend documentation](../../Backend/docs/): backend architecture, domain model, API and delivery
  order (referred to below as "the backend plan")

---

## Contents

1. [Purpose and scope](#1-purpose-and-scope)
2. [Product surfaces and rendering](#2-product-surfaces-and-rendering)
3. [Technology stack](#3-technology-stack)
4. [Architecture](#4-architecture)
5. [Routing](#5-routing)
6. [Design system](#6-design-system)
7. [Cross-cutting concerns](#7-cross-cutting-concerns)
8. [API contract](#8-api-contract)
9. [Environments, build and deployment](#9-environments-build-and-deployment)
10. [Testing strategy](#10-testing-strategy)
11. [Delivery milestones](#11-delivery-milestones)
12. [Open decisions](#12-open-decisions)
13. [Definition of done](#13-definition-of-done)
14. [Later phases](#14-later-phases)

---

## 1. Purpose and scope

This document describes how the KidsPartyVenue web frontend is structured and delivered: the
architecture, the conventions every screen follows, the contract with the .NET API, and the
milestones with their acceptance criteria.

**In scope:** phase 1 (MVP) for all three audiences (parents, providers, admin), as described in
[section 11](#11-delivery-milestones), plus the technical groundwork for phases 2 and 3
([section 14](#14-later-phases)).

**Out of scope:** the backend (see the [backend plan](../../Backend/docs/implementation-plan.md)), native mobile apps,
the embeddable provider calendar (phase 2) and the planning assistant (phase 3).

**Market:** Serbia, city by city. Every city exists in the catalog, but parents only see the cities
and categories the admin has opened. The launch is Novi Sad with party venues only; catering and
cakes, and further cities, follow (see [section 14](#14-later-phases)).

**Languages:** Serbian (Latin script, default), English and Russian, with formal address ("Vi",
"вы"). The public site and the provider panel are translated; the admin panel is Serbian only.
Texts written by providers are shown as written. The brand is always written "Slavljenik.rs" and
is not declined or translated.

---

## 2. Product surfaces and rendering

One Angular application with three lazily loaded areas:

| Area           | Users                              | URL prefix                               | Rendering                   | Primary device |
| -------------- | ---------------------------------- | ---------------------------------------- | --------------------------- | -------------- |
| Public site    | Parents, search engines            | `/`, `/rodjendaonice/{city}`, `/upit`, … | Server-side rendering (SSR) | Phone          |
| Provider panel | Venue owners and service providers | `/panel`                                 | Client-side rendering (CSR) | Phone          |
| Admin panel    | Portal owner                       | `/admin`                                 | Client-side rendering (CSR) | Desktop        |

Rendering is configured per route in `app.routes.server.ts`. During early development every route
is client-rendered; public routes switch to `RenderMode.Server` in milestone M7, once the pages and
their data access are stable. Authenticated areas stay client-rendered: they have no SEO value and
are always behind a session.

Each language is a separate build of the same application, served under its own prefix: Serbian at
`/`, English at `/en/` and Russian at `/ru/`. Paths and query parameters are identical in all three,
so switching language only changes the prefix.

---

## 3. Technology stack

| Concern                | Choice                                                                    | Notes                                                        |
| ---------------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Framework              | Angular (current major), standalone components, signals, zoneless         | No NgModules                                                 |
| SSR                    | `@angular/ssr` with route-level render modes, hydration with event replay | Node server behind Caddy                                     |
| Localisation           | `@angular/localize` (built-in i18n), one build per language               | `sr-Latn` (source), `en`, `ru`; JSON translation files       |
| Styling                | Tailwind CSS v4 via PostCSS, design tokens in `@theme`                    | Custom breakpoints `tablet:` / `desktop:`                    |
| UI components          | In-house component kit on top of Angular CDK                              | No third-party component library; the design is fully custom |
| Icons                  | Lucide (`@lucide/angular`), wrapped by `app-icon`                         | Icons registered once in `core/icons.ts`                     |
| Maps                   | Leaflet, loaded on demand in the browser only                             | Tile provider configurable (see D12)                         |
| Forms                  | Angular reactive forms, strictly typed                                    |                                                              |
| State                  | Signals in services; RxJS for request orchestration                       | No global store library                                      |
| HTTP                   | `HttpClient` with `withFetch()`, functional interceptors                  |                                                              |
| Formatting             | `Intl` APIs with the build's locale and a fixed time zone                 | See [7.2](#72-dates-times-and-money)                         |
| Unit tests             | Vitest (Angular CLI builder)                                              |                                                              |
| End-to-end tests       | Playwright                                                                | Smoke suites per audience                                    |
| Linting and formatting | angular-eslint, Prettier with `prettier-plugin-tailwindcss`               | Enforced in CI                                               |

---

## 4. Architecture

### 4.1 Workspace layout

The Angular workspace lives in `Frontend/kids-party-venue/`. Inside `src/app/`:

```
core/                 application-wide services and infrastructure
  api/                API clients (one per backend module) and DTO models
  auth/               session, interceptors, guards
  errors/             error interceptor, ApiError, error copy table
  i18n/               supported languages, language switcher, Accept-Language interceptor
  layout/             public, panel and admin shells; title strategy
  seo/                meta tags, canonical links, structured data
  icons.ts            icon registration
shared/
  ui/                 design-system components (no business logic)
  util/               pure functions and pipes (formatting, dates, phone numbers, parsing)
  domain/             business types and small components shared by more than one area
features/
  public/             parent-facing pages (home, search, provider-profile, inquiry, tracking, my-parties, for-providers)
    shared/           components used by several public pages (provider card, phone verification)
  panel/              provider panel pages and panel.routes.ts
  admin/              admin pages and admin.routes.ts
  auth/               sign-in, invitation claim, password reset
  dev/                styleguide (development builds only)
```

Translation files live in `src/locale/` (`messages.json` extracted from the source,
`messages.en.json`, `messages.ru.json`).

### 4.2 Dependency rules

- `features/public`, `features/panel` and `features/admin` never import from each other. Code
  needed by more than one area moves to `shared/` or `core/`.
- `shared/ui` has no knowledge of the API or the domain. `shared/domain` may use `shared/ui`.
- Only `core/api` talks to `HttpClient`. Pages and components use the API services.
- Only `shared/util/format.ts` (and the pipes built on it) formats money, dates, times and plurals.

These rules are enforced in code review; lint-level enforcement (import boundaries) is added once the
folder structure has settled.

### 4.3 State management

- Component state and derived state: `signal`, `computed`, `model`.
- Shared state (catalogs with the open cities, selected city, session, current provider,
  favourites, inquiry draft, toasts): injectable services exposing read-only signals.
- **The URL is the source of truth** for search criteria, filters, sorting, selected slot and list
  tabs. Pages read them through component input binding and change them by navigating
  (`queryParamsHandling: 'merge'`). Back/forward and shared links therefore always restore the view.
- Server data is loaded with an RxJS pipeline per page: `toObservable(criteria)` →
  `switchMap(request)` → `toSignal(state)`, where the state is a discriminated union
  (`loading` | `loaded` | `error`). Superseded requests are cancelled.

### 4.4 API layer

- One API service per backend module (`CatalogsApi`, `SearchApi`, `ProvidersApi`, `InquiriesApi`,
  `TrackingApi`, `AuthApi`, `PanelInquiriesApi`, `AvailabilityApi`, `ProfileApi`, `BillingApi`,
  `AdminApi`).
- Each service is an **abstract class** used as the DI token, with an HTTP implementation and an
  in-memory fake. The fake allows UI work to proceed ahead of the corresponding endpoint and backs
  component tests. The binding is chosen in `app.config.ts`.
- DTOs are TypeScript interfaces in `core/api/models/`, named after the backend contracts. Enums
  are string unions. No `any`.
- Translation between Serbian URL values and API values (e.g. `vreme=popodne` →
  `partOfDay=Afternoon`) happens in the API service, in one place, with unit tests.
- An interceptor sends the page's language as `Accept-Language` (`sr`, `en` or `ru`) on every API
  request, so lookup names (cities, districts, amenities) arrive translated.
- All requests use relative `/api/...` URLs. On the server, an `API_BASE_URL` injection token points
  requests at the API over the internal network; the HTTP transfer cache prevents the browser from
  repeating requests made during SSR.

### 4.5 Error handling

- A functional error interceptor converts `HttpErrorResponse` into an `ApiError`
  (`status`, `code`, `fieldErrors`). `code` is the ProblemDetails `title` (e.g.
  `Slot.AlreadyBooked`).
- `ERROR_COPY` maps error codes to user-facing messages (translated like all other copy), with
  fallbacks per HTTP status.
- Field-level validation errors (`errors` in ProblemDetails) are applied to the matching form
  controls.
- Feedback channels: inline next to the field, inline in the form or page, or a toast. Nothing fails
  silently.

### 4.6 Authentication

- Access token (short-lived JWT) is held **in memory only**. The refresh token is an HttpOnly,
  Secure, SameSite=Strict cookie set by the API.
- On application start (browser only), `provideAppInitializer` attempts a refresh and loads the
  current user.
- An auth interceptor attaches the bearer token to `/api/` requests (excluding auth endpoints).
- On `401`, a single shared refresh is performed and the original request retried once; concurrent
  failures wait for the same refresh. A failed refresh ends the session and redirects to sign-in with
  a return URL.
- Route protection: `authGuard` for `/panel`, `roleGuard('Admin')` for `/admin`. Guards are for
  navigation only; the API authorises every request.
- Parents never have an account in the MVP. Inquiry tracking uses an unguessable link; "Moje
  proslave" uses phone verification (see D5).

---

## 5. Routing

URLs are Serbian and stable (they are part of SEO). Code uses English names, mapped with input
aliases. The same paths are used in every language; English and Russian pages add a `/en` or `/ru`
prefix (`/ru/rodjendaonice/novi-sad`).

### 5.1 Public

| Path                                  | Page                                                 | Render mode | Indexed |
| ------------------------------------- | ---------------------------------------------------- | ----------- | ------- |
| `/`                                   | Home with search box                                 | Server      | Yes     |
| `/{category}`                         | Open cities for the category (redirects if only one) | Server      | Yes     |
| `/{category}/{city}`                  | Search results / city landing page                   | Server      | Yes     |
| `/{category}/{city}/kvart/{district}` | Search results / district landing page               | Server      | Yes     |
| `/{category}/{city}/{slug}`           | Provider profile                                     | Server      | Yes     |
| `/{category}/{city}/{slug}/upit`      | Inquiry (three steps)                                | Server      | No      |
| `/upit/{token}`                       | Inquiry tracking                                     | Server      | No      |
| `/moje-proslave`                      | All inquiries for a verified phone                   | Server      | No      |
| `/za-ponudjace`                       | Information and registration for providers           | Server      | Yes     |
| `/uslovi`, `/privatnost`              | Terms, privacy policy                                | Server      | Yes     |
| `**`                                  | Not found                                            | Server      | No      |

Category routes are generated by one helper; the category is passed as route data. The MVP has
one category route, `/rodjendaonice`; `/ketering` and `/torte` are added with the service
categories. A city or category that isn't open, or a district that isn't in the city, renders the
not-found page. A profile reached through an outdated city or category redirects to its canonical
URL.

### 5.2 Authentication

| Path                                            | Page                        | Render mode |
| ----------------------------------------------- | --------------------------- | ----------- |
| `/prijava`                                      | Sign-in                     | Client      |
| `/preuzmi-profil?token=`                        | Claim a provider invitation | Client      |
| `/zaboravljena-lozinka`, `/nova-lozinka?token=` | Password reset              | Client      |

### 5.3 Provider panel (`/panel`, client-rendered, authenticated)

`''` dashboard · `upiti` inquiries · `upiti/{inquiryId}` inquiry detail · `kalendar` calendar ·
`kalendar/podesavanje` first setup · `profil` profile editor / wizard · `pretplata` subscription and
invoices · `vise` more (settings, language, provider switcher, sign out).

### 5.4 Admin (`/admin`, client-rendered, role `Admin`)

`''` dashboard · `provera` review queue · `provera/{providerId}` profile review · `ponudjaci`
providers and invitations · `gradovi` cities and launches · `pretplate` subscriptions, invoices
and payments · `sifarnici` catalogs · `istorija` audit log.

### 5.5 Query parameters

| Parameter    | Meaning                 | Values                              |
| ------------ | ----------------------- | ----------------------------------- |
| `datum`      | Party date              | `YYYY-MM-DD`                        |
| `vreme`      | Part of day             | `prepodne`, `popodne`, `uvece`      |
| `deca`       | Number of children      | integer                             |
| `odrasli`    | Number of adults        | integer                             |
| `kvart`      | Districts of the city   | comma-separated slugs               |
| `pogodnosti` | Amenities               | comma-separated slugs               |
| `uzrast`     | Child's age             | integer                             |
| `cena`       | Maximum price per child | integer (RSD)                       |
| `budzet`     | Total budget            | integer (RSD)                       |
| `sort`       | Sort order              | `preporuceno`, `cena`, `udaljenost` |
| `termin`     | Selected slot           | slot id                             |

Page titles are set by a custom `TitleStrategy` ("{page} | Slavljenik.rs"). Scroll position is reset
on navigation and restored on back/forward.

---

## 6. Design system

### 6.1 Tokens

All visual values are design tokens declared in `src/styles/tokens.css` (`@theme`). Templates use
token utilities only; raw hex values and the default Tailwind palette are not used.

| Group    | Tokens                                                                                                                 |
| -------- | ---------------------------------------------------------------------------------------------------------------------- |
| Surfaces | `page`, `surface`, `surface-muted`, `surface-sunken`, `ink` (dark surfaces)                                            |
| Borders  | `border`, `border-strong`, `border-input` (3:1 contrast for controls)                                                  |
| Text     | `text`, `text-secondary`, `text-muted`, `text-disabled`, `disabled-fg`                                                 |
| Brand    | `primary-50…900` (base 600), `accent-50…800` (highlights only, never on buttons)                                       |
| Feedback | `success`, `warning`, `danger`, `info` (each `-fg`, `-bg`, `-border`)                                                  |
| Booking  | `paid`, `completed`, `refunded` (each `-fg`, `-bg`, `-border`)                                                         |
| Type     | `display`, `h1`, `h2` (each with a desktop `-lg` size), `h3`, `body-lg`, `body`, `label`, `button`, `price`, `caption` |
| Shape    | radii `sm` 8, `md` 12, `lg` 16, `xl` 24 px; shadows `raised`, `sticky`, `window`                                       |
| Layout   | breakpoints `tablet` 600 px, `desktop` 1024 px; content width `page` 1200 px                                           |

Fonts: Nunito 700/800 for headings and prices (Latin and Cyrillic), Atkinson Hyperlegible Next
400/600/700 for body text and controls, both with full Serbian Latin glyph coverage. Atkinson
Hyperlegible Next has no Cyrillic, so Russian pages switch the body font through `:lang(ru)` (D14).

### 6.2 Layout

- Mobile first: unprefixed utilities describe the phone layout (< 600 px); `tablet:` and
  `desktop:` extend it.
- Gutters 16 / 24 / 32 px; content max width 1200 px.
- Public site: sticky header, footer on `ink`. Provider panel: bottom tab bar below desktop, 264 px
  sidebar on desktop. Admin: sidebar on desktop, horizontal navigation below.

### 6.3 Component inventory

| Component                               | Purpose                                                             | Milestone |
| --------------------------------------- | ------------------------------------------------------------------- | --------- |
| `app-logo`                              | Brand mark and wordmark                                             | M0        |
| `app-icon`                              | Icon wrapper; decorative by default, labelled when standalone       | M0        |
| `button[appButton]`, `a[appButtonLink]` | Buttons and button-styled links; 5 variants, 2 sizes, loading       | M0 / M1   |
| `app-badge`                             | Pill with tone, optional icon                                       | M0        |
| `app-inquiry-status-badge`              | Inquiry status for provider or parent wording                       | M0        |
| `app-provider-card`                     | Search result card with next free slots                             | M0 / M3   |
| `app-empty-state`                       | Empty and no-result states                                          | M0        |
| `app-language-switcher`                 | Links to the same page in the other languages                       | M1        |
| City picker                             | Chooses among open cities; hidden while only one is open            | M2        |
| `app-stepper`                           | Numeric stepper; later a form control                               | M2 / M12  |
| `app-month-calendar`                    | Month grid, Monday first, past days disabled                        | M2        |
| `app-sheet`                             | Native `<dialog>`: bottom sheet on phones, centered from tablet     | M2        |
| `app-field`, `input[appInput]`          | Label, hint, error and accessible wiring for form controls          | M5        |
| OTP input                               | One-time code entry with SMS autofill support                       | M5        |
| `app-toaster`                           | Polite live-region notifications with optional action               | M5        |
| Results map                             | Price pins synchronised with the result list                        | M7        |
| Slot grid                               | Calendar slot states (free, pending, held, booked, blocked, closed) | M10 / M11 |
| Photo manager                           | Upload with progress, reorder, delete, moderation state             | M12       |
| Launch matrix                           | Cities × categories with provider counts and open/close actions     | M14       |

A development-only `/styleguide` route renders every token and component in all states.

### 6.4 Status presentation

Inquiry statuses (`New`, `Seen`, `AwaitingDeposit`, `DepositPaid`, `PaidInFull`, `Completed`,
`CancelledRefunded`, `CancelledForfeited`, `Declined`, `Expired`, `Archived`) are described in one
exhaustive table (`INQUIRY_STATUS_META`): provider label, parent label, tone and icon. Every status
is shown as colour + icon + text, accompanied by a sentence stating the next step and its deadline.

---

## 7. Cross-cutting concerns

### 7.1 Copy and localisation

- Serbian (Latin script) is the source language; English and Russian are translations. Formal
  address in all three. Each build sets `<html lang>` (`sr-Latn`, `en`, `ru`).
- Angular's built-in i18n: every user-facing string in the public site and the provider panel is
  marked (`i18n` in templates, `$localize` in code) with a stable custom ID (`@@area.page.item`),
  extracted to `src/locale/messages.json` and translated in `messages.en.json` and
  `messages.ru.json`. Sentences are never assembled from translated fragments; placeholders and ICU
  plural messages are used instead (Russian has four plural forms).
- The admin panel is not marked and stays Serbian in every build.
- Lookup names (cities, districts, amenities) come translated from the API. Texts written by
  providers are displayed as written, wrapped in `lang="sr-Latn"` on English and Russian pages.
- Language names in the switcher are written in their own language ("Srpski", "English",
  "Русский") and carry `lang` and `hreflang`.
- User-facing error messages live in a single table keyed by API error code.
- Development builds tolerate missing translations (they fall back to Serbian with a warning);
  the production build treats a missing translation as an error from launch on.

### 7.2 Dates, times and money

- Calendar dates from the API (`DateOnly`, `YYYY-MM-DD`) are handled as strings and, where a `Date`
  is needed, created and formatted in UTC. They are never converted through the local time zone.
- Instants (`DateTimeOffset`) are displayed in `Europe/Belgrade`, whatever the language.
- "Today" is computed in `Europe/Belgrade`, never from the device's local date.
- Formatting helpers take the locale as a parameter; pipes pass the build's `LOCALE_ID`.
- Money is whole RSD: `30.000 din` (Serbian), `30,000 RSD` (English), `30 000 дин.` (Russian).
- Plurals use ICU messages in templates and `Intl.PluralRules` in code (Serbian one / few / other,
  Russian one / few / many / other, English one / other). Deadline phrases that need the genitive
  ("do petka, 23. oktobra", "до пятницы, 23 октября") use dedicated helpers per language.
- Server and browser produce identical output (a prerequisite for hydration without mismatches).

### 7.3 Accessibility

Target: **WCAG 2.2 AA**.

- Semantic HTML first; native `<button>`, `<a>`, `<dialog>`, form controls.
- Every control has a visible label; errors are linked with `aria-describedby` and announced.
- Touch targets ≥ 48 px (56 px for primary actions on phones).
- Visible focus (3 px primary ring, 2 px offset) on every interactive element; skip link on public
  pages; `aria-current` in navigation.
- Status and state never conveyed by colour alone. Colour pairs meet 4.5:1 for text, 3:1 for
  controls.
- `prefers-reduced-motion` respected.
- Keyboard-only and screen-reader passes before launch (M15).

### 7.4 Performance

- Budgets (mobile, Lighthouse): Performance ≥ 90 on home, search and profile.
- Lazy-loaded areas and pages; `@defer` for below-the-fold and heavy blocks (map, gallery viewer).
- `NgOptimizedImage` with a loader for the backend's 400/800/1600 px WebP variants; the LCP image is
  prioritised; all images have intrinsic dimensions.
- Fonts inlined at build time; bundle size budgets in `angular.json`.

### 7.5 SEO

- Server-rendered public pages with unique titles and descriptions, canonical URLs and Open Graph
  tags.
- Structured data (JSON-LD): `LocalBusiness` and `Offer` on profiles; `AggregateRating` once reviews
  exist (phase 2).
- `noindex` on tracking, inquiry, "Moje proslave" and authentication pages.
- Every indexed page links its three language versions with `hreflang` (`sr-Latn`, `en`, `ru`, and
  `x-default` pointing at Serbian); each version's canonical URL is its own.
- Only open cities and categories are linked or listed in the sitemap.
- `sitemap.xml` and `robots.txt` are generated by the API and routed by the reverse proxy.

### 7.6 Privacy and analytics

- Anonymous engagement events (`Impression`, `ProfileView`, `SlotClick`, `InquiryStarted`,
  `InquirySent`) are sent with `navigator.sendBeacon`, from the browser only, batched, and never
  contain names, phone numbers, e-mail addresses or free text.
- The anonymous session identifier is configurable and can be disabled (D13).
- Personal data entered by parents is kept only for the duration of the flow (`sessionStorage`) and
  cleared after submission.
- Tracking links and invitation tokens are treated as secrets: not logged client-side, not sent to
  analytics, and pages using them are `noindex`.

### 7.7 Security

- Access token in memory only; refresh token in an HttpOnly cookie (not readable by scripts).
- Same-origin API (`/api`), so no CORS configuration is required and cookies stay first-party.
- Angular's built-in template sanitisation; no `innerHTML` with API content.
- Honeypot field and idempotency key on inquiry submission; rate-limit responses (`429`) handled
  with clear messages.
- Content Security Policy configured at the reverse proxy before launch.

---

## 8. API contract

### 8.1 Conventions

| Topic           | Convention                                                                                      |
| --------------- | ----------------------------------------------------------------------------------------------- |
| Base path       | `/api`                                                                                          |
| JSON            | camelCase                                                                                       |
| Enums           | Strings, PascalCase (`"AwaitingDeposit"`)                                                       |
| Dates           | `DateOnly` → `"2026-11-14"`; `TimeOnly` → `"17:00:00"`; instants → ISO 8601 with offset         |
| Money           | Decimal RSD                                                                                     |
| Paging          | `{ items, page, pageSize, totalCount }`, `page` starting at 1                                   |
| Errors          | RFC 9457 ProblemDetails; `title` is a stable error code; validation errors in `errors` by field |
| Idempotency     | `Idempotency-Key` header on `POST /api/inquiries`                                               |
| Arrays in query | Repeated parameters (`districts=liman&districts=grbavica`)                                      |
| Language        | `Accept-Language: sr` \| `en` \| `ru` on every request; lookup names are returned in it         |
| Auth            | Bearer access token; refresh token in an HttpOnly cookie                                        |

The OpenAPI document (Scalar in development) is the source of truth. The list below is the
frontend's expected surface; it is reconciled with the API as each milestone starts.

### 8.2 Endpoints by area

**Public (anonymous)**

| Method and path                                                               | Purpose                                                    |
| ----------------------------------------------------------------------------- | ---------------------------------------------------------- |
| `GET /api/catalogs`                                                           | Open cities with their categories and districts, amenities |
| `GET /api/providers/search`                                                   | Search in one city with filters, sorting and paging        |
| `GET /api/providers/{slug}`                                                   | Provider profile                                           |
| `GET /api/providers/{slug}/slots?from=&to=`                                   | Free slots                                                 |
| `GET /api/providers/{slug}/quote`                                             | Price, deposit and deadlines for a selection (D2)          |
| `POST /api/phone-verifications`, `POST /api/phone-verifications/{id}/confirm` | Phone verification (OTP)                                   |
| `POST /api/inquiries`                                                         | Create an inquiry; returns the tracking token              |
| `GET /api/track/{token}`                                                      | Inquiry tracking                                           |
| `POST /api/track/{token}/cancel`                                              | Cancel by the parent                                       |
| `GET/POST /api/track/{token}/messages`                                        | Messages                                                   |
| `POST /api/track/{token}/payment-notice`                                      | Parent reports a deposit payment (D3)                      |
| `POST /api/my-parties/session`, `GET /api/my-parties`                         | "Moje proslave" (D5)                                       |
| `POST /api/events`                                                            | Anonymous engagement events                                |

**Authentication**

| Method and path                                                           | Purpose                    |
| ------------------------------------------------------------------------- | -------------------------- |
| `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout` | Session                    |
| `GET /api/users/me`                                                       | Current user               |
| `PUT /api/users/me/language`                                              | Preferred language         |
| `GET /api/cities`                                                         | All cities (registration)  |
| `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`         | Password reset             |
| `POST /api/auth/register-provider`                                        | Provider self-registration |
| `GET /api/claim/{token}`, `POST /api/claim`                               | Claim an invited profile   |

**Provider panel** (`/api/provider/{providerId}/…`, see D11)

| Method and path                                                                              | Purpose                        |
| -------------------------------------------------------------------------------------------- | ------------------------------ |
| `GET /api/provider/memberships`                                                              | Providers the user manages     |
| `GET …/dashboard`                                                                            | Dashboard                      |
| `GET …/inquiries`, `GET …/inquiries/{id}`                                                    | Inquiries                      |
| `GET …/inquiries/{id}/accept-preview`, `POST …/accept`, `POST …/decline`                     | Respond to an inquiry          |
| `POST …/inquiries/{id}/payments`, `POST …/cancel`, `POST …/extend-deposit` (D4)              | Payments and cancellation      |
| `GET/POST …/inquiries/{id}/messages`                                                         | Messages                       |
| `GET/PUT …/availability/rules`, `POST …/availability/preview`, `POST …/availability/initial` | Calendar rules and first setup |
| `GET …/slots`, `POST …/slots/quick-block`, `POST …/slots/{id}/unblock`                       | Everyday calendar              |
| `GET/POST/DELETE …/blocked-periods`                                                          | Holidays and closures          |
| `GET …/profile`, `PUT …/profile/{section}`, `POST …/submit-for-review`                       | Profile                        |
| `POST/PUT/DELETE …/media`                                                                    | Photos                         |
| `GET/POST/PUT/DELETE …/offerings`                                                            | Packages                       |
| `GET …/subscription`, `GET …/invoices`, `GET …/invoices/{id}/pdf`, `GET …/invoices/{id}/qr`  | Billing                        |

**Admin** (`/api/admin/…`)

`GET dashboard` · `GET review-queue` · `POST providers/{id}/approve` ·
`POST providers/{id}/request-changes` · `GET/POST providers` · `POST providers/{id}/invitations` ·
`GET subscriptions` · `POST invoices/{id}/payments` · catalog CRUD (`districts` per city,
`amenities`) · `GET cities`, `PUT cities/{id}`, `POST cities/{id}/categories/{category}/open` and
`…/close` · `GET audit-log`.

### 8.3 Development support expected from the API

- Fixed verification code in the Development environment.
- Links that would be e-mailed or sent by SMS (tracking, invitations, password reset) written to the
  log.
- Seeded admin account and seed data: all cities (hidden), venues opened in Novi Sad, the Novi Sad
  districts, amenities in three languages, and a representative set of venues with photos,
  packages, rules and generated slots, plus one published venue in a city that isn't open yet.

---

## 9. Environments, build and deployment

| Environment | Frontend                        | API access                                                                              |
| ----------- | ------------------------------- | --------------------------------------------------------------------------------------- |
| Local       | `ng serve` on port 4200         | Dev-server proxy: `/api` → local API (`proxy.conf.json`)                                |
| Staging     | SSR Node container behind Caddy | Caddy routes `/api/*` to the API container; SSR calls the API over the internal network |
| Production  | Same as staging                 | Same as staging                                                                         |

- **Build:** `npm run build` produces `dist/kids-party-venue/browser` (static assets, one folder
  per language) and `dist/kids-party-venue/server` (one SSR server for all languages). The dev
  server runs one language at a time (`npm start` for Serbian, `start:en`, `start:ru`).
- **Container:** multi-stage Dockerfile (`npm ci && npm run build`, then a slim Node runtime running
  `server/server.mjs`). Configuration (internal API URL, map tile URL) comes from environment
  variables.
- **Reverse proxy (Caddy):** `/api/*`, `/sitemap.xml`, `/robots.txt` → API; everything else → SSR
  server; automatic HTTPS; security headers.
- **CI (GitHub Actions):** install, lint, format check, unit tests, build, container image; e2e smoke
  tests against staging after deployment.

---

## 10. Testing strategy

| Level              | Tooling                                       | Covers                                                                                                                                                  |
| ------------------ | --------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unit               | Vitest                                        | Every function in `shared/util` (formatting in all three locales, dates, phone normalisation, parsing, time ranges), URL ↔ API mapping, status tables   |
| Service            | Vitest + `HttpTestingController`              | API services, error interceptor, auth interceptor and refresh logic                                                                                     |
| Component          | Vitest + Angular testing utilities            | Form validation and submission states, components with non-trivial logic                                                                                |
| End-to-end (smoke) | Playwright                                    | Parent: search → profile → inquiry → tracking (Serbian and Russian). Provider: sign in → accept → record deposit. Admin: approve a profile, open a city |
| Accessibility      | axe, manual keyboard and screen-reader passes | All page types before launch                                                                                                                            |
| Performance        | Lighthouse (mobile)                           | Home, search, profile                                                                                                                                   |

Rules with many edge cases (dates across month and year boundaries, Serbian plurals, phone formats,
slot overlaps) are written test-first.

---

## 11. Delivery milestones

Milestones are sequential. Each lists its scope, the backend capabilities it depends on, and
acceptance criteria. Relative size: S (small), M (medium), L (large).

### Phase 1 (MVP)

#### M0 — Workspace and design system foundation · M

Scope: Angular workspace with SSR, Tailwind and design tokens, linting, formatting and unit tests;
logo, icon, button, badge and status badge, provider card, empty state; formatting utilities and
pipes; development styleguide.
Backend: none.
Acceptance:

- Lint, format check, tests and production build pass in CI.
- The styleguide shows all tokens and components at phone, tablet and desktop widths.
- Formatting utilities are covered by tests (money, long and short dates, time ranges, plurals).

#### M1 — Application shell, routing and languages · M

Scope: public, panel and admin layouts; all routes from section 5 with placeholder pages; route
input binding; title strategy; scroll restoration; not-found page; the three language builds with
the translation workflow, locale-aware formatting and the language switcher.
Backend: none.
Acceptance:

- Every route resolves to its page; the three layouts render correctly at all breakpoints.
- Route parameters, query parameters and route data reach pages as typed inputs.
- Skip link, `aria-current` and keyboard navigation work in all layouts.
- Every page exists under `/`, `/en/` and `/ru/` with `<html lang>` set; the switcher keeps the
  current path and query; formatting tests pass in all three locales.

#### M2 — Home page and search box · M

Scope: home page; search box (city when more than one is open, category when more than one is
open in the city, date, part of day, guests, districts of the city); stepper, month calendar and
sheet components; date utilities.
Backend: none (fake catalog).
Acceptance:

- A complete search can be composed on a phone and produces the documented URL and query string.
- With one open city and one open category, neither picker is shown.
- The calendar never offers past dates, using the Belgrade calendar day.
- The search box is fully operable by keyboard.

#### M3 — Search results · M

Scope: API layer foundations (proxy, models, abstract/HTTP/fake services, a request pipeline
with cancellation, the `Accept-Language` interceptor); the catalog of open cities; results for one
city with next free slots; summary bar; filters; sorting; paging; city handling (choice, redirect,
not open); loading, empty and error states.
Backend: catalogs, provider search (D1, D8, D9).
Acceptance:

- Results match the criteria in the URL; filters, sorting and back/forward behave consistently.
- Cities and categories that aren't open can't be reached; their URLs show the not-found page.
- Superseded requests are cancelled.
- Loading, empty and error states match the design.

#### M4 — Provider profile · L

Scope: profile sections; optimised gallery; free slots by date; packages with a backend-calculated
total; rules and deadlines; booking bar; map; canonical redirect; provider-written texts marked as
Serbian on English and Russian pages; favourites.
Backend: profile, slots, quote (D2).
Acceptance:

- All amounts and deadlines shown come from the API.
- Selected slot, package and guests survive refresh and are passed to the inquiry.
- The LCP image is prioritised and served at the right size.

#### M5 — Inquiry submission · L

Scope: inquiry draft; review step; contact form with phone validation; conditions and consent;
phone verification; submission with idempotency; global error handling and toasts.
Backend: phone verification, inquiry creation (D6, D16).
Acceptance:

- An inquiry can be sent end to end; duplicates are impossible on retry or double submission.
- Conflict, rate-limit, validation and network errors each have a clear, specific message.
- The form meets the accessibility requirements in 7.3.

#### M6 — Inquiry tracking and "Moje proslave" · M

Scope: tracking page (status, next step, payment details, deadline countdown, timeline, messages,
cancellation with consequences shown first); "Moje proslave".
Backend: tracking endpoints (D3, D5, D7).
Acceptance:

- Status changes made by the provider appear without a reload.
- Cancellation shows the refund outcome before confirmation and the result after.

#### M7 — SSR, SEO and performance · M

Scope: server rendering for public routes in all three languages; browser-only code isolated;
internal API access and transfer cache; meta tags, canonical URLs, `hreflang` alternates, JSON-LD,
`noindex`; a language suggestion for visitors whose browser prefers English or Russian; results
map; deferred loading; engagement events.
Backend: as M3–M6, plus events (D12, D13).
Acceptance:

- Public pages deliver complete HTML in every language; no hydration errors.
- Lighthouse mobile Performance ≥ 90 on home, search and profile.
- Private pages carry `noindex`; events contain no personal data.

#### M8 — Provider authentication · M

Scope: sign-in, session restore, interceptors, guards, silent refresh, invitation claim, password
reset, provider switcher.
Backend: auth, claim, memberships (D10, D11).
Acceptance:

- Sessions survive reloads and access-token expiry; concurrent `401`s trigger one refresh.
- Protected areas redirect correctly, including the return URL.

#### M9 — Provider dashboard and inquiries · L

Scope: urgency-ordered dashboard; inquiry list with tabs and search; inquiry detail with
status-driven actions (accept with preview, decline, record payment, cancel, extend deadline) and
the parent's language; messages; undo; new-inquiry indicator.
Backend: panel inquiry endpoints (D4, D7).
Acceptance:

- Every status exposes exactly the actions in the status/action table.
- An inquiry can be taken through its full lifecycle on a phone.

#### M10 — Calendar first setup · L

Scope: rule templates and editor; "booked until a date, except…"; live preview; save; draft
autosave.
Backend: availability rules, preview, initial setup.
Acceptance:

- The reference schedule ("until 30 November only Tuesday 17–19, Friday 19–21 and Saturday 15–19;
  from 1 December every day 17–19 and 19–21") can be entered in under five minutes on a phone.
- Overlapping or invalid ranges are reported on the exact rows.
- The preview matches what parents subsequently see.

#### M11 — Calendar everyday use · M

Scope: day and week views with all slot states; slot actions; multi-slot quick block; blocked
periods; rule editing with conflict warnings.
Backend: slots, quick block, blocked periods.
Acceptance: a manual booking or a holiday can be recorded in under a minute.

#### M12 — Profile editor and registration wizard · L

Scope: all profile sections (city from the full list, district when the city has districts);
packages; booking policy with a plain-language preview; photo upload, ordering and deletion;
wizard for drafts; review states, including "published, waiting for the city to open";
settings mode with unsaved-changes guard; self-registration of venues from any city.
Backend: profile, media, offerings, review submission, registration.
Acceptance:

- A provider can complete the wizard on a phone and act on review notes.
- A provider in a city that isn't open sees clearly that parents can't see the profile yet.
- Unsaved changes are never lost silently.

#### M13 — Subscription and invoices · S

Scope: plan and status; invoices; payment details with IPS QR code; PDF download (invoices are
Serbian in every language).
Backend: billing.
Acceptance: a provider in `PastDue` sees what to pay, how and by when.

#### M14 — Admin panel · L

Scope: dashboard; review queue and profile review; providers and invitations (with the
invitation's language); cities and launches (provider counts per city and category, open and
close, locative names, districts); subscriptions, invoices and manual payments; catalogs; audit
log. Serbian only.
Backend: admin endpoints.
Acceptance:

- A venue can be onboarded end to end (create, invite, claim, submit, approve) without direct API
  calls.
- A city can be opened for venues, and its venues appear in search without a deployment.

#### M15 — Launch readiness · M

Scope: container image; reverse-proxy configuration; error and content pages in three languages;
accessibility pass; complete English and Russian translations reviewed by native speakers, with
missing translations failing the production build; end-to-end smoke tests; CI/CD to staging and
production.
Acceptance:

- Staging serves the full MVP over HTTPS in all three languages; smoke tests pass against it.
- No open WCAG 2.2 AA issues on the main flows; Lighthouse targets still met.

---

## 12. Open decisions

| #   | Decision                                                                                                                       | Needed by | Proposal                                                                                                                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------------ | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D1  | Free-slot search is listed for phase 2 in the backend plan, while the MVP designs search by date and show next free slots      | M3        | Include in the MVP                                                                                                                                       |
| D2  | Price calculation is listed for phase 2, while the MVP designs show a live total and deadlines before sending                  | M4        | `GET …/quote` in the MVP                                                                                                                                 |
| D3  | Parent's "deposit paid" notification has no endpoint                                                                           | M6        | `POST /api/track/{token}/payment-notice` (notification only), or remove                                                                                  |
| D4  | Extending a deposit deadline is not in the backend plan                                                                        | M9        | `POST …/extend-deposit`, moving `DepositDueAt` and `HeldUntil` together                                                                                  |
| D5  | Session model for "Moje proslave" after phone verification                                                                     | M6        | HttpOnly cookie valid 30 days, scoped to the verified phone; privacy review                                                                              |
| D6  | SMS is a phase 2 channel, but verification codes require it in the MVP                                                         | M5        | SMS provider in the MVP for verification only                                                                                                            |
| D7  | "Isteklo" is also used in the designs for inquiries unanswered within 48 h; the domain's `Expired` covers unpaid deposits only | M6        | Decide on response expiry; if adopted, add a job and an expiry reason                                                                                    |
| D8  | Ratings appear in designs; reviews are phase 2                                                                                 | M3        | Hide ratings until reviews exist                                                                                                                         |
| D9  | "Compare" and "Add to party plan" appear in designs; both are phase 2                                                          | M3        | Hide in the MVP                                                                                                                                          |
| D10 | Google sign-in is in the stack but not in MVP designs                                                                          | M8        | Defer                                                                                                                                                    |
| D11 | Provider scoping for panel endpoints (users may manage several providers)                                                      | M8        | Provider id in the path                                                                                                                                  |
| D12 | Map tile provider for production                                                                                               | M7        | A commercial provider's free tier; URL configurable                                                                                                      |
| D13 | Anonymous session identifier for search and engagement events                                                                  | M7        | Random id in `sessionStorage`, behind a configuration switch                                                                                             |
| D14 | Atkinson Hyperlegible Next has no Cyrillic glyphs                                                                              | M1        | Keep it for Serbian and English; Russian body text uses a Cyrillic font with similar metrics (e.g. Nunito Sans) via `:lang(ru)`, confirmed in the design |
| D15 | Review of the English and Russian translations                                                                                 | M15       | Native speakers review both before launch; the copy is frozen a week before                                                                              |
| D16 | Parents using English or Russian may have foreign phone numbers, but verification and the API accept Serbian numbers only      | M5        | Serbian mobile numbers only in the MVP (SMS cost and delivery), stated in the form in every language; revisit with search-log data                       |

---

## 13. Definition of done

A change is done when:

- It meets the acceptance criteria of its milestone item and matches the approved design at phone,
  tablet and desktop widths.
- Lint, format check, unit tests and the production build pass.
- New pure logic has unit tests; new API services have tests for success and error paths.
- It is operable by keyboard, has visible focus, labelled controls, and no colour-only meaning.
- Loading, empty and error states exist where data is loaded.
- No hard-coded colours or API hosts. In the public site and the provider panel, every
  user-facing string is marked for translation with a custom ID and has English and Russian
  translations (or is listed as missing, before launch only).
- On SSR routes: no browser-only API is used outside `afterNextRender` or a platform check, and no
  hydration warnings appear.

---

## 14. Later phases

**Phase 2:** catering and cakes (the service variants of search, profile, inquiry, calendar and
profile editor, opened city by city); multi-provider party plan; guest-count changes; comparison;
reviews and ratings; city, district and category landing pages; embeddable provider calendar;
provider statistics; date-range search; Google sign-in; "my city isn't listed yet" wishes for the
admin's launch decisions; optional English and Russian versions of provider-written texts.

**Phase 3:** planning assistant; digital invitations and RSVP; card payments for subscriptions;
parent accounts and birthday reminders; provider suggestions ("Predlozi"); additional categories
(animators, photographers, decoration); installable provider panel (PWA).

Each later feature is planned as its own milestone with scope, backend dependencies and acceptance
criteria in the format above.
