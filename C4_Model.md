# Modelo C4: QuizArena

Modelo de arquitectura a diferentes niveles de abstracción. El Nivel 1 identifica los actores y sistemas externos. El Nivel 2 desglosa la infraestructura técnica desplegable.

---

## 1. Diagrama de Contexto (Nivel 1)

```plantuml
@startuml
!include <C4/C4_Context>

title Diagrama de Contexto de Sistema (Nivel 1) - QuizArena

Person(instructor, "Instructor", "Crea contenido, sube grabaciones de clase y lanza partidas de trivia en vivo.")
Person(estudiante, "Estudiante", "Compite en trivias en tiempo real desde su dispositivo móvil.")
System(quizarena, "QuizArena", "Plataforma educativa: Gamificación en vivo con generación de contenido por IA.")
System_Ext(ollama, "Ollama", "Servidor local de inferencia LLM.")
System_Ext(whisper, "Whisper", "Motor local de transcripción Speech-to-Text.")
System_Ext(idp, "Identity Provider", "Servicio externo de identidad (OIDC).")

Rel(instructor, quizarena, "Gestiona contenido y lanza partidas", "HTTPS / WSS")
Rel(estudiante, quizarena, "Compite en vivo", "HTTPS / WSS")
Rel(instructor, idp, "Se autentica")
Rel(estudiante, idp, "Se autentica (opcional, puede ingresar como invitado con PIN)")
Rel(quizarena, idp, "Verifica tokens JWT", "HTTPS / JWKS")
Rel(quizarena, ollama, "Solicita generación de preguntas", "HTTP REST")
Rel(quizarena, whisper, "Solicita transcripción de audio/video", "HTTP / CLI")
@enduml
```

---

## 2. Diagrama de Contenedores (Nivel 2)

```plantuml
@startuml
!include <C4/C4_Container>

title Diagrama de Contenedores (Nivel 2) - QuizArena

Person(usuario, "Usuario", "Instructor o Estudiante interactuando vía navegador.")
System_Ext(ollama, "Ollama", "Generador de IA")
System_Ext(whisper, "Whisper", "Transcripción Speech-to-Text")
System_Ext(idp, "Identity Provider", "OAuth2 / OIDC")

System_Boundary(c1, "Plataforma QuizArena") {
    Container(spa, "Single Page Application", "Angular 17+", "Provee todas las interfaces de usuario.")
    Container(webapi, "Monolito Web API", ".NET 8 ASP.NET Core", "Expone APIs REST, aloja el Hub SignalR e implementa Screaming Architecture.")
    ContainerDb(db, "Base de Datos Principal", "PostgreSQL + pgvector", "Almacena perfiles, preguntas, base de conocimiento vectorizada y métricas de partidas.")
    Container(storage, "Almacenamiento Local", "Local Disk", "Persiste archivos multimedia subidos por los instructores.")
}

Rel(usuario, spa, "Visita y visualiza", "HTTPS")
Rel(spa, idp, "Redirige para Login y obtiene JWT", "HTTPS")
Rel(spa, webapi, "Envía peticiones con JWT Bearer", "HTTPS / WSS")
Rel(webapi, idp, "Descarga llaves públicas para validar firmas", "HTTPS")
Rel(webapi, db, "Lee y escribe estado", "EF Core")
Rel(webapi, storage, "Guarda archivos pesados", "I/O")
Rel(webapi, ollama, "Envía prompts y recibe JSON", "HTTP REST")
Rel(webapi, whisper, "Envía archivos multimedia y recibe transcripción", "HTTP / CLI")
@enduml
```

---

## 3. Flujos Críticos Revelados por el Nivel 2

1. **Pipeline de IA Asíncrono:**
   El contenedor Web API recibe un archivo multimedia del SPA, lo guarda en Storage, lo envía a Whisper para transcripción, y finalmente alimenta a Ollama con el texto resultante. Todo este flujo es asíncrono y gestionado mediante Background Workers para no bloquear las partidas en vivo.

2. **Cuello de Botella en el Disco:**
   Si múltiples instructores suben grabaciones simultáneamente, el disco local sufrirá saturación I/O. Se debe considerar almacenamiento externo para V2.

3. **Persistencia Transaccional Estricta:**
   Todos los eventos del juego pasan por la Web API hacia la base de datos. El cliente nunca escribe directamente ni calcula puntos; la Web API es el árbitro absoluto del tiempo.
