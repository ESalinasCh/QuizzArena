# 2. Use Local Ollama for AI Generation

Date: 2026-05-12

## Status

Accepted

## Context

The QuizArena platform requires an AI pipeline to ingest multimedia recordings (classes/lectures) and generate structured question sets (Quizzes) on demand. Utilizing managed cloud LLMs (like OpenAI GPT-4o or Azure OpenAI) provides high quality but introduces significant variable operational costs per token, which is restrictive during the initial development and validation phases of the project.

## Decision

We will use **Ollama** running locally (or in a controlled container) hosting open-source models (e.g., Llama 3) to handle the AI generation pipeline. The integration will be abstracted behind an `ILLMProvider` interface (Hexagonal Architecture). 

## Consequences

- **Positive:** Zero variable costs during development and early testing.
- **Positive:** Complete data privacy for user-uploaded lecture content.
- **Positive:** The `ILLMProvider` interface ensures that we can swap to a managed cloud LLM (like OpenAI) in the future by simply writing a new adapter, without touching the core domain logic.
- **Negative:** Local inference is significantly slower and requires high-end hardware/GPUs on the host machine.
- **Negative:** Open-source models may hallucinate more frequently, reinforcing the business rule that all AI-generated questions must enter a "Draft" state for mandatory human review.
