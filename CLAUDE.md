# KidsPartyVenue

A site for finding and booking kids' party venues in Serbia. Claude is started from this root folder.

| Folder | What | Start here |
|---|---|---|
| `Backend/` | ASP.NET Core (.NET 10) API, PostgreSQL 18 + PostGIS, clean architecture with CQRS vertical slices | [Backend/CLAUDE.md](Backend/CLAUDE.md), [plan](Backend/docs/implementation-plan.md), [specs](Backend/docs/specs/README.md) |
| `Frontend/` | Angular 22 with SSR for the public site, SPA panels for providers and admins, Tailwind | [frontend plan](Frontend/docs/frontend-implementation-plan.md) |

`Backend/CLAUDE.md` loads when you work on backend files. Its paths are relative to `Backend/`.

## How the backend is built

Claude writes it, step by step from the plan. Each step starts with an approved spec (`backend-write-spec` skill), is built with `backend-add-vertical-slice` and `backend-db-migration`, and is checked by the `backend-arch-reviewer` agent before committing.

## Conventions

- All work happens directly on `main`, in small, focused commits. No branches or pull requests.
- Keep backend and frontend changes in separate commits.

## Commands

- Backend: run from `Backend/` (`cd Backend`); see [Backend/CLAUDE.md](Backend/CLAUDE.md#commands).
- Frontend: run from `Frontend/`: `npm start` (http://localhost:4200), `npm run build`, `npm run lint`, `npm run format:check`.

## Claude setup (`.claude/`)

| Path | Purpose |
|---|---|
| `rules/backend-architecture.md`, `rules/backend-coding-style.md` | Backend rules; they load only for files under `Backend/` |
| `skills/backend-write-spec`, `skills/backend-add-vertical-slice`, `skills/backend-db-migration` | Backend workflows: spec a plan step, add a use case, change the schema |
| `agents/backend-arch-reviewer.md` | Reviews backend changes before each commit |
| `hooks/backend-protect-migrations.ps1` | Blocks hand edits to generated EF Core migrations |
