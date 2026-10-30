---
name: backend-write-spec
description: Draft a one-page spec for a step of the backend plan (Backend/docs/implementation-plan.md) before it is built, checked against the current code, and wait for approval before writing code. Backend steps are numbered like 1.3 or 0.6c (frontend milestones are M1, M2 and have their own skills). Use when asked to "spec 1.3", "write the spec for the next backend step", "start 0.9", "build 1.4", or before implementing any backend plan step that has no approved spec in Backend/docs/specs yet.
---

# Write a spec for a plan step

Every step of the backend plan, `Backend/docs/implementation-plan.md`, gets a short spec in `Backend/docs/specs/` before any code is written. The plan says *what* to build; the spec says what "done" means, written just in time so it matches the current code. Paths are relative to `Backend/`. The conventions are in [Backend/docs/specs/README.md](../../../Backend/docs/specs/README.md).

## 1. Find the step

- If a step is named ("1.3", "0.6c"), use it. If asked for "the next step", it is the first step in plan order with no spec at `Done` and no implementation in the code or `git log`. Say which step you picked and why.
- Read the step, the **Key decisions** table, the **Dependencies** diagram, and any step it mentions. If a dependency isn't done, stop and say so.
- A step with sub-steps (0.6a to 0.6e) gets one spec per sub-step when each one is a commit of its own. A step that would need more than a page of acceptance criteria is too big: propose a split.

## 2. Check it against the code

- Read the code the step will touch or build on: entities, existing slices that do something similar, configuration, migrations, tests. Follow what is already there rather than what the plan's wording suggests.
- Note every place where the plan and the code disagree (a name, a type, a flow that already works differently). These become open questions, not silent choices.
- Check `.claude/rules/backend-architecture.md` and `.claude/rules/backend-coding-style.md` (at the repo root) for constraints that apply (layers, LLM output, SQL, caching).

## 3. Draft the spec

Write `docs/specs/<step>-<kebab-title>.md` (for example `1.3-provider-entity.md`, `0.6c-refresh-tokens.md`) from the template in `docs/specs/README.md`, with `Status: Draft`.

- **Acceptance criteria** are concrete and testable: each one maps to at least one test in the Tests section. Include failure paths (validation, not found, forbidden, conflict) with their error codes.
- **Scope → Out** names the step that covers each excluded thing, so nothing falls between steps.
- **API** lists method, route, auth, request, response and errors (HTTP status and `Error` code). Write "None" when the step has no endpoint.
- **Data model** lists tables, columns, types, constraints and indexes, or "No change".
- **Decisions** records each choice you made that the plan doesn't dictate, with a one-line reason.
- **Open questions** lists what only the owner can decide. Give each one your recommendation. Don't hide a guess in the other sections.
- Keep it to about one page. Don't paste code; name types and files instead.

## 4. Stop for approval

Show a short summary: the step, the main acceptance criteria, your decisions, and the open questions. **Don't write code yet.** The owner edits the spec or answers the questions; move each answer into Decisions and remove the question. When the owner approves, set `Status: Approved`.

## 5. While building and after

- Implement against the approved spec (with `backend-add-vertical-slice` and `backend-db-migration` where they apply).
- If something has to change during implementation, update the spec first (acceptance criteria or Decisions) and tell the owner. The spec must describe what was built, not what was first planned.
- When the step is finished and the tests pass, set `Status: Done`. The spec goes in the same commit as the code.
- If the step changed the plan (a split, a move, a new later step), update `docs/implementation-plan.md` in the same commit.
