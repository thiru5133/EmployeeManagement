# Employee Management Web API

A **.NET 10 ASP.NET Core Web API** built as part of the **Week 2 .NET Engineering Bridge Training Program**.

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Project Structure](#project-structure)
- [Day 6 — DTOs, Validation & REST API Basics](#day-6--what-we-did)
- [Day 7 — Dependency Injection & Middleware](#day-7--dependency-injection--middleware)
- [Day 8 — Layered Architecture & Central Exception Handling](#day-8--layered-architecture--central-exception-handling)
- [Day 9 — EF Core, Relationships & Pagination](#day-9--ef-core-relationships--pagination)
- [Day 10 — Authentication, Authorization & Security (Gate 2)](#day-10--authentication-authorization--security-gate-2)
- [API Endpoints](#api-endpoints)
- [Running the Project](#running-the-project)
- [Running Tests](#running-tests)
- [Key Concepts Learned](#key-concepts-learned)

---

## Project Overview

This project is an **Employee Management REST API** that demonstrates core ASP.NET Core concepts including:

- RESTful API design with proper HTTP verbs and status codes
- Data Transfer Objects (DTOs) with validation annotations
- Swagger/OpenAPI documentation
- xUnit-based unit testing

> **Note:** Data is currently stored in-memory using a `static List<Employee>`. A real database (EF Core + SQL) will be integrated in Day 9.

---

## Project Structure

```
EmployeeManagement/
├── EmployeeManagement.slnx              # Solution file
│
├── EmployeeManagement.API/              # Main Web API project
│   ├── Controllers/
│   │   └── EmployeeController.cs        # CRUD endpoints for Employee
│   ├── DTOs/                            # ✨ Added in Day 6
│   │   ├── CreateEmployeeDto.cs         # Input DTO for POST with validation
│   │   ├── UpdateEmployeeDto.cs         # Input DTO for PUT with validation
│   │   └── EmployeeResponseDto.cs       # Output DTO for all GET/POST responses
│   ├── Models/
│   │   └── Employee.cs                  # Core Employee domain entity
│   ├── Program.cs                       # App bootstrap & middleware pipeline
│   ├── appsettings.json                 # App configuration
│   └── EmployeeManagement.API.http      # ✨ Expanded in Day 6 — 10 test scenarios
│
└── EmployeeManagement.Tests/            # ✨ Added in Day 6 — xUnit test project
    ├── EmployeeControllerTests.cs       # 7 controller unit tests (HTTP status codes)
    └── EmployeeValidationTests.cs       # 10 DTO validation unit tests
```

---

## Day 6 — What We Did

### 1. DTOs & Model Validation

**Problem before:** The controller was directly binding HTTP request bodies to the `Employee` entity, exposing internal fields (like `Id`) and having zero validation.

**Solution:** Introduced a proper **DTO layer** with three classes:

#### `CreateEmployeeDto` — used as the `POST` request body
```csharp
public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Department { get; set; }

    [Range(1000, 1000000, ErrorMessage = "Salary must be between 1,000 and 1,000,000")]
    public decimal Salary { get; set; }

    [Required]
    public DateTime JoiningDate { get; set; }
}
```

#### `UpdateEmployeeDto` — used as the `PUT` request body
Same validations as `CreateEmployeeDto`. The `Id` is NOT included — it comes from the route (`/api/employee/{id}`), never the body.

#### `EmployeeResponseDto` — returned in all responses
Contains `Id`, `Name`, `Email`, `Department`, `Salary`, `JoiningDate`. This decouples what clients see from the internal domain model.

**Why DTOs matter:**
| Without DTOs | With DTOs |
|---|---|
| Client can send/receive internal fields (e.g. `Id` on create) | Full control over input/output shape |
| No validation — garbage in, garbage out | `[ApiController]` automatically returns `400` for invalid input |
| Breaking your API means changing your Entity | Entity and API contract evolve independently |

---

### 2. Updated Controller

[`EmployeeController.cs`](EmployeeManagement.API/Controllers/EmployeeController.cs) was updated to:

- Accept **DTOs** as input (`[FromBody] CreateEmployeeDto`, `[FromBody] UpdateEmployeeDto`)
- Return **DTOs** as output (`EmployeeResponseDto`) — never the raw `Employee` entity
- Added **`[ProducesResponseType]`** attributes on every action so Swagger shows exact response shapes and status codes
- Added a private `MapToResponseDto()` helper method for clean entity → DTO mapping

```csharp
// Before (Day 5)
public ActionResult<Employee> CreateEmployee([FromBody] Employee employee)

// After (Day 6)
public ActionResult<EmployeeResponseDto> CreateEmployee([FromBody] CreateEmployeeDto dto)
```

---

### 3. HTTP Test File

[`EmployeeManagement.API.http`](EmployeeManagement.API/EmployeeManagement.API.http) now covers **10 scenarios** (open in Visual Studio and click ▶ to run):

| # | Request | Expected Status |
|---|---------|----------------|
| 1 | `GET /api/employee` | `200 OK` |
| 2 | `GET /api/employee/1` | `200 OK` |
| 3 | `GET /api/employee/999` | `404 Not Found` |
| 4 | `POST /api/employee` — valid body | `201 Created` |
| 5 | `POST /api/employee` — invalid body (empty name, bad email, bad salary) | `400 Bad Request` |
| 6 | `PUT /api/employee/1` — valid body | `204 No Content` |
| 7 | `PUT /api/employee/999` — non-existent ID | `404 Not Found` |
| 8 | `PUT /api/employee/1` — validation failure | `400 Bad Request` |
| 9 | `DELETE /api/employee/2` | `204 No Content` |
| 10 | `DELETE /api/employee/999` — non-existent ID | `404 Not Found` |

---

### 4. Unit Tests

A new **`EmployeeManagement.Tests`** xUnit project was added with **17 tests** across 2 files:

#### `EmployeeValidationTests.cs` — 10 tests
Tests that Data Annotation validations work correctly on DTOs using `Validator.TryValidateObject`:

| Test | Checks |
|---|---|
| `CreateEmployeeDto_WithValidData_PassesValidation` | A fully valid DTO passes with zero errors |
| `CreateEmployeeDto_WithMissingOrEmptyName_FailsValidation` | Theory: `""`, `" "`, `null` all fail `[Required]` / `[StringLength]` |
| `CreateEmployeeDto_WithInvalidEmail_FailsValidation` | Theory: `"not-an-email"`, `"nodomain"`, `"@nodomain.com"` fail `[EmailAddress]` |
| `CreateEmployeeDto_WithSalaryOutOfRange_FailsValidation` | Theory: `500`, `-100`, `2000000` all fail `[Range(1000, 1000000)]` |

#### `EmployeeControllerTests.cs` — 7 tests
Tests that the controller returns the correct HTTP status codes and response shapes:

| Test | Verifies |
|---|---|
| `GetEmployees_ReturnsOkResult` | `200 OK` with non-null body |
| `GetEmployeeById_WithValidId_ReturnsOkResult` | `200 OK`, correct `Id` returned |
| `GetEmployeeById_WithNonExistentId_Returns404NotFound` | `404 Not Found` |
| `CreateEmployee_WithValidDto_Returns201Created` | `201 Created`, body contains `Id > 0` and correct fields |
| `UpdateEmployee_WithValidDto_Returns204NoContent` | `204 No Content` |
| `UpdateEmployee_WithNonExistentId_Returns404NotFound` | `404 Not Found` |
| `DeleteEmployee_WithNonExistentId_Returns404NotFound` | `404 Not Found` |

**Test results:**
```
Total tests:  17
Passed:       17
Failed:        0
Time:         ~0.7s
```

> ⚠️ **Known limitation (fixed in Day 8):** The controller uses a `static` in-memory list which is shared across test instances. Currently tests avoid asserting mutable values (e.g. Name after an Update test runs). This will be properly isolated once we introduce the Service layer with Dependency Injection in Day 7/8.

---

## Day 7 — Dependency Injection & Middleware

- **Service Layer Separation**: Extracted business operations into `IEmployeeService` and concrete `EmployeeService`.
- **DI Container Lifetimes**:
  - `AddScoped`: One instance per client request (used for `IEmployeeService` and DbContext).
  - `AddTransient`: Fresh instance per injection.
  - `AddSingleton`: Single application-wide instance.
  - Avoided captive dependencies (never inject Scoped into Singleton).
- **Custom Logging Middleware**: `RequestLoggingMiddleware` inspects request method, route, and records response time in ms with appropriate log levels.
- **Mocking with NSubstitute**: Unit tests in `EmployeeControllerTests` isolated with mocks instead of shared in-memory lists.

---

## Day 8 — Layered Architecture & Central Exception Handling

### 1. Layered Architecture & Separation of Concerns

The solution clearly separates HTTP concerns, business logic, and data persistence:

```
┌────────────────────────────────────────────────────────┐
│                   HTTP Request                         │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────┐
│  Presentation Layer: Controllers (EmployeeController)  │
│  - HTTP Status Codes, Route Binding, Model Validation  │
└──────────────────────────┬─────────────────────────────┘
                           │ calls IEmployeeService
                           ▼
┌────────────────────────────────────────────────────────┐
│  Application Layer: Services (EmployeeService)         │
│  - Business logic, DTO mapping, Orchestration          │
└──────────────────────────┬─────────────────────────────┘
                           │ calls IEmployeeRepository
                           ▼
┌────────────────────────────────────────────────────────┐
│  Infrastructure Layer: Repositories (EmployeeRepository│
│  - EF Core AppDbContext queries, SaveChanges           │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────┐
│           SQL Server / Relational Database             │
└────────────────────────────────────────────────────────┘
```

### 2. Repository Pattern
- Introduced `IEmployeeRepository` and `EmployeeRepository` to isolate EF Core data access.
- Controllers depend on `IEmployeeService`; Services depend on `IEmployeeRepository`.
- Facilitates complete unit testing without database dependencies via `EmployeeServiceTests`.

### 3. Central Exception Handling Middleware
- `GlobalExceptionMiddleware` wraps all pipeline requests.
- Standardizes errors to RFC 7807 `ProblemDetails` (`application/problem+json`).
- Translates known exceptions (`ArgumentException`, `KeyNotFoundException`, `UnauthorizedAccessException`) to appropriate HTTP status codes (400, 404, 401, 500).

---

## Day 9 — EF Core, Relationships & Pagination

### 1. Entity Relationships (One-to-Many)
- Established relationship between `Department` (1) and `Employee` (Many).
- Configured via EF Core **Fluent API** in `AppDbContext.OnModelCreating` with `OnDelete(DeleteBehavior.SetNull)`.
- Seeded initial departments (Engineering, Human Resources, Finance).

### 2. Eager Loading & Query Tracking
- **Eager Loading**: Used `.Include(d => d.Employees)` to fetch related entities in a single SQL query, avoiding the N+1 select problem.
- **Tracking vs AsNoTracking**:
  - `AsNoTracking()` applied to all read operations (e.g. `GetAllAsync`, `GetPagedAsync`, `GetByIdAsync`) to eliminate change-tracker overhead.
  - Tracked queries (`FindAsync`) reserved for state mutations (e.g. Update, Delete).

### 3. Filtering, Sorting & Pagination
- Introduced `PagedResult<T>` wrapper providing `page`, `pageSize`, `totalCount`, `totalPages`, `hasPrevious`, `hasNext`.
- Query parameters via `EmployeeQueryParameters`:
  - `department`: Case-insensitive filter on department name.
  - `searchTerm`: Substring matching across name and email.
  - `sortBy`: Dynamic column ordering (`name`, `salary`, `joiningdate`, `department`).
  - `isDescending`: Toggle ascending or descending ordering.
  - Safe page bounds with `MaxPageSize = 50`.

---

## Day 10 — Authentication, Authorization & Security (Gate 2)

### 1. Authentication vs Authorization
- **Authentication (AuthN)**: *"Who are you?"*
  - Verifies identity via credentials (username/password) through `POST /api/auth/login`.
  - On success, issues a cryptographically signed JSON Web Token (JWT) bearing claims (`sub`, `name`, `role`).
- **Authorization (AuthZ)**: *"What are you allowed to do?"*
  - Determines if the authenticated caller has permission for a specific action or resource.
  - Enforced via `[Authorize]` attributes and role policies (e.g. `[Authorize(Roles = "Admin")]`).

### 2. HTTP 401 Unauthorized vs HTTP 403 Forbidden
| Status Code | Meaning | Example Scenario |
|-------------|---------|------------------|
| **401 Unauthorized** | The request lacks valid authentication credentials. | Calling `POST /api/employee` without an `Authorization: Bearer <token>` header, or with an expired/tampered token. |
| **403 Forbidden** | The caller is authenticated, but their identity lacks necessary privileges. | A standard user (`Role = "User"`) attempting to call `DELETE /api/employee/1` which requires `Role = "Admin"`. |

### 3. Secrets & Secure Configuration
- Sensitive secrets (like JWT symmetric keys) are read from `IConfiguration` (`builder.Configuration["Jwt:Key"]`).
- In local development, .NET User Secrets (`dotnet user-secrets`) or environment variables override configuration files without committing sensitive credentials to source control.

---

## API Endpoints

Base URL (Development): `http://localhost:5293`

| Method | Route | Auth / Role | Body | Success | Failure |
|--------|-------|-------------|------|---------|---------|
| `POST` | `/api/auth/login` | Anonymous | `LoginRequestDto` | `200 OK` (JWT) | `400`, `401 Unauthorized` |
| `GET` | `/api/employee` | Anonymous | — | `200 OK` | — |
| `GET` | `/api/employee/paged` | Anonymous | Query Params | `200 OK` (`PagedResult`) | — |
| `GET` | `/api/employee/{id}` | Anonymous | — | `200 OK` | `404 Not Found` |
| `POST` | `/api/employee` | `[Authorize]` | `CreateEmployeeDto` | `201 Created` | `400`, `401 Unauthorized` |
| `PUT` | `/api/employee/{id}` | `[Authorize]` | `UpdateEmployeeDto` | `204 No Content` | `400`, `401`, `404` |
| `DELETE` | `/api/employee/{id}` | `[Authorize(Roles="Admin")]` | — | `204 No Content` | `401`, `403 Forbidden`, `404` |
| `GET` | `/api/department` | Anonymous | — | `200 OK` | — |
| `GET` | `/api/department/{id}` | Anonymous | — | `200 OK` | `404 Not Found` |
| `POST` | `/api/department` | Anonymous | `CreateDepartmentDto` | `201 Created` | `400 Bad Request` |

Swagger UI available at: `http://localhost:5293/swagger` (includes Bearer Token input).

---

## Running the Project

```bash
# From solution root
cd EmployeeManagement.API
dotnet run
```

Then open: http://localhost:5293/swagger

---

## Running Tests

```bash
# From solution root
dotnet test EmployeeManagement.Tests
```

---

## Key Concepts Learned

| Concept | What it means |
|---------|--------------|
| **DTO (Data Transfer Object)** | A class dedicated to carrying data across API boundaries — separate from your domain model |
| **Data Annotations** | Attributes like `[Required]`, `[EmailAddress]`, `[Range]` that declare validation rules declaratively |
| **`[ApiController]`** | Automatically validates the model and returns `400 Bad Request` with error details if validation fails — no manual `ModelState.IsValid` check needed |
| **`[ProducesResponseType]`** | Documents exactly which HTTP status codes an action can return — powers Swagger response schema |
| **`CreatedAtAction`** | Returns `201 Created` with a `Location` header pointing to the new resource's GET URL — proper REST practice |
| **xUnit `[Theory]` + `[InlineData]`** | Parameterized tests — run the same test with multiple input values, reducing code duplication |
| **`Validator.TryValidateObject`** | Programmatically triggers Data Annotation validation — useful for testing DTO validation rules in isolation |

---

## 🎯 Week 2 Submission Checklist (Gate 2 Checkpoint)

- [x] **Source code with a clear project structure**: Split into Controllers, Services, Repositories, Data, Models, DTOs, and Middleware.
- [x] **README with setup and run instructions**: Complete documentation covering Days 6–10, request pipeline, and architecture diagrams.
- [x] **OpenAPI documentation and example requests**: Swagger UI with JWT Bearer support + 21 scenario HTTP test file (`EmployeeManagement.API.http`).
- [x] **Database setup or migration instructions**: EF Core with LocalDB/SQL Server connection strings and code-first migrations.
- [x] **Automated test suite and results**: Comprehensive unit tests covering DTO validation, Controllers, Services (with NSubstitute mocks), and Auth token generation.
- [x] **Build/test evidence and security**: Role-based authorization (`Admin` vs `User`) demonstrating 200, 201, 204, 400, 401, 403, and 404 responses.
