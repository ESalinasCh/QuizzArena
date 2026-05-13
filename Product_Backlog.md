# Scrum Product Backlog: QuizArena (Fase MVP)

**Proyecto:** QuizArena  
**Clasificación:** Gestión de Producto / Metodología Ágil  

Este documento desglosa el alcance funcional del MVP en Épicas e Historias de Usuario, listas para ser estimadas y asignadas en los Sprints de desarrollo.

---

## Epic 0: Infraestructura y Habilitadores

*Prerrequisitos técnicos del Sprint 0.*

### EN-0.1: Andamiaje de la Arquitectura(German Luigi)
*   **Tipo:** Tarea Técnica.
*   **Descripción:** Crear la solución `QuizArena.sln` en .NET 8 y aislar los proyectos físicos según la Screaming Architecture.
*   **Definition of Done:** La solución compila en limpio y las dependencias de proyectos respetan la regla de dependencia hacia adentro.

### EN-0.2: Aprovisionamiento de Infraestructura Local(David)
*   **Tipo:** Tarea Técnica.
*   **Descripción:** Configurar `docker-compose.yml` para levantar PostgreSQL con pgvector y un contenedor de Ollama. Evaluar la inclusión de un contenedor de Whisper para transcripción local.
*   **Definition of Done:** Un desarrollador puede ejecutar `docker compose up -d` y tener el entorno de datos e IA operativo.

### EN-0.3: CI/CD Pipeline Base(Juan)
*   **Tipo:** Tarea Técnica.
*   **Descripción:** Configurar un pipeline de GitHub Actions que compile la solución y ejecute pruebas unitarias en cada Pull Request.

### EN-0.4: Sistema de Diseño y Mockups(Bruno y Geronimo)
*   **Tipo:** Habilitador de Diseño.
*   **Descripción:** Establecer el Design System y generar los mockups de alta fidelidad para las pantallas críticas del MVP.
*   **Definition of Done:** Los mockups están aprobados por Producto y listos para implementación en el Sprint 1.

### EN-0.5: Persistencia (Roger y Edson)
*   **Tipo:** Habilitador de Persistencia.
*   **Descripción:** Establecer el model ER que soporte el MVP.
*   **Definition of Done:** Las entidas en un modelo ER en el Sprint 1.
---

## Epic 1: Identidad y Seguridad (Identity Context)

*Acceso seguro de profesores e ingreso sin fricciones de alumnos.*

### US-1.1: Autenticación de Instructores
*   **Como** Profesor,
*   **Quiero** iniciar sesión delegando la autenticación a un Identity Provider externo,
*   **Para** acceder a mis bancos de preguntas bajo un estándar seguro sin que el sistema gestione contraseñas.
*   **Criterios de Aceptación:**
    *   El sistema redirige al portal del IdP al hacer clic en "Login".
    *   Tras el redireccionamiento exitoso, la Web API valida el token JWT.
    *   El usuario tiene el claim de rol `instructor`.
*   **Nota:** Decisión pendiente entre IdP SaaS (Google/Microsoft) o Self-Hosted (Keycloak).

### US-1.2: Ingreso de Estudiantes
*   **Como** Estudiante,
*   **Quiero** unirme a una partida introduciendo un PIN y un apodo, o directamente si ya estoy autenticado en la plataforma,
*   **Para** competir en el juego con la menor fricción posible.
*   **Criterios de Aceptación:**
    *   El formulario de ingreso anónimo pide "PIN de Sala" (6 dígitos) y "Apodo" (máx. 15 caracteres).
    *   Si el PIN es válido y la sala está en estado `Waiting`, el sistema autoriza el WebSocket.
    *   Si el apodo ya existe en la sala, el sistema exige uno nuevo.
    *   Si el estudiante ya está autenticado, puede unirse directamente sin introducir apodo.

---

## Epic 2: Gestión de Contenido (Assessment Context)

*Curación de preguntas generadas por IA.*

### US-2.1: Curación de Preguntas AI-First
*   **Como** Profesor,
*   **Quiero** revisar una lista de preguntas sugeridas automáticamente por la IA basándose en la base de conocimiento de mis clases,
*   **Para** editarlas, aprobarlas o descartarlas, armando mi cuestionario sin escribir preguntas desde cero.
*   **Criterios de Aceptación:**
    *   El flujo principal no permite crear preguntas en blanco; todo nace de un borrador de IA.
    *   El profesor puede modificar el texto o las opciones antes de aprobar.
    *   Al aprobar, la entidad se transforma en una `Question` oficial lista para partidas.
    *   El sistema valida que haya exactamente 1 opción correcta y entre 1 y 3 distractores.

### US-2.2: Selección de Preguntas para una Partida
*   **Como** Profesor,
*   **Quiero** seleccionar un subconjunto de preguntas aprobadas de mi banco para incluirlas en una partida específica,
*   **Para** adaptar el contenido y la dificultad según el tema de la clase.
*   **Criterios de Aceptación:**
    *   El profesor puede marcar/desmarcar preguntas individuales de su banco aprobado.
    *   Se requiere un mínimo de 5 preguntas para crear una partida.
    *   El sistema muestra un contador de preguntas seleccionadas.

---

## Epic 3: Asistente de IA (AIKnowledge Context)

*Generación de contenido mediante inteligencia artificial local.*

### US-3.1: Ingesta de Documentos y Transcripción
*   **Como** Profesor,
*   **Quiero** subir un archivo PDF, texto, audio o video de mi clase,
*   **Para** que el sistema extraiga la transcripción textual y la prepare para análisis.
*   **Criterios de Aceptación:**
    *   El sistema acepta archivos `.pdf`, `.txt`, `.mp4` y `.mp3`.
    *   Si es audio/video, un Background Worker lo procesa usando Whisper.
    *   El estado del `TranscriptJob` pasa a `Processing`.

### US-3.2: Generación Automática de Preguntas
*   **Como** Sistema,
*   **Quiero** enviar fragmentos de texto al modelo LLM junto con un prompt de sistema estricto,
*   **Para** recibir un JSON estructurado con preguntas evaluativas basadas en el contexto provisto.
*   **Criterios de Aceptación:**
    *   El output de Ollama cumple con el JSON Schema predefinido.
    *   Si la IA rompe el esquema JSON, la Anti-Corruption Layer rechaza e intenta hasta 3 reintentos.
    *   Al finalizar, se dispara `TranscriptJobCompletedEvent` y los registros se guardan como `DraftQuestions`.

### US-3.3: Notificación y Monitoreo del Job de IA
*   **Como** Profesor,
*   **Quiero** ver el estado de mis trabajos de procesamiento (en cola, procesando, completado, fallido) y recibir una notificación cuando terminen,
*   **Para** saber cuándo mis preguntas están listas para revisión sin tener que refrescar la página.
*   **Criterios de Aceptación:**
    *   La interfaz muestra una lista de Jobs con su estado actual y timestamp de creación.
    *   Al completarse un Job, el servidor envía una notificación push al Profesor via SignalR.
    *   Si el Job falla, se muestra un mensaje de error descriptivo con la opción de reintentar.

---

## Epic 4: Motor de Gamificación en Vivo (LiveArena Context)

*Sincronización de dispositivos en tiempo real para la experiencia competitiva.*

### US-4.1: Creación de Sala
*   **Como** Profesor,
*   **Quiero** generar una sala de espera vinculada a las preguntas que seleccioné,
*   **Para** que mis estudiantes puedan unirse mediante un PIN y yo pueda ver quiénes están conectados.
*   **Criterios de Aceptación:**
    *   Se genera un PIN aleatorio único de 6 dígitos.
    *   El servidor inicializa el agregado `Match` en estado `Waiting`.
    *   La pantalla del profesor se actualiza vía SignalR cada vez que un alumno entra.

### US-4.2: Lanzamiento de Pregunta
*   **Como** Profesor,
*   **Quiero** presionar "Siguiente Pregunta",
*   **Para** que la pregunta aparezca simultáneamente en la pantalla principal y en los móviles de los alumnos.
*   **Criterios de Aceptación:**
    *   El estado del `Match` cambia a `QuestionActive`.
    *   SignalR envía `MatchQuestionActivatedEvent` a todos los clientes de la sala.
    *   Inicia el cronómetro del servidor.

### US-4.3: Recepción de Respuestas
*   **Como** Estudiante,
*   **Quiero** seleccionar una opción en mi dispositivo antes de que se agote el tiempo,
*   **Para** registrar mi intento en el servidor.
*   **Criterios de Aceptación:**
    *   El servidor rechaza peticiones si el estado no es `QuestionActive`.
    *   El servidor calcula la latencia basándose en su propio reloj interno, no en el del cliente.
    *   Una vez que el estudiante responde, su interfaz se bloquea hasta la siguiente ronda.

### US-4.4: Leaderboard y Cálculo de Puntajes
*   **Como** Sistema,
*   **Quiero** recalcular la tabla de posiciones al finalizar cada pregunta,
*   **Para** mostrar en la pantalla del profesor el ranking de estudiantes basado en respuestas correctas y velocidad.
*   **Criterios de Aceptación:**
    *   Puntaje = Correctitud * Bonus por rapidez.
    *   El evento `PlayerAnswerReceivedEvent` recalcula el Leaderboard en memoria.
    *   Cuando el tiempo se acaba, SignalR envía el Leaderboard actualizado al Profesor y los alumnos ven "Correcto/Incorrecto".

### US-4.5: Finalización y Resumen de Partida
*   **Como** Profesor,
*   **Quiero** que al agotarse la última pregunta se presente un resumen final de la partida,
*   **Para** revisar el desempeño de la clase y cerrar la sesión de juego.
*   **Criterios de Aceptación:**
    *   Al lanzar la última pregunta y finalizar su temporizador, el estado del `Match` cambia a `Finished`.
    *   El sistema persiste los resultados finales en la base de datos.
    *   La pantalla del profesor muestra el podio final (Top 3) y estadísticas de la partida (% de acierto global, pregunta más difícil).
    *   Se cierran todas las conexiones WebSocket de la sala.
