# ADR-0001: Target .NET 10 (LTS)

- **Status:** Accepted
- **Date:** 2026-07-26
- **Deciders:** Engineering

## Context

The initial spec named **.NET 9** ("LTS if available for production, otherwise the latest
stable .NET 9 SDK"). As of mid-2026:

- **.NET 9 is a Standard-Term Support (STS)** release and reaches **End of Support on
  2026-11-10** — roughly four months out.
- **.NET 10 is the current Long-Term Support (LTS)** release, supported through **Nov 2028**.

The stated intent was production-grade, LTS-quality stability. .NET 9 is never LTS, so honoring
the *intent* points to .NET 10.

## Decision

Target **.NET 10 (LTS)** across every project. Pin all packages to the latest **stable**
versions compatible with .NET 10. **No preview/prerelease packages.** Keep the solution
upgrade-friendly.

## Consequences

- Support runway to Nov 2028; no near-term forced runtime migration.
- EF Core 10 / Npgsql 10 / ASP.NET Core 10 across the stack.
- Requires the .NET 10 SDK on all build/CI machines (installed: 10.0.302).
- The `.slnx` XML solution format (new default in .NET 10 CLI) is used.
- Supersedes any documentation that still references ".NET 9" (e.g. earlier Tech Stack notes).

## Alternatives considered

- **.NET 9 as literally specified** — rejected: EOL in months; immediate upgrade debt.
- **.NET 8 (previous LTS)** — rejected: older; .NET 10 LTS is available now.

## Verification

`src/StayPilot.slnx` builds clean on .NET 10.0.302 (0 warnings, 0 errors, warnings-as-errors on).
