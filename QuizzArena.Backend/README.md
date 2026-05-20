# 🏆 QuizzArena: Advanced Modular Architecture Guide


This repository hosts the **QuizzArena Backend**, a high-performance system built with **.NET 10**. The project is architected as a **Modular Monolith**, strictly adhering to **Hexagonal Architecture (Ports & Adapters)**, **Domain-Driven Design (DDD)** principles, and the **Screaming Architecture** methodology.


## 🏛️ Architectural Manifesto

The primary goal of this architecture is **Independence**. By decoupling the business logic from the infrastructure, the system becomes:
- **Testable:** Logic can be tested without a database or web server.
- **Maintainable:** Changes in the UI or Database don't affect business rules.
- **Scalable:** Modules can be extracted into Microservices with zero friction.

---

## 📂 Comprehensive Directory Breakdown


```
├── Host/
└── Modules/
    └── MyModule/
        ├── Domain/                         
        │   ├── Entities/                              
        │   ├── Enums/                      
        │   ├── Exceptions/
        │   └── ValueObjects/
        ├── Application/                    
        │   ├── Ports/
        │   │   ├── In/
        │   │   └── Out/
        │   ├── UseCases/
        │   ├── DTOs/
        │   ├── Validators/
        │   └── Mappers/ 
        └── Infrastructure/                 
            └── Adapters/
                ├── In/
                │   └── Web/
                └── Out/
                    ├── ExternalServices/
                    └── Persistence/
```

### 🛠️ 1. The Host (Composition Root)
The `Host/` directory is the only executable project. It serves as the **Orchestrator**.
- **Responsibilities:** Imports the modules' extension methods to compose the application and merges modular routes into a single API surface. The Host no longer performs direct dependency injection configuration; all DI is handled within each module.

---

### 📦 2. The Modules (`Modules/XModule/`)
Each module represents a bounded context (e.g., Quizzing, Users).

#### 🔴 A. Domain Layer (The Pure Heart)
*Located at `Modules/XModule/Domain/`*
This layer has **zero dependencies**. It contains the "Truth" of the business.
- **Entities:** Objects with a unique identity that persist over time (e.g., `Quiz.cs`).
- **Value Objects:** Immutable objects defined by their attributes rather than identity (e.g., `Score.cs`, `Address.cs`).
- **Enums:** Strongly typed constants representing domain states (e.g., `QuizStatus.Published`).
- **Exceptions:** Custom, domain-specific errors (e.g., `InvalidQuestionFormatException`) that prevent the system from entering an invalid state.

#### 🟢 B. Application Layer (The Orchestrator)
*Located at `Modules/XModule/Application/`*
This layer implements the **Use Cases**. It tells the domain what to do.
- **UseCases:** Commands and logic that satisfy a user requirement (e.g., `SubmitQuizHandler.cs`).
- **DTOs (Data Transfer Objects):** Simple contracts for input/output. They ensure the Domain Entities never leak to the outside world.
- **Mappers:** Responsible for the transformation between Entities and DTOs (using AutoMapper or manual mapping).
- **Validators:** Input validation logic, typically implemented using FluentValidation, to ensure data integrity before processing use cases.
- **Ports (The Hexagon's Boundary):**
    - **Inbound Ports:** Interfaces defining the "Entry Points" for the application.
    - **Outbound Ports:** Interfaces defining what the application needs from the infrastructure (e.g., `IUserRepository`, `IEmailService`).

#### 🔵 C. Infrastructure Layer (The Technical Detail)
*Located at `Modules/XModule/Infrastructure/`*
This layer handles the "How" (How we store data, How we send messages).
- **Adapters In (Web):**
    - **Web API:** ASP.NET Core Controllers. They translate HTTP requests into Application Use Cases.
- **Adapters Out (Persistence):**
    - **Persistence:** EF Core implementations, Migrations, and Repository concrete classes (e.g., `SqlQuizRepository.cs`).
    - **External Services:** Implementations of shared contracts/interfaces that are used across modules (e.g., `IUsersContract`).

---

## 🚀 The Data Lifecycle (Step-by-Step)

1. **The Entry:** An HTTP request hits a Controller in `Infrastructure/Adapters/In/Web`.
2. **The Translation:** The Controller maps the request to a **DTO**.
3. **The Execution:** The Controller calls a **UseCase** in the `Application` layer.
4. **The Logic:** The UseCase fetches an **Entity** from the `Domain` using a **Port Out** (Interface).
5. **The Persistence:** The **Infrastructure Adapter Out** executes the SQL query in **PostgreSQL**.
6. **The Result:** The UseCase processes the logic, the **Mapper** converts the Entity back to a DTO, and the Controller returns a `200 OK`.

---


## 🔗 Shared Module & Inter-Module Communication

### Purpose of the Shared Module
The **Shared** module acts as a contract boundary for cross-module communication. It contains interfaces, DTOs, and contracts that define how different modules interact without creating direct dependencies between their internal implementations.

### How Communication Works
- **Decoupling via Contracts:** Instead of referencing each other's internal classes, modules depend on interfaces defined in the Shared module (e.g., `IUsersContract`).
- **Implementation in Infrastructure:** Each module provides its own implementation of the shared contracts in its Infrastructure layer (e.g., `UsersContractImpl` in the Users module).
- **Dependency Injection:** The module registers its implementation with the DI container, so when another module requests the contract, it receives the correct implementation.
- **Usage Example:**
    - The Quizzing module needs to check if a user is a professor. It depends on the `IUsersContract` interface from Shared, not on the Users module directly.
    - The Users module implements `IUsersContract` and registers it in its DI setup.
    - At runtime, the DI container injects the Users module's implementation wherever `IUsersContract` is required, enabling safe, decoupled communication.

This approach ensures that modules remain independent, testable, and maintainable, while still enabling rich interactions across bounded contexts.

---
## 💎 Design Patterns & Principles Applied

| Pattern | Benefit |
| :--- | :--- |
| **Encapsulation** | Value Objects and Entities protect the business data from invalid states. |
| **Single Responsibility (SRP)** | Mappers handle data shape, Repositories handle data storage, UseCases handle logic. |
| **Screaming Architecture** | Just by looking at the folders, you know this is a Quizzing system. |

---


## 📦 Pre-installed Packages Per Module

Each module comes with the following NuGet packages ready to use:

- **Npgsql.EntityFrameworkCore.PostgreSQL** — PostgreSQL provider for Entity Framework Core
- **Microsoft.EntityFrameworkCore.Design** — Design-time utilities for Entity Framework Core
- **AutoMapper** — Object-object mapping for DTOs and entities
- **FluentValidation** — Powerful validation library for .NET
- **Asp.Versioning.Mvc** — API versioning support for ASP.NET Core MVC


- **Runtime:** .NET 10.0 (Latest Release)
- **Database:** PostgreSQL + pgvector (for vector similarity search)
- **ORM:** Entity Framework Core
- **Architecture:** Modular Hexagonal Monolith
- **Communication:** RESTful API

---
