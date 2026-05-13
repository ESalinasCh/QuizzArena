# QuizArena: Engineering Standards & Guidelines

**Proyecto:** QuizArena  
**Propósito:** Reglas, metodologías y estándares obligatorios para todos los desarrolladores que contribuyan al código fuente.

---

## 1. Flujo de Trabajo y Colaboración

### A. Gestión del Código y Tareas
*   **Plataforma:** Todo el código, documentación y ciclo de vida reside en **GitHub**.
*   **Gestión de Tareas:** Epics y Bugs se gestionan mediante **GitHub Issues** y GitHub Projects. Cada Pull Request debe referenciar el Issue que resuelve.

### B. Estrategia de Ramas
*   **Modelo:** **GitFlow**.
*   **Ramas Permanentes:** `main` (producción) y `develop` (integración).
*   **Ramas Efímeras:** Todo código nuevo nace de `develop` en una rama prefijada: `feature/*`, `bugfix/*`, o desde `main` como `hotfix/*`.

### C. Commits
*   **Estándar:** **Conventional Commits**.
*   **Formato:** `feat: agregar login por google`, `fix: corregir cálculo de latencia`, `chore: actualizar versión de EF Core`.
*   Permite autogenerar el `CHANGELOG.md` e inyectar versionamiento semántico en los pipelines.

### D. Integración Continua
*   **Motor:** **GitHub Actions**.
*   Se prohíben los commits directos a `main` y `develop`. Todo ingreso debe ser por Pull Request.
*   Cada PR dispara un Action que compila la solución, verifica el linting y ejecuta las pruebas unitarias. Si falla, el PR no se puede fusionar.

---

## 2. Estándares Frontend

### A. Arquitectura Estructural
*   **Paradigma:** **Feature-Driven Architecture**.
*   El código se agrupa por dominio de negocio, no por tipo técnico. No debe existir una carpeta gigante `/components` global. Cada feature encapsula sus propios componentes, lógica de estado y llamadas a API.

### B. Diseño de Componentes
*   **Paradigma:** **Atomic Design** para el Design System compartido.
*   **Átomos:** Botones, inputs, tipografía.
*   **Moléculas:** Barra de búsqueda, campos de formulario compuestos.
*   **Organismos:** Formularios completos, barras de navegación.
*   **Plantillas y Páginas:** Vistas finales acopladas a la lógica de ruta.

### C. Metodología CSS
*   **Estándar:** **BEM** (Block, Element, Modifier).
*   Todas las clases deben seguir el formato `.block__element--modifier` para evitar colisiones de estilos globales.

**Estructura de Carpetas:**
```text
src/
│
├── 📁 app/                    Ruteo global y configuración
│
├── 📁 shared/                 Design System reutilizable
│   ├── 📁 ui/
│   │   ├── 📁 atoms/          Button, Input, Icon
│   │   ├── 📁 molecules/      SearchBar, FormField
│   │   └── 📁 organisms/      Navbar, GlobalModal
│   └── 📁 styles/             Variables CSS, utilidades BEM
│
├── 📁 features/               Código agrupado por dominio
│   ├── 📁 LiveArena/
│   │   ├── 📁 components/     Leaderboard, AnswerButtons
│   │   ├── 📁 api/            Servicio de conexión SignalR
│   │   └── 📁 store/          Estado de la partida
│   │
│   ├── 📁 Assessment/
│   │   ├── 📁 components/     DraftQuestionEditor
│   │   └── 📁 api/            Llamadas REST a la API
│   │
│   └── 📁 AIKnowledge/        Subida de videos y monitoreo de jobs
│
└── 📁 pages/                  Páginas finales atadas a rutas
    ├── InstructorDashboardPage
    ├── MatchLobbyPage
    └── StudentPinEntryPage
```

---

## 3. Estándares Backend y Contratos API

### A. Diseño de Contratos
*   **Estándar:** **OpenAPI 3.0**.
*   Antes de escribir la lógica del controlador, se debe diseñar el endpoint en OpenAPI. Esto permite a los desarrolladores de Frontend generar sus clientes HTTP tipados de forma automática.

### B. Seguridad e Identidad
*   **Estándar:** **OAuth2.0 + OpenID Connect**.
*   El backend nunca manejará contraseñas. Toda autenticación se delega a un Identity Provider. El backend únicamente valida la firma digital de los tokens JWT.

### C. Estructura y Arquitectura Backend
*   **Paradigma Macro (Monolito Modular + Screaming Architecture):** La solución está dividida en proyectos físicos que representan los Bounded Contexts. Está prohibido referenciar la base de datos de un contexto desde otro; la comunicación inter-módulo se realiza mediante Eventos de Dominio asíncronos con MediatR. Los nombres de los proyectos reflejan la intención del negocio.
*   **Paradigma Micro (Arquitectura Hexagonal):** Dentro de cada módulo, el código se aísla mediante Puertos y Adaptadores:
    *   **Domain Layer:** Entidades y reglas de negocio puras. Cero dependencias hacia frameworks externos.
    *   **Application Layer:** Casos de uso y definición de interfaces.
    *   **Infrastructure Layer:** Implementación de las interfaces: repositorios de EF Core, integraciones HTTP hacia Ollama.

**Estructura de Carpetas:**
```text
QuizArena.sln
│
├── 📁 QuizArena.SharedKernel     Value Objects compartidos (UserId)
├── 📁 QuizArena.Identity         Validación JWT y autorización
├── 📁 QuizArena.Assessment       Curación de preguntas AI-First
│   ├── 📁 Domain                 Entidad Question, reglas de negocio
│   ├── 📁 Application            Comandos, Queries, Interfaces
│   └── 📁 Infrastructure         Repositorios EF Core
│
├── 📁 QuizArena.LiveArena        Motor del juego en vivo
├── 📁 QuizArena.AIKnowledge      Orquestación de Whisper y Ollama
│
└── 📁 QuizArena.Api              Ejecutable ASP.NET que compone los módulos
```

---

## 4. Calidad de Código y Observabilidad

### A. Estrategia de Pruebas
*   **Backend:** Pruebas unitarias obligatorias en la capa de dominio usando `xUnit` y `Moq`/`NSubstitute`. Pruebas de integración con **Testcontainers** para levantar un PostgreSQL real efímero. Prohibido el uso del proveedor In-Memory de EF Core.
*   **Frontend:** Pruebas unitarias de componentes con `Jest` o `Vitest`.

### B. Formateo y Linting
*   Cero debates de estilo en los Code Reviews. El formateo lo dicta la máquina.
*   **Backend:** `.editorconfig` en la raíz. El pipeline CI/CD rechaza el PR si `dotnet format` detecta infracciones.
*   **Frontend:** **Prettier** para formateo y **ESLint** para análisis estático.

### C. Manejo Global de Errores
*   **Estándar:** **RFC 7807 (Problem Details for HTTP APIs)**.
*   Prohibido devolver excepciones directas al cliente. Un Middleware intercepta las excepciones y devuelve un JSON estandarizado con `type`, `title`, `status` y `detail`.

### D. Logging Estructurado
*   **Herramienta:** **Serilog**.
*   Los mensajes de log deben escribirse como plantillas de propiedades estructuradas para que los sistemas de monitoreo puedan indexar y filtrar por variables.
