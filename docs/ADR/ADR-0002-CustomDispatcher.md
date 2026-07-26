# ADR-0002: Custom CQRS Dispatcher (no MediatR)

- **Status:** Accepted
- **Date:** 2026-07-26
- **Deciders:** Engineering

## Context

The stack called for CQRS with **MediatR**. In July 2025, **MediatR v13+ moved to a commercial
dual-license** (Reciprocal Public License / paid, Lucky Penny Software); the free "Community"
tier is limited to organizations under **$5M annual revenue**. v12.x remains Apache-2.0 but is
frozen. StayPilot is a commercial SaaS intended to scale past that threshold, so depending on
MediatR creates a future licensing cost/obligation.

## Decision

Do **not** depend on MediatR. Implement a lightweight, in-house CQRS layer in
`StayPilot.Application`:

- `IRequest<TResponse>` with semantic markers `ICommand` / `ICommand<T>` / `IQuery<T>`.
- `IRequestHandler<TRequest,TResponse>` (+ `ICommandHandler` / `IQueryHandler`).
- `IRequestDispatcher` — resolves handler + `IPipelineBehavior<,>` chain from DI (reflection
  cached per closed request type).
- Pipeline behaviors: Logging → Performance → Validation → Authorization → Idempotency →
  Transaction → Handler. Validation uses **FluentValidation**.

The dispatcher surface is intentionally minimal so it stays **replaceable** — MediatR (or another
mediator) can be reintroduced later without changing handlers or business logic.

## Consequences

- No third-party licensing strings on the core request path.
- We own a small amount of infrastructure (dispatcher + behaviors) and its tests.
- Handlers follow a familiar `IRequest`/`ICommand`/`IQuery` shape, easing onboarding and any
  future swap.

## Alternatives considered

- **MediatR Community tier** — rejected: license trigger at >$5M revenue.
- **Pin MediatR 12.x (Apache-2.0)** — rejected: frozen line, no future fixes.
- **Other OSS mediators** — viable, but a ~150-line dispatcher we control is simpler and
  dependency-free.

See also [ADR-0005 Licensing](ADR-0005-Licensing.md).
