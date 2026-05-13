# Domain-Driven Design (DDD): Arquitectura Estratégica y Táctica

**Proyecto:** QuizArena  
**Clasificación:** Diseño de Arquitectura / Modelo de Dominio  

Este documento establece el modelado de dominio de la plataforma QuizArena. Delimita formalmente los contextos de negocio, especifica los contratos de integración inter-modular y define la estructura semántica táctica para asegurar la consistencia del sistema.

---

## 1. Destilación del Dominio

Segregación del dominio para priorizar el esfuerzo arquitectónico y técnico basándose en el valor competitivo del sistema.

### 1.1 Core Domain
Representa el diferenciador de negocio principal. Exige desarrollo interno y máximo rigor en patrones de diseño.
*   **LiveArena Context:** Sincronización de estado concurrente en milisegundos para partidas N-vs-N y cálculo de latencia de respuestas. Constituye el núcleo algorítmico y transaccional crítico del sistema.

### 1.2 Supporting Subdomains
Dominios necesarios para la viabilidad del producto, pero que no constituyen su núcleo competitivo o que dependen de la orquestación de servicios de terceros.
*   **AIKnowledge Context:** Orquestación de inferencia IA. El código en este dominio se limita a la segmentación de texto, estructuración de prompts y parseo de respuestas. La ejecución del modelo subyacente es un recurso de terceros.
*   **Assessment Context:** Gestión del banco de preguntas y curación de borradores de IA. Se clasifica como dominio de soporte dado que la evaluación estática es un patrón estandarizado y no representa la innovación tecnológica de la plataforma.
*   **Broadcasting Context `[V2]`:** Señalización de streaming y negociación de protocolos WebRTC. Diferido al segundo release.

### 1.3 Generic Subdomains
Problemas transversales resueltos por estándares de la industria.
*   **Identity Context:** Autenticación, autorización y gestión de sesiones. Delegación a un Identity Provider externo.

---

## 2. Context Mapping y Patrones de Integración

![Context Map](https://www.plantuml.com/plantuml/proxy?cache=no&src=https://raw.githubusercontent.com/ESalinasCh/QuizzArena/main/docs/diagrams/ddd_context_map.puml)

> Código fuente: [`docs/diagrams/ddd_context_map.puml`](docs/diagrams/ddd_context_map.puml)

### Especificación de Patrones
1.  **Anti-Corruption Layer:** Se implementa entre `AIKnowledge` y `Assessment`. Dado el comportamiento no determinista de los LLMs, el módulo de Assessment emplea una ACL para validar y sanitizar el payload JSON proveniente de la IA, transformándolo en entidades válidas de negocio tras validación manual.
2.  **Shared Kernel:** `Identity` comparte exclusivamente el Value Object `UserId` con el resto del sistema, evitando la exposición de atributos personales entre módulos.
3.  **Conformist `[V2]`:** El Broadcasting Context adoptará el estado del LiveArena Context. La señalización de video se someterá a los timestamps dictados por el motor transaccional del juego.
4.  **Customer / Supplier:** LiveArena (Downstream) depende de las estructuras de datos predefinidas por Assessment (Upstream) para inicializar las partidas.

---

## 3. Contratos de Nivel de Sistema

### 3.1 Contrato del Identity Provider
La API de QuizArena actúa como Resource Server. Valida de forma asimétrica los JWT emitidos por el IdP. Claims requeridos:
*   `sub`: Identificador persistente del usuario, mapeado al `UserId` local.
*   `email`: Para flujos de notificación asíncrona.
*   `quizarena:roles`: Array de roles para la evaluación de políticas RBAC.

### 3.2 Contrato del Modelo de Lenguaje
Las llamadas HTTP desde `QuizArena.AIKnowledge` hacia Ollama incluyen directivas de Few-Shot Prompting y fuerzan la generación de un JSON Schema predefinido:
*   **Estructura Esperada:**
    `{"questions": [{"text": "string", "correctAnswer": "string", "incorrectAnswers": ["string"]}]}`

---

## 4. Lenguaje Ubicuo

Glosario normativo para garantizar la paridad semántica entre los requerimientos de producto y el código fuente.

*   **Instructor / Student:** Actores principales con flujos de autorización mutuamente excluyentes.
*   **QuestionBank:** Repositorio persistente de preguntas aprobadas, asignado a un Instructor.
*   **Match:** Sesión síncrona N-vs-N gestionada por el servidor, con control de latencia y estado finito.
*   **PlayerResponse:** Registro inmutable de la interacción del estudiante: opción seleccionada y timestamp en milisegundos.
*   **KnowledgeChunk:** Fragmento vectorizado de una transcripción multimedia para procesos RAG.
*   **DraftQuestion:** Entidad temporal en estado de validación generada por la capa de inferencia.
*   **TranscriptJob:** Tarea asíncrona que representa el procesamiento de un archivo multimedia.

---

## 5. Modelado Táctico: Agregados y Tipos

Diseño para garantizar la consistencia ACID. Todo cambio de estado se procesa a través del Aggregate Root.

### 5.1 Aggregate: `Match` (QuizArena.LiveArena)
*   **Invariantes:**
    *   Mutaciones rechazadas si `MatchState != QuestionActive`.
    *   Restricción de una respuesta por combinación de `MatchId`, `PlayerId` y `QuestionId`.
*   **Value Object - `PlayerResponse`:** Inmutable. Encapsula `PlayerId`, `OptionId` y `LatencyMilliseconds`.

### 5.2 Aggregate: `QuestionBank` (QuizArena.Assessment)
*   **Invariantes:** Validación de cardinalidad estricta (1 opción correcta, al menos 1 distractor). Soft Delete obligatorio para preservar integridad referencial de partidas finalizadas.

### 5.3 Aggregate: `TranscriptJob` (QuizArena.AIKnowledge)
*   **Invariantes:** Máquina de estados finita unidireccional: `Queued -> Processing -> [Completed | Failed]`.

---

## 6. Eventos de Dominio

Propagación de estado asíncrona entre Bounded Contexts.

1.  **`TranscriptJobCompletedEvent`**
    *   Publisher: AIKnowledge
    *   Subscriber: Notificación al Instructor via WebSocket.
2.  **`MatchQuestionActivatedEvent`**
    *   Publisher: LiveArena
    *   Subscriber: Dispatcher de SignalR para sincronización global de clientes.
3.  **`PlayerAnswerReceivedEvent`**
    *   Publisher: LiveArena
    *   Subscriber: Motor de Leaderboard para re-evaluación de puntuaciones.
