# Specs

One short spec per step of the [implementation plan](../implementation-plan.md), written right before the step is built. The plan is the roadmap; a spec says what "done" means for one step, checked against the code as it is at that moment.

Steps finished before this folder existed (Milestone 0 up to 0.6) have no spec; their commits describe them.

## Workflow

1. **Draft**: the AI coding agent drafts the spec from the plan and the current code (the `backend-write-spec` skill), listing its decisions and the open questions.
2. **Approved**: the owner edits it and answers the questions. No code is written before this.
3. **Done**: the step is built against the spec. Anything that changed during the build is written back into the spec, which is committed together with the code.

## Naming

`<step>-<kebab-title>.md`, for example `1.3-provider-entity.md` or `0.6c-refresh-tokens.md`. Sub-steps that are separate commits get separate specs.

## Template

```markdown
# 1.3 Provider entity

Status: Draft | Approved | Done
Plan: [1.3 Provider entity](../implementation-plan.md#13-provider-entity)
Depends on: 1.2 (done)

## Goal
One or two sentences: what this step makes possible, and for whom.

## Scope
- In: ...
- Out: ... (and the step that covers it)

## Acceptance criteria
- Concrete, testable statements, failure paths included, each covered by a test below.

## API
Method, route, auth, request, response, errors (HTTP status and error code). "None" if there is no endpoint.

## Data model
Tables, columns, types, constraints, indexes. "No change" if there is none.

## Edge cases
- ...

## Tests
- Unit: ...
- Integration: ...

## Decisions
- The choice, and a one-line reason. Includes answered questions and anything changed during the build.

## Open questions
- The question, and the agent's recommendation. Removed once answered.
```
