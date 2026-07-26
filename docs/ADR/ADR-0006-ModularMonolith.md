# ADR-0006: Modular Monolith + Clean Architecture + Vertical Slices

- **Status:** Accepted
- **Date:** 2026-07-26
- **Deciders:** Engineering

## Context

StayPilot spans many bounded contexts (Organizations, Properties, Accommodation, Guests,
Reservations, Pricing, Availability, Housekeeping, Maintenance, Payments, OTA, Channel Manager,
CRM, AI). We need clear domain boundaries and independent module evolution without the operational
cost of microservices for an early-stage product.

## Decision

Build a **Modular Monolith** that combines **Domain-Driven Design**, **Clean Architecture**, and
**Vertical Slice** organization, deployed as a single ASP.NET Core host.

- **Building blocks** (shared): `StayPilot.SharedKernel` (domain primitives, tenancy/audit markers,
  domain events), `StayPilot.Application` (CQRS core, behaviors, ports), `StayPilot.Infrastructure`
  (EF Core tenancy machinery, dispatch, behaviors).
- **Modules** — one bounded context each, internally layered Domain → Application → Infrastructure,
  communicating across boundaries via contracts and **domain/integration events**, never by
  reaching into another module's internals or tables.
- **Host** (`StayPilot.Api`) — composition root that wires modules; API-first.
- Dependencies always point **inward** (Clean Architecture); the domain has no infrastructure
  dependencies.

The modular boundaries are drawn so a module could later be **extracted into a service** with
minimal churn if scale demands it.

## Consequences

- Single build/deploy/debug; one database (with row-level tenancy, see
  [ADR-0003](ADR-0003-MultiTenancy.md)).
- Requires discipline to keep module boundaries clean — enforced by **architecture tests**
  (NetArchTest) that fail on illegal cross-module or layer-violating references (planned).
- Fast iteration now; a viable path to selective service extraction later.

## Alternatives considered

- **Microservices from day one** — rejected: premature operational complexity for the current stage.
- **Layered monolith without module boundaries** — rejected: erodes into a big ball of mud; no
  extraction path.
