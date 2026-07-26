# ADR-0005: Dependency Licensing Policy

- **Status:** Accepted
- **Date:** 2026-07-26
- **Deciders:** Engineering

## Context

Several popular .NET libraries have shifted to commercial licensing, creating cost/obligation risk
for a commercial SaaS as it scales. We need a standing policy so licensing is considered when
choosing dependencies, not discovered later.

## Decision

Prefer **permissively licensed (MIT/Apache-2.0), long-term-supported** dependencies. Specific
consequences of applying this policy today:

| Library | Choice | Reason |
|---|---|---|
| **MediatR** | **Not used** — custom dispatcher | v13+ commercial (free only < $5M revenue). See [ADR-0002](ADR-0002-CustomDispatcher.md). |
| **FluentAssertions** | **Pinned to 7.x** | v8+ (Jan 2025) is commercial (Xceed). 7.x is the last free (Apache-2.0) line. Shouldly is the free-forever fallback. |
| **FluentValidation** | **11.x line** | Last broadly-permissive line; used for the validation behavior. |
| **AutoMapper** | **Avoid** | Same commercial move as MediatR; prefer manual/explicit mapping. |
| **Semantic Kernel pgvector connector** | **Avoided while preview** | Not a license issue but a stability rule; see [ADR-0004](ADR-0004-AIAbstraction.md). |

Additional rules:
- **No preview/prerelease packages** in the build.
- Central Package Management (`Directory.Packages.props`) pins every version; new dependencies are
  reviewed for license before adoption.

## Consequences

- No revenue-triggered license fees on core paths.
- Occasionally more first-party code (e.g. the dispatcher, explicit mapping) instead of a library.
- Version ceilings on some test libraries (e.g. FluentAssertions 7.x) — revisit if a free
  successor emerges.

## Alternatives considered

- **Accept commercial licenses and budget for them** — rejected for now; avoidable with modest effort.
