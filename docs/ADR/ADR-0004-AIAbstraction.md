# ADR-0004: AI Provider Abstraction

- **Status:** Accepted
- **Date:** 2026-07-26
- **Deciders:** Engineering

## Context

StayPilot is AI-native: RAG, agents, and recommendations are core. The stack names Microsoft
**Semantic Kernel**, OpenAI-compatible providers, **Anthropic Claude**, and **pgvector**. Two
forces shape this decision: (1) models and providers change quickly, so business logic must not
bind to a specific vendor; (2) our "no preview packages" rule (see [ADR-0001](ADR-0001-DotNet10.md))
conflicts with Semantic Kernel's **pgvector/Postgres vector connector, which is preview-only**.

## Decision

- Introduce an **AI provider abstraction** so models can be switched without touching business
  logic. Business/domain code depends on our interfaces, not on a concrete SDK.
- Use **Semantic Kernel (stable core)** for orchestration; support **OpenAI-compatible** and
  **Anthropic Claude** providers behind the abstraction.
- For vector storage/RAG, use the **stable `Pgvector` + Npgsql** packages directly (behind the
  abstraction) rather than Semantic Kernel's **preview** vector connector, until that connector
  ships stable.
- Isolate all AI concerns in the **`AI` module**; the rest of the system consumes it through
  application-level ports.

## Consequences

- Vendor/model swaps are localized to the AI module.
- No preview packages enter the build; SK's vector connector can be adopted later when stable,
  with changes confined to the AI infrastructure.
- Per the Constitution: AI recommends before automating, every recommendation carries rationale,
  agents use tools (not direct DB writes), and AI is never the system of record — these constraints
  live behind the abstraction.

## Alternatives considered

- **Bind directly to a single provider SDK** — rejected: vendor lock-in, hard to A/B models.
- **Adopt SK's preview pgvector connector now** — rejected: violates the no-preview rule.
