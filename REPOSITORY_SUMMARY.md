# StayPilot — Repository Summary

> **Generated:** 2026-07-26
> **Branch:** `main`
> **Product:** StayPilot — an AI-native hospitality operating system (multi-tenant SaaS).
> **Status:** Early greenfield. Documentation scaffold + the first two foundational
> backend libraries (SharedKernel, Application/CQRS core) are in place. No runnable
> application yet. **.NET 10 SDK (10.0.302) is installed**; the solution
> (`src/StayPilot.slnx`) builds clean — **0 warnings, 0 errors** with warnings-as-errors on.

---

## 1. Folder Structure

```
StayPilot-Code/
├── .editorconfig                     # Solution-wide code style (added this session)
├── .gitignore                        # bin/ obj/ .vscode/
├── CLAUDE.md                         # Working rule: read docs first; DDD + Clean + CQRS; docs with code
├── README.md                         # One-liner: "AI-native Hospitality Operating System"
├── REPOSITORY_SUMMARY.md             # This document
│
├── .github/workflows/ci.yml          # STUB — `jobs: {}` (no pipeline yet)
│
├── docs/                             # Architecture & governance docs (all one-line stubs)
│   ├── 00_Engineering_Bible/README.md
│   ├── 01_Product/001_Product_Vision.md
│   ├── 02_Architecture/001_System_Architecture.md
│   ├── 03_Domain/001_Domain_Model.md
│   ├── 04_Database/001_Database_Overview.md
│   ├── 05_API/001_API_Standards.md
│   ├── 06_AI/001_AI_Architecture.md
│   └── StayPilot_Kickoff_Pack.docx   # One sentence of content
│
├── prds/                            # Product requirements (both "TBD")
│   ├── PRD-001-Authentication.md
│   └── PRD-002-Organization.md
│
├── docker/docker-compose.yml         # STUB — `services: {}` (no Postgres/Redis yet)
│
├── diagrams/                         # EMPTY
├── prompts/                          # EMPTY (intended for AI prompt assets)
├── scripts/                          # EMPTY
│
├── src/
│   ├── Directory.Build.props          # net10.0, nullable, warnings-as-errors, CPM on
│   ├── Directory.Packages.props       # Central Package Management — all versions pinned
│   └── BuildingBlocks/
│       ├── StayPilot.SharedKernel/    # ✅ Domain primitives (zero external deps)
│       │   ├── Primitives/            #   Entity, AggregateRoot, ValueObject
│       │   ├── Domain/                #   IDomainEvent, DomainEvent (tenant-stamped)
│       │   ├── Tenancy/               #   ITenantOwned
│       │   ├── Auditing/              #   IAuditable, ISoftDeletable
│       │   ├── Results/               #   Result, Result<T>, Error
│       │   └── Abstractions/          #   IClock
│       └── StayPilot.Application/      # ✅ CQRS core (references SharedKernel + FluentValidation)
│           ├── Messaging/             #   IRequest/ICommand/IQuery, dispatcher, pipeline
│           ├── Behaviors/             #   Logging, Performance, Validation, Authorization (+ Tx/Idempotency seams)
│           ├── Abstractions/          #   ICurrentTenant, ICurrentUser, IUnitOfWork, IIdempotencyStore
│           └── DependencyInjection.cs
│
└── tests/                           # EMPTY (no test projects yet)
```

**Note:** The top-level folder layout is fixed and must not be reorganized. New backend
code goes under `src/`, tests under `tests/`, docs under `docs/`.

---

## 2. Existing Architecture

The intended architecture (per `CLAUDE.md` and the doc stubs) is a **Modular Monolith**
combining **Domain-Driven Design + Clean Architecture + CQRS + Vertical Slice + Event-Driven +
API-First**. What has actually been built so far establishes the backbone:

### Layering (Clean Architecture, dependencies point inward)

| Layer | Project | Depends on | Status |
|-------|---------|-----------|--------|
| Domain kernel | `StayPilot.SharedKernel` | *(nothing)* | ✅ Built |
| Application | `StayPilot.Application` | SharedKernel, FluentValidation | ✅ Built |
| Infrastructure (shared) | `StayPilot.Infrastructure` | Application, EF Core 10, Npgsql 10 | ✅ Built — base ctx, tenant+soft-delete filters, audit/tenant interceptor, domain-event dispatch, Transaction + Idempotency behaviors, in-memory idempotency store (Redis TODO) |
| Host / API | `StayPilot.Api` | all modules | ⬜ Not started |
| Feature modules | `Modules/<Context>/…` | SharedKernel, Application | ⬜ Not started |

### Cross-cutting design decisions already encoded

- **CQRS without MediatR.** A hand-rolled, license-safe `IRequestDispatcher` resolves
  handlers and `IPipelineBehavior<,>` chains from DI (reflection-cached per request type).
  Deliberately swappable for MediatR later without touching business logic.
- **Pipeline order:** Logging → Performance → Validation → Authorization → *Idempotency* →
  *Transaction* → Handler. The italicised two are seams (markers + abstractions defined in
  Application; implementations belong in Infrastructure because they need EF Core / Redis).
- **Result pattern.** Expected failures flow as `Result` / `Result<T>` + typed `Error`
  (`ErrorType` maps cleanly to HTTP status), not exceptions.
- **Multi-tenancy (row-level, shared database).** Encoded as first-class primitives:
  - `ITenantOwned` (UUID `OrganizationId`) marks tenant-owned entities; global reference
    entities (Country, Currency, TimeZone, AmenityCatalog…) intentionally do **not** implement it.
  - Every `IDomainEvent` carries `OrganizationId` (record-based, tenant-stampable at persistence).
  - `IAuditable` / `ISoftDeletable` are read-only to the domain, stamped by infrastructure;
    soft-delete is designed to combine with the tenant filter.
  - `ICurrentTenant` is the single tenant seam; tenant resolution will happen in middleware,
    EF Core **global query filters** will enforce isolation, and controllers never see tenant filtering.
  - The design keeps tenancy in the infrastructure seam so **schema-per-tenant can be introduced
    later without changing the domain model**.

---

## 3. Technology Stack

Decided and pinned (see `src/Directory.Packages.props`). **Latest stable only — no preview
packages.** Exact patch versions are to be finalized at first `dotnet restore` under the
.NET 10 SDK.

| Area | Choice | Notes |
|------|--------|-------|
| Runtime | **.NET 10 (LTS)** | Chosen over the originally-specified .NET 9 because .NET 9 is STS and reaches EOL 2026-11-10; .NET 10 is supported to Nov 2028. |
| Web | ASP.NET Core (net10.0) | API-first; OpenAPI/Swagger. |
| Persistence | EF Core 10 + Npgsql, **PostgreSQL 17** | Central Package Management, migrations per module. |
| Cache / distributed | **Redis 7** | Idempotency store, caching. |
| CQRS dispatch | **Custom dispatcher (no MediatR)** | MediatR v13+ is commercially licensed; avoided by design. |
| Validation | FluentValidation 11.x | 11.x is the last permissively-licensed line. |
| Logging | Serilog | Structured logs. |
| Telemetry | OpenTelemetry | Tracing/metrics (EF Core instrumentation pkg is still beta — to confirm at restore). |
| Auth | ASP.NET Core Identity + JWT + refresh tokens + RBAC + policy-based authz | Not yet implemented (PRD-001 is TBD). |
| AI | Microsoft Semantic Kernel (stable core), OpenAI-compatible + Anthropic Claude, **pgvector** | Provider abstraction required. SK's pgvector connector is **preview only**, so vector storage will use the stable `Pgvector` + Npgsql packages instead, behind the AI abstraction. |
| Testing | xUnit + **FluentAssertions 7.x** + Testcontainers + NetArchTest | FA v8+ is commercial; 7.x is the last free line (Shouldly is the free-forever fallback if preferred). |
| Frontend | Angular (latest) + Angular Material + Tailwind | Not started; `ng` CLI not installed. |
| Infra/CI | Docker, Docker Compose, GitHub Actions, Azure-ready | compose + CI are stubs; Docker not on PATH. |
| Docs | OpenAPI, Mermaid, Markdown | — |

### Toolchain status on the build machine

| Tool | Needed | Present |
|------|--------|---------|
| .NET SDK 10 | ✅ | ✅ 10.0.302 installed (alongside 8.0.404 & 9.0.314) |
| Docker | later | ❌ not on PATH |
| Node | frontend | ✅ v26 (Angular CLI not installed) |

---

## 4. Completed Modules

There are **no feature/business modules yet**. What is complete are two **foundational
building-block libraries**:

| Component | What it provides | State |
|-----------|------------------|-------|
| `StayPilot.SharedKernel` | Entity / AggregateRoot / ValueObject; domain events (tenant-aware); Result/Error; tenancy, audit, soft-delete marker interfaces; IClock. Zero external dependencies. | ✅ Built — compiles clean |
| `StayPilot.Application` | CQRS contracts (IRequest/ICommand/IQuery + handlers), custom `IRequestDispatcher`, 4 implemented pipeline behaviors + 2 seams, ICurrentTenant/ICurrentUser, IUnitOfWork/IIdempotencyStore, DI registration. | ✅ Built — compiles clean |

> "Completed" means authored and compiling under .NET 10 with warnings-as-errors — **not yet
> unit-tested** (no test projects exist under `tests/` yet).

Domain contexts named in the docs but **not yet modelled or built**: Organizations,
Properties, Cabins, Guests, Reservations, plus Identity/Auth and AI.

---

## 5. Outstanding Work

### Immediate blockers
- [ ] Install **.NET 10 SDK** on the build machine.
- [ ] Create `StayPilot.sln`, add the two projects, `dotnet restore` + `dotnet build`,
      and **lock package versions** to verified latest-stable.

### Backend scaffolding (planned phases)
- [x] **Infrastructure.Shared** — base `DbContext` with tenant + soft-delete global query
      filters; SaveChanges interceptor (audit + tenant stamping + soft-delete conversion);
      in-process domain-event dispatch; `SystemClock`; **Transaction** and **Idempotency**
      pipeline behaviors. *Remaining here:* transactional **Outbox** for reliable events,
      **Redis-backed** idempotency store (currently in-memory placeholder), and the
      `ICurrentTenant`/`ICurrentUser` implementations (belong in the Host layer, from HttpContext/JWT).
- [ ] **Host (`StayPilot.Api`)** — composition root; Serilog + OpenTelemetry; OpenAPI/Swagger;
      JWT auth; tenant-resolution middleware; health checks; module registration seam.
- [ ] **Organizations module** — first reference vertical slice (root aggregate + a
      tenant-owned child; command/query/validator) to lock the pattern.
- [ ] Remaining modules: **Identity/Auth, Properties, Cabins, Guests, Reservations, AI**.
- [ ] **AI module** — Semantic Kernel wiring, provider abstraction (OpenAI-compatible +
      Anthropic Claude), pgvector RAG store (stable packages only).

### Testing
- [ ] Test projects under `tests/`: architecture tests (NetArchTest) to enforce DDD/Clean
      boundaries; per-module Domain/Application unit tests; integration tests (Testcontainers
      for Postgres/Redis).

### Product / documentation
- [ ] Write real content for all `docs/` files (currently one-liners).
- [ ] Author **PRD-001 (Authentication)** and **PRD-002 (Organization)** — both are "TBD",
      so auth/org features cannot be built to spec yet.
- [ ] Add ADRs recording the decisions (.NET 10 over 9, no-MediatR, FluentAssertions 7.x,
      row-level tenancy, pgvector-stable-only).

### Infra / DevOps
- [ ] `docker-compose.yml`: Postgres 17 (pgvector image) + Redis 7.
- [ ] `.github/workflows/ci.yml`: restore → build → test on .NET 10.
- [ ] Angular frontend workspace (later).

---

## 6. Documentation Quality

**Current state: skeletal.** The information architecture is thoughtfully laid out
(`00_Engineering_Bible` … `06_AI`, plus a `prds/` folder), which signals good intent — but
**every doc is a single sentence and both PRDs say "TBD."** The `.docx` kickoff pack likewise
contains one line.

Implications:
- `CLAUDE.md` mandates "read docs before coding," yet the docs currently carry almost no
  decision content — so the *real* source of truth is presently this summary and the code.
- There is no ADR trail, no domain glossary, no ERD, no API contract, no sequence/Mermaid
  diagrams yet (the `diagrams/` folder is empty).

**Strength:** structure and conventions are in place and consistent.
**Gap:** substance. Docs need to be filled before/with the code they govern.

---

## 7. Recommendations

1. **Unblock the build first.** Install the .NET 10 SDK, then create the solution, restore,
   build the two existing libraries, and pin verified package versions. Do not author more
   layers blind — Infrastructure onward binds to external packages whose compatibility must
   be confirmed at restore.

2. **Make the docs real, incrementally, alongside code (honor `CLAUDE.md`).** For each module,
   write the domain model section + relevant ADR *before or with* the implementation. Start
   with: System Architecture (the modular-monolith + layering diagram), Domain Model (aggregate
   boundaries), Database Overview (row-level tenancy + naming), API Standards (error envelope
   from `Error`/`ErrorType`, pagination, versioning), and AI Architecture (provider abstraction).

3. **Write the two PRDs before building their features.** Authentication and Organization are
   foundational and currently unspecified; building them without requirements guarantees rework.

4. **Land the tenancy enforcement end-to-end early** and cover it with tests. Row-level
   multi-tenancy is only safe if the global query filter, tenant stamping, and middleware
   resolution are all verified together — a single missed filter is a cross-tenant leak.
   Add an architecture/integration test that fails if a tenant-owned entity lacks a filter.

5. **Record the licensing-driven choices as ADRs** so they are not silently "corrected" later:
   no MediatR (v13+ commercial), FluentAssertions pinned to 7.x (v8+ commercial), FluentValidation
   on the 11.x line, and Semantic Kernel's pgvector connector avoided while it is preview-only.

6. **Add CI as soon as the build is green** (restore/build/test on .NET 10) and stand up
   `docker-compose` (Postgres 17 + Redis 7) so integration tests and local dev work day one.

7. **Establish the reference vertical slice (Organizations) as the template** the team copies
   for every subsequent module — aggregate, command/query, validator, EF config, endpoint,
   and tests — so DDD/Clean/CQRS conventions stay consistent across the modular monolith.

8. **Keep `REPOSITORY_SUMMARY.md` (or migrate its living parts into `docs/`) updated** as
   modules land, so the summary does not drift from reality.
```
