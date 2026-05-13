# Product Requirements Document (PRD)
**Producto:** QuizArena  
**Estado:** Activo / En Diseño  
**Fecha:** Mayo 2026  

> **Nota:** Para el alcance del MVP, consultar `MVP_Scope.md`. Las Épicas marcadas con `[V2]` están planificadas para versiones posteriores.

---

## 1. Problema y Objetivo

La educación remota enfrenta un desafío constante: mantener la atención del estudiante. Las clases en formato de pantalla compartida suelen ser unidireccionales y pasivas, provocando una rápida pérdida de interés.

**Objetivo del Producto:** QuizArena transforma el aprendizaje en una experiencia interactiva y competitiva. La plataforma permite a los estudiantes repasar conceptos mediante partidas de trivia en tiempo real, y le entrega a los instructores un ecosistema para transmitir sus clases en vivo. El sistema utiliza Inteligencia Artificial para procesar grabaciones de clase y generar cuestionarios automáticamente.

---

## 2. Usuarios Objetivo

- **Instructores / Profesores:** Buscan herramientas para evaluar la atención de su clase sin invertir horas redactando cuestionarios.
- **Estudiantes / Jugadores:** Buscan formas dinámicas y entretenidas de estudiar o repasar contenido, compitiendo de manera amistosa con sus compañeros.

---

## 3. Historias de Usuario y Funcionalidades Clave

### Epic 1: Autenticación y Acceso `[MVP]`
- **User Story:** Como usuario, quiero iniciar sesión con un proveedor de identidad externo para acceder a la plataforma de forma segura sin gestionar contraseñas.
- **Criterios de Aceptación:**
  - Flujo de login delegado a un Identity Provider mediante OIDC.
  - El sistema diferencia los roles de Instructor y Estudiante mediante claims JWT.
  - Los estudiantes pueden ingresar a partidas en vivo como invitados anónimos mediante PIN, sin necesidad de registro previo.

### Epic 2: Gestión de Contenido `[MVP]`
- **User Story:** Como instructor, quiero curar las preguntas generadas por la IA para armar cuestionarios de calidad sin escribirlos desde cero.
- **Criterios de Aceptación:**
  - Las preguntas nacen exclusivamente como borradores generados por la IA.
  - El instructor puede editar, aprobar o descartar cada borrador antes de añadirlo al banco oficial.
  - Cada pregunta aprobada debe tener exactamente 1 respuesta correcta y entre 1 y 3 distractores.

### Epic 3: Arena Multijugador `[MVP]`
- **User Story:** Como estudiante, quiero unirme a una sala de trivia en tiempo real para competir con mis compañeros y ver quién domina mejor un tema.
- **Criterios de Aceptación:**
  - Creación de sala mediante un código PIN de invitación.
  - El servidor garantiza que todos los jugadores reciban la pregunta en el mismo instante.
  - El sistema recompensa con más puntos a quienes respondan correctamente en menor tiempo.
  - Al concluir cada pregunta, se despliega una tabla de posiciones actualizada.

### Epic 4: Base de Conocimiento y Generación por IA `[MVP]`
- **User Story:** Como instructor, quiero procesar una grabación de mi clase para generar múltiples cuestionarios bajo demanda.
- **Criterios de Aceptación:**
  - El sistema procesa archivos multimedia, extrae la transcripción y almacena el contexto de forma permanente.
  - El instructor puede solicitar la generación de N preguntas basándose en un contexto previamente guardado.
  - El LLM devuelve las preguntas en formato JSON estructurado, cada una con distractores plausibles y la respuesta correcta.
  - Las preguntas generadas entran en estado "Borrador" para revisión humana obligatoria.

### Epic 5: Transmisión de Clase en Vivo `[V2]`
- **User Story:** Como instructor, quiero impartir mi clase compartiendo mi pantalla directamente desde QuizArena, sin depender de aplicaciones externas.
- **Criterios de Aceptación:**
  - Integración nativa de transmisión de video desde el navegador del profesor hacia los clientes.
  - Baja latencia y sincronía de audio en el reproductor del estudiante.

### Epic 6: Overlay de Quizzes en Vivo `[V2]`
- **User Story:** Como instructor, quiero lanzar una pregunta sorpresa durante mi transmisión para evaluar la atención, sin que el alumno cambie de pestaña.
- **Criterios de Aceptación:**
  - Panel lateral con preguntas preparadas o generadas previamente por IA.
  - Al lanzar, el cliente del alumno oscurece el video y superpone el quiz en el centro de la pantalla.
  - El video sigue corriendo de fondo. Una vez que el alumno responde, el overlay desaparece.

---

## 4. Reglas de Negocio

- **Trazabilidad de Respuestas:** El sistema registra de forma auditable la opción seleccionada por cada estudiante y el tiempo de respuesta con precisión de milisegundos.
- **Gobernanza del Estado:** El ciclo de vida de una partida es administrado exclusivamente por el servidor. El producto no confía en el dispositivo del usuario para calcular tiempos ni decidir si un quiz terminó.
- **Historial de Retención:** Las estadísticas acumuladas de los jugadores se preservan de manera indefinida para alimentar el perfil del usuario.

---

## 5. Fuera de Alcance

- **Integraciones con ecosistemas cerrados:** No se desarrollarán plugins para Microsoft Teams o Zoom. La plataforma es un destino independiente.
- **Escalamiento masivo global:** La concurrencia está optimizada para el tamaño típico de una clase (5 a 150 usuarios por sala), no para torneos de decenas de miles de usuarios simultáneos.

---

## 6. Requisitos No Funcionales

- **Server-Authoritative:** Ningún cálculo de tiempo, validación de respuesta o transición de estado puede depender del reloj del cliente. El backend tiene la verdad absoluta.
- **Abstracción de Infraestructura:** El código de gestión de archivos debe estar desacoplado mediante interfaces, permitiendo intercambiar el almacenamiento local por S3 o Azure Blob sin impacto en la lógica de negocio.
- **Latencia de Mensajería:** El Hub de conexiones en tiempo real debe procesar el evento "Lanzar Quiz" hacia todos los alumnos en menos de 200 milisegundos.
- **Tolerancia a Fallos de IA:** Si el LLM falla o devuelve un formato no parseable, el sistema captura la excepción, alerta al instructor y aborta la importación sin corromper la base de datos.
