# QuizArena

Plataforma educativa de trivia competitiva en tiempo real con generación de contenido por Inteligencia Artificial.

---

## Documentación del Proyecto

La siguiente tabla presenta todos los documentos de diseño y planificación del proyecto en el orden lógico de lectura. Se recomienda revisarlos secuencialmente para comprender el producto de arriba hacia abajo: desde la visión de negocio hasta las reglas de código.

| # | Documento | Descripción |
|---|---|---|
| 1 | [PRD.md](PRD.md) | **Product Requirements Document.** Visión completa del producto, usuarios objetivo, funcionalidades y requisitos no funcionales. |
| 2 | [MVP_Scope.md](MVP_Scope.md) | **Definición del MVP.** Hipótesis de negocio a validar, alcance funcional recortado y métricas de éxito. |
| 3 | [DDD_Model.md](DDD_Model.md) | **Modelo de Dominio (DDD).** Destilación del dominio, Context Mapping, lenguaje ubicuo, agregados y eventos. |
| 4 | [C4_Model.md](C4_Model.md) | **Diagramas C4.** Contexto de sistema (Nivel 1) y contenedores desplegables (Nivel 2) en PlantUML. |
| 5 | [Tech_Spec.md](Tech_Spec.md) | **Especificación Técnica.** Stack tecnológico, componentes del sistema, modelo de datos y decisiones clave. |
| 6 | [Engineering_Standards.md](Engineering_Standards.md) | **Manual del Desarrollador.** GitFlow, Conventional Commits, CI/CD, Arquitectura Hexagonal, BEM, Atomic Design, Testing y Logging. |
| 7 | [Product_Backlog.md](Product_Backlog.md) | **Scrum Product Backlog.** Épicas e Historias de Usuario con criterios de aceptación, listas para Sprint Planning. |

### Architecture Decision Records (ADRs)

| ADR | Decisión |
|---|---|
| [0001](docs/adr/0001-use-modular-monolith.md) | Monolito Modular con Screaming Architecture |
| [0002](docs/adr/0002-use-local-ollama-for-ai.md) | Ollama local para generación de IA |
| [0003](docs/adr/0003-use-webrtc-for-native-streaming.md) | WebRTC para streaming nativo (Diferido a V2) |
| [0004](docs/adr/0004-use-postgresql-with-pgvector.md) | PostgreSQL con pgvector como motor único |

---

## Stack Tecnológico (MVP)

| Capa | Tecnología |
|---|---|
| Backend | .NET 10, ASP.NET Core |
| Frontend | Angular 21 |
| Base de Datos | PostgreSQL + pgvector |
| Tiempo Real | SignalR |
| Inferencia IA | Ollama |
| Transcripción | Whisper |

---

## Inicio Rápido

```bash
# Levantar infraestructura local
docker compose up -d

# Compilar la solución
dotnet build QuizArena.sln
```
