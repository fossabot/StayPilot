# ADR-0003: Row-Level Multi-Tenancy (shared PostgreSQL)

- **Status:** Accepted
- **Date:** 2026-07-26
- **Deciders:** Engineering

## Context

StayPilot is multi-tenant SaaS from day one, scaling from one property to thousands. Options were
shared database with row-level isolation, schema-per-tenant, or database-per-tenant. We need
strong isolation guarantees with low operational overhead early, without painting the domain into
a corner.

## Decision

Use a **shared PostgreSQL database with row-level multi-tenancy**, keyed by a `Guid`
**`OrganizationId`**. Rules (enforced in code — see `StayPilot.SharedKernel` and
`StayPilot.Infrastructure`):

- Every tenant-owned entity implements `ITenantOwned` (`OrganizationId`). Global reference
  entities (Country, Currency, TimeZone, AmenityCatalog, …) do **not**.
- **EF Core global query filters** enforce `OrganizationId == CurrentTenant` automatically; the
  filter is applied to every `ITenantOwned` entity by convention in `StayPilotDbContext`.
- Tenant resolution happens in **middleware** and is exposed via **`ICurrentTenant`**; controllers
  never see or pass tenant filtering.
- On insert, tenant is **stamped** by the save interceptor. Audit rows and **domain events** all
  carry `OrganizationId`. Soft-delete filters combine with the tenant filter.
- `OrganizationId` is **indexed** on tenant-owned tables.
- The tenant seam lives in **infrastructure**, so **schema-per-tenant can be introduced later
  without changing the domain model**.

## Consequences

- Simplest to operate at launch; one schema, one migration stream.
- Correctness hinges on the global filter + stamping being applied everywhere — mitigated by
  convention-based application and an architecture/integration test that fails if a tenant-owned
  entity lacks a filter (planned).
- Deliberate cross-tenant/global reads must use `IgnoreQueryFilters()` explicitly.

## Alternatives considered

- **Schema-per-tenant** — stronger isolation, heavier migration/ops; deferred (seam preserved).
- **Database-per-tenant** — strongest isolation, highest cost; not warranted at this stage.
