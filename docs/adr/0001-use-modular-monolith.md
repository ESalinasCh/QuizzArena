# 1. Use Modular Monolith with Screaming Architecture

Date: 2026-05-12

## Status

Accepted

## Context

QuizArena is a complex educational platform with multiple distinct domains (identity, multiplayer arenas, asynchronous exams, AI generation, and live streaming). Developing this as a distributed microservices architecture from day one introduces unnecessary operational complexity, deployment overhead, and latency for a team in the initial MVP phase. However, a traditional layered monolith (Controllers/Services/Repositories) often degrades into a "Big Ball of Mud" where domain boundaries blur.

## Decision

We will adopt a **Modular Monolith** architecture combined with **Screaming Architecture**. The codebase will be physically structured by business domains (`Identity`, `Assessment`, `LiveArena`, `Broadcasting`, `AIKnowledgeBase`) rather than technical concerns. Furthermore, within each module, we will enforce **Hexagonal Architecture (Ports and Adapters)** to decouple the domain logic from infrastructure dependencies.

## Consequences

- **Positive:** Development speed is maximized (single deployment unit, simple debugging).
- **Positive:** Domain boundaries are strictly enforced. If a module needs to be extracted into a microservice in the future, the effort is minimal due to the Hexagonal abstractions.
- **Negative:** Requires discipline from developers to not bypass the Ports and Adapters within the monolith.
