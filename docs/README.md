# StayPilot Documentation Index

## Canonical Documents

Only the documents listed here are considered authoritative.

All implementation must follow these documents.

If multiple documents conflict:

1. ADRs
2. Engineering Bible
3. Domain Documents
4. PRDs
5. Architecture
6. Product Documents

Archived documents must never be used for implementation.

## Structure

| Section | Purpose |
|---|---|
| [`00_Engineering_Bible/`](00_Engineering_Bible/) | Governance: Constitution, principles, coding & doc standards, Definition of Done |
| [`01_Product/`](01_Product/) | Vision, strategy, market, competition, MVP, charter, roadmap |
| [`02_Architecture/`](02_Architecture/) | System architecture, modular monolith, Clean, DDD, CQRS, event-driven, multi-tenancy, AI, security |
| [`03_Domain/`](03_Domain/) | Hospitality domain model and per-context docs (Organization, Property, Reservation, …) |
| [`04_Database/`](04_Database/) | DB architecture, ERD, naming, indexing, migrations, multi-tenant data model |
| [`05_API/`](05_API/) | API standards, error handling, pagination/filtering/sorting, versioning, webhooks, OpenAPI |
| [`06_AI/`](06_AI/) | AI architecture, agents, RAG, prompts, vector search, memory, model providers |
| [`07_Operations/`](07_Operations/) | Deployment, monitoring, logging, backup, DR, security ops |
| [`ADR/`](ADR/) | Architecture Decision Records (highest authority) |
| `assets/` | Images and diagrams referenced by docs |

## Architecture Decision Records

| ADR | Decision |
|---|---|
| [ADR-0001](ADR/ADR-0001-DotNet10.md) | Target .NET 10 (LTS) |
| [ADR-0002](ADR/ADR-0002-CustomDispatcher.md) | Custom CQRS dispatcher (no MediatR) |
| [ADR-0003](ADR/ADR-0003-MultiTenancy.md) | Row-level multi-tenancy (shared PostgreSQL) |
| [ADR-0004](ADR/ADR-0004-AIAbstraction.md) | AI provider abstraction |
| [ADR-0005](ADR/ADR-0005-Licensing.md) | Dependency licensing policy |
| [ADR-0006](ADR/ADR-0006-ModularMonolith.md) | Modular monolith + Clean + vertical slices |

> **Status legend:** most topic docs are currently `Draft` scaffolds to be filled with the
> module/topic they govern. The Constitution, Project Charter, and the ADRs above carry content.