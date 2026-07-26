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
├── docs/                             # All Markdown now (docx removed, deduped) as of 2026-07-26
│   ├── README.md                                 # Doc index + canonical-conflict hierarchy
│   │                                             #   (ADRs > Eng Bible > Domain > PRDs > Architecture > Product)
│   ├── 00_Engineering_Bible/
│   │   ├── ENG-000_StayPilot_Constitution.md     # v1 governance (15 principles, eng standards, DoD)
│   │   └── README.md                             # STUB (one line)
│   ├── 01_Product/
│   │   ├── 001_Product_Vision.md                 # STUB (one line)
│   │   └── 006_Project_Charter.md                # Charter + 5-competitor analysis (converted from docx)
│   ├── 02_Architecture/001_System_Architecture.md# STUB
│   ├── 03_Domain/001_Domain_Model.md             # STUB — no aggregate detail yet
│   ├── 04_Database/001_Database_Overview.md      # STUB
│   ├── 05_API/001_API_Standards.md               # STUB
│   └── 06_AI/001_AI_Architecture.md              # STUB
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
│       ├── StayPilot.Application/      # ✅ CQRS core (references SharedKernel + FluentValidation)
│       │   ├── Messaging/             #   IRequest/ICommand/IQuery, dispatcher, domain-event dispatch
│       │   ├── Behaviors/             #   Logging, Performance, Validation, Authorization (+ Tx/Idempotency seams)
│       │   ├── Abstractions/          #   ICurrentTenant, ICurrentUser, IUnitOfWork, IIdempotencyStore
│       │   └── DependencyInjection.cs
│       └── StayPilot.Infrastructure/   # ✅ EF Core 10 tenancy machinery
│           ├── Persistence/           #   StayPilotDbContext (tenant+soft-delete filters), interceptor, UnitOfWork
│           ├── Messaging/             #   DomainEventDispatcher (in-process)
│           ├── Behaviors/             #   Transaction, Idempotency
│           ├── Idempotency/           #   InMemoryIdempotencyStore (Redis TODO)
│           ├── Time/                  #   SystemClock
│           └── DependencyInjection.cs
│   StayPilot.slnx                     # .NET 10 XML solution (3 projects, builds clean)
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
- [x] Governance added and normalized to Markdown: Constitution (`ENG-000`), Project Charter +
      competitive analysis (`01_Product/006`), doc index with canonical-conflict hierarchy (`docs/README.md`).
- [x] **Doc hygiene done:** `.docx` files removed, Constitution deduped to one version, all docs
      now Markdown (diff-able) — satisfies the Constitution's "Documentation as Code" principle.
- [x] **Stack contradictions resolved by removal:** the `.NET 9` / `MediatR` `.docx` files no
      longer exist. *(Still worth recording the decisions positively as ADRs — see below.)*
- [ ] **Fill the topic docs — still one-line stubs:** **System Architecture**, **Domain Model**
      (aggregate boundaries/invariants), **Database** (ERD, row-level tenancy, naming),
      **API Standards** (error envelope from `Error`/`ErrorType`, pagination, versioning),
      **AI Architecture** (provider abstraction, RAG), and the Engineering Bible README.
- [ ] **Recover/relocate the removed product-strategy content** if still wanted: Product Vision &
      Business Strategy (MVP scope, target market, metrics) and the PMS/CRS research (S&R) are no
      longer in the repo — only the one-line `001_Product_Vision.md` stub + the Charter remain.
- [ ] Author **PRD-001 (Authentication)** and **PRD-002 (Organization)** — both still say "TBD",
      so auth/org features cannot be built to spec yet.
- [ ] Add ADRs recording the decisions (.NET 10 over 9, no-MediatR, FluentAssertions 7.x,
      row-level tenancy, pgvector-stable-only). No `docs/07_ADR/` (or equivalent) exists yet,
      though `docs/README.md` ranks ADRs as the highest authority.

### Infra / DevOps
- [ ] `docker-compose.yml`: Postgres 17 (pgvector image) + Redis 7.
- [ ] `.github/workflows/ci.yml`: restore → build → test on .NET 10.
- [ ] Angular frontend workspace (later).

---

## 6. Documentation Quality

**Current state: clean, well-organized, and consistent — but thin on technical substance.**
As of 2026-07-26 the docs were consolidated: the earlier `.docx` sprawl and duplicate
Constitutions were removed, everything is Markdown inside the numbered topic folders, and a
`docs/README.md` index now defines a **canonical-document conflict hierarchy**
(ADRs > Engineering Bible > Domain > PRDs > Architecture > Product), with a rule that archived
docs must never drive implementation.

**Strengths:**
- **Governance is solid and now the single source of truth.** `ENG-000` Constitution: 15
  principles, engineering standards ("every feature must include business objective, requirements,
  domain model, events, APIs, security review, AI design, test strategy, docs"), Definition of
  Done, amendment policy — all reinforcing the architecture already built (DDD, event-driven,
  multi-tenant, RBAC, CQRS, observability, explainable/human-in-the-loop AI).
- **Project Charter** (`01_Product/006`): documentation-first ground rules and a real competitor
  analysis (Cloudbeds, Mews, Guesty, Hostaway, Little Hotelier) with StayPilot's positioning.
- **Hygiene fixed:** all Markdown (diff-able, PR-reviewable), one canonical Constitution, and the
  previous doc-vs-code contradictions (docs asserting `.NET 9` / `MediatR`) are gone. This now
  honors the Constitution's own "Documentation as Code" principle.

**Gaps:**
- **Technical docs are still one-line stubs:** System Architecture, Domain Model (aggregates,
  invariants, bounded-context boundaries), Database (ERD, row-level tenancy, naming), API Standards
  (error envelope, pagination, versioning), AI Architecture (provider abstraction, RAG), and the
  Engineering Bible README. `diagrams/` is still empty.
- **No ADR trail yet** — despite `docs/README.md` ranking ADRs as the highest authority, there is
  no ADR folder. The locked decisions (.NET 10, no-MediatR, tenancy model, licensing pins) are
  recorded only in this summary and the code.
- **Some product-strategy content was dropped in the cleanup.** The MVP scope / target-market /
  metrics material and the PMS/CRS research (S&R) are no longer present; only the Charter and a
  one-line Product Vision stub remain. Recover or re-home if still wanted.
- **Both PRDs remain "TBD."**

**Net:** governance + structure + hygiene = strong; technical specification (domain, DB, API, AI)
and the ADR trail = the current gap.

---

## 7. Recommendations

1. **Stand up the ADR trail — it's the top of your own authority hierarchy but doesn't exist.**
   `docs/README.md` ranks ADRs #1, yet there is no ADR folder. Create one and record the decisions
   already made: .NET 10 (over 9), no-MediatR (custom dispatcher), FluentAssertions 7.x,
   FluentValidation 11.x, row-level tenancy, pgvector-stable-only. This makes the "why" durable
   and prevents well-meaning reversals. *(Doc hygiene and the earlier .NET 9/MediatR contradictions
   are already resolved — the `.docx` files were removed and everything is Markdown now.)*

2. **Fill the *technical* docs alongside code (honor `CLAUDE.md`).** Governance is strong; still
   one-line stubs: System Architecture, Domain Model (aggregate boundaries/invariants), Database
   Overview (ERD + row-level tenancy + naming), API Standards (error envelope from
   `Error`/`ErrorType`, pagination, versioning), and AI Architecture (provider abstraction, RAG).
   Write each with the module it governs.

3. **Recover the dropped product-strategy content if it's still needed.** The cleanup removed the
   Product Vision & Business Strategy (MVP scope, target market, metrics) and PMS/CRS research;
   only the Charter and a stub vision remain. Decide whether to re-home them as Markdown or treat
   them as intentionally archived.

4. **Write the two PRDs before building their features.** Authentication and Organization are
   foundational and currently unspecified; building them without requirements guarantees rework.

5. **Land the tenancy enforcement end-to-end early** and cover it with tests. Row-level
   multi-tenancy is only safe if the global query filter, tenant stamping, and middleware
   resolution are all verified together — a single missed filter is a cross-tenant leak.
   Add an architecture/integration test that fails if a tenant-owned entity lacks a filter.

6. **Record the licensing-driven choices as ADRs** so they are not silently "corrected" later:
   no MediatR (v13+ commercial), FluentAssertions pinned to 7.x (v8+ commercial), FluentValidation
   on the 11.x line, and Semantic Kernel's pgvector connector avoided while it is preview-only.

7. **Add CI as soon as the build is green** (restore/build/test on .NET 10) and stand up
   `docker-compose` (Postgres 17 + Redis 7) so integration tests and local dev work day one.

8. **Establish the reference vertical slice (Organizations) as the template** the team copies
   for every subsequent module — aggregate, command/query, validator, EF config, endpoint,
   and tests — so DDD/Clean/CQRS conventions stay consistent across the modular monolith.

8. **Keep `REPOSITORY_SUMMARY.md` (or migrate its living parts into `docs/`) updated** as
   modules land, so the summary does not drift from reality.
```
