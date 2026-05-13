# Definición de Minimum Viable Product (MVP): QuizArena

**Proyecto:** QuizArena  
**Clasificación:** Definición de Producto / Lean Startup  

Este documento define el alcance estricto de la primera versión funcional del producto. Su propósito es establecer las hipótesis de negocio a validar minimizando la inversión de recursos de desarrollo.

---

## 1. Propuestas de Valor e Hipótesis a Validar

1.  **Hipótesis de Instructor:** Los profesores ahorrarán tiempo significativo al delegar la generación de preguntas a una IA que procesa directamente las grabaciones de sus clases.
2.  **Hipótesis de Estudiante:** La atención y retención del estudiante aumentan cuando la evaluación se transforma en una competencia en vivo interactiva a través de sus dispositivos móviles.

---

## 2. Alcance Funcional

### A. LiveArena
*   Sincronización de eventos en tiempo real mediante SignalR.
*   El Profesor actúa como Host, controlando el flujo del juego.
*   Los Estudiantes ingresan a una Sala usando un PIN autogenerado y un apodo. Si ya están autenticados en la plataforma, ingresan directamente.
*   El cálculo de puntajes se basa en la correctitud de la respuesta y la velocidad.

### B. AIKnowledge
*   Integración con Ollama para generación de preguntas.
*   El Profesor puede subir archivos de video, audio, PDF o texto plano de sus clases.
*   Transcripción local mediante Whisper para extraer el texto del contenido multimedia y usarlo como contexto para la generación de preguntas.

### C. Assessment (AI-First)
*   Las preguntas nacen exclusivamente como borradores generados por la IA.
*   El Profesor actúa como curador: revisa, edita, aprueba o descarta cada borrador antes de incorporarlo al banco de preguntas oficial.

### D. Identidad y Acceso
*   Autenticación delegada a un Identity Provider externo mediante OIDC.
*   Los Estudiantes pueden unirse a partidas como invitados anónimos mediante PIN, reduciendo la fricción de entrada.

---

## 3. Fuera de Alcance (V2)

*   **Broadcasting Context (WebRTC):** No habrá streaming de video nativo. Se asume que el profesor utilizará herramientas externas para la clase en vivo y QuizArena como complemento de gamificación.
*   **Overlay Quizzes:** Sin reproductor de video nativo, no se implementarán interfaces superpuestas sobre un flujo multimedia.
*   **Exámenes Asíncronos:** El MVP se enfoca exclusivamente en la competencia multijugador síncrona.

---

## 4. Métricas de Éxito

1.  **AI Adoption Rate:** Porcentaje de preguntas generadas por IA versus las editadas manualmente desde cero.
2.  **LiveArena Engagement:** Porcentaje de alumnos conectados a una sala que completan el 100% del cuestionario.

---

## 5. Apéndice: Notas de Viabilidad Técnica

Para garantizar la ejecución bajo una política de costo cercano a cero, se determinaron los siguientes límites en servidores básicos:

*   **SignalR:** Capacidad para sostener 500 alumnos simultáneos (10 clases de 50 estudiantes) con ~50 MB de RAM.
*   **PostgreSQL:** Absorción de picos de hasta 500 escrituras por segundo sin cachés intermedios.
*   **Procesamiento Multimedia:** La transcripción con Whisper sumada a la inferencia de Ollama requiere idealmente 8GB+ de RAM/VRAM. El tiempo de procesamiento asíncrono de un video corto ronda entre 2 y 5 minutos, operando mediante Background Workers.
