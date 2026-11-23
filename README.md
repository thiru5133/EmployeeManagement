# Employee Management Web API

A **.NET 10 ASP.NET Core Web API** with JWT authentication, role-based authorization, EF Core with SQL Server, and a full layered architecture (Controller → Service → Repository → Database).

---

## 📋 Table of Contents

- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Local Setup (Step by Step)](#local-setup-step-by-step)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Authentication Guide](#authentication-guide)
- [Running Tests](#running-tests)
- [Architecture](#architecture)
- [Common Issues & Fixes](#common-issues--fixes)
- [Day-by-Day Progress](#day-by-day-progress)
  - [Day 6 — DTOs, Validation & REST API Basics](#day-6--dtos-validation--rest-api-basics)
  - [Day 7 — Dependency Injection & Middleware](#day-7--dependency-injection--middleware)
  - [Day 8 — Layered Architecture & Central Exception Handling](#day-8--layered-architecture--central-exception-handling)
  - [Day 9 — EF Core, Relationships & Pagination](#day-9--ef-core-relationships--pagination)
  - [Day 10 — Authentication, Authorization & Security](#day-10--authentication-authorization--security-gate-2)
  - [Latest — User Registration with Password Hashing](#latest--user-registration-with-password-hashing)
- [Gate 2 Submission Checklist](#-week-2-submission-checklist-gate-2-checkpoint)

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (via Docker) |
| Auth | JWT Bearer Tokens |
| API Docs | Swagger / OpenAPI |
| Testing | xUnit + NSubstitute |

---

## Prerequisites

Make sure you have these installed before starting:

| Tool | Purpose | Download |
|------|---------|----------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Build & run the app | `dotnet --version` to verify |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Run SQL Server locally | Must be running |
| [SSMS](https://aka.ms/ssms) | View & query the database (optional) | GUI for SQL Server |
| [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) | IDE | — |

---

## Local Setup (Step by Step)

### Step 1 — Clone the Repository

```bash
git clone <your-repo-url>
cd EmployeeManagement
```

---

### Step 2 — Start SQL Server via Docker

Make sure **Docker Desktop is open and running**, then run:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Admin@123456" -p 1433:1433 --name sqlserver --restart unless-stopped -d mcr.microsoft.com/mssql/server:2022-latest
```

> **Note:** This only needs to be run **once**. After that, the container auto-starts with Docker Desktop.

To verify it's running:
```bash
docker ps
```
You should see `sqlserver` with status `Up`.

**To start/stop the container later:**
```bash
docker start sqlserver   # start
docker stop sqlserver    # stop
```

---

### Step 3 — Configure the Connection String

The connection string in `EmployeeManagement.API/appsettings.json` is already set for Docker:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=EmployeeManagementDB;User Id=sa;Password=Admin@123456;TrustServerCertificate=True;"
}
```

> ✅ No changes needed if you used the Docker command in Step 2 exactly as-is.  
> ⚠️ If you used a different `sa` password, update it here.

---

### Step 4 — Apply Database Migrations

This creates the database and all tables automatically:

```bash
dotnet ef database update --project EmployeeManagement.API
```

**Expected output:**
```
Build started...
Build succeeded.
Applying migration '20261116040903_InitialCreate'...
Done.
```

After this, your database will have:

| Table | Description |
|-------|-------------|
| `dbo.Employees` | Employee records |
| `dbo.Departments` | Departments (pre-seeded with 3 entries) |
| `dbo.Users` | Registered user accounts |
| `dbo.__EFMigrationsHistory` | EF migration tracking |

---

### Step 5 — Run the API

```bash
cd EmployeeManagement.API
dotnet run
```

Or press **F5** in Visual Studio.

The API will be available at:
- **Swagger UI:** http://localhost:5293/swagger
- **Base URL:** http://localhost:5293

---

### Step 6 — Verify It Works

Open Swagger at http://localhost:5293/swagger and try:

1. `POST /api/auth/register` — create a new user
2. `POST /api/auth/login` — get a JWT token
3. Click **Authorize** (🔒) in Swagger → paste the token → test protected endpoints

---

## Project Structure

```
EmployeeManagement/
├── EmployeeManagement.slnx                   # Solution file
│
├── EmployeeManagement.API/                   # Main Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs                 # Register & Login endpoints
│   │   ├── EmployeeController.cs             # Employee CRUD endpoints
│   │   └── DepartmentController.cs           # Department endpoints
│   │
│   ├── Services/
│   │   ├── IAuthService.cs / AuthService.cs  # Auth business logic + JWT generation
│   │   ├── IEmployeeService.cs / EmployeeService.cs
│   │   └── IDepartmentService.cs / DepartmentService.cs
│   │
│   ├── Repositories/
│   │   ├── IEmployeeRepository.cs / EmployeeRepository.cs  # EF Core data access
│   │   └── IDepartmentRepository.cs / DepartmentRepository.cs
│   │
│   ├── Models/
│   │   ├── Employee.cs                       # Employee entity
│   │   ├── Department.cs                     # Department entity
│   │   └── User.cs                           # User entity
│   │
│   ├── DTOs/
│   │   ├── AuthDtos.cs                       # Login/Register request & response DTOs
│   │   ├── EmployeeDto.cs                    # Employee request & response DTOs
│   │   ├── DepartmentDto.cs                  # Department DTOs
│   │   ├── EmployeeQueryParameters.cs        # Filtering, sorting, pagination params
│   │   └── PagedResult.cs                    # Generic pagination wrapper
│   │
│   ├── Data/
│   │   └── AppDbContext.cs                   # EF Core DbContext + seeding
│   │
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs       # Centralized error handling (RFC 7807)
│   │   └── RequestLoggingMiddleware.cs        # HTTP request/response logging
│   │
│   ├── Migrations/                            # EF Core migration files (auto-generated)
│   ├── appsettings.json                       # App config (connection string, JWT, logging)
│   ├── Program.cs                             # DI registration + middleware pipeline
│   └── EmployeeManagement.API.http            # HTTP test scenarios (run in VS)
│
└── EmployeeManagement.Tests/                  # xUnit test project
    ├── AuthServiceTests.cs                    # Auth + registration unit tests
    ├── EmployeeControllerTests.cs             # Controller HTTP status code tests
    └── EmployeeServiceTests.cs                # Service layer unit tests (NSubstitute mocks)
```

---

## API Endpoints

Base URL: `http://localhost:5293`

### Auth (No token required)

| Method | Endpoint | Body | Response |
|--------|----------|------|----------|
| `POST` | `/api/auth/register` | `RegisterRequestDto` | `201 Created` |
| `POST` | `/api/auth/login` | `LoginRequestDto` | `200 OK` + JWT token |

### Employees

| Method | Endpoint | Auth Required | Response |
|--------|----------|--------------|----------|
| `GET` | `/api/employee` | ❌ | `200 OK` — all employees |
| `GET` | `/api/employee/paged` | ❌ | `200 OK` — paginated + filtered |
| `GET` | `/api/employee/{id}` | ❌ | `200 OK` / `404` |
| `POST` | `/api/employee` | ✅ Any role | `201 Created` / `400` |
| `PUT` | `/api/employee/{id}` | ✅ Any role | `204 No Content` / `400` / `404` |
| `DELETE` | `/api/employee/{id}` | ✅ **Admin only** | `204 No Content` / `403` / `404` |

### Departments

| Method | Endpoint | Auth Required | Response |
|--------|----------|--------------|----------|
| `GET` | `/api/department` | ❌ | `200 OK` — all departments |
| `GET` | `/api/department/{id}` | ❌ | `200 OK` / `404` |
| `POST` | `/api/department` | ❌ | `201 Created` / `400` |

---

## Authentication Guide

### 1. Register a User

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john",
  "email": "john@example.com",
  "password": "SecurePass@123",
  "role": "Admin"   // "Admin" or "User"
}
```

### 2. Login and Get Token

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john",
  "password": "SecurePass@123"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john",
  "role": "Admin"
}
```

### 3. Use Token in Requests

```http
GET /api/employee
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**In Swagger:** Click the 🔒 **Authorize** button → enter `Bearer <your-token>`.

### Demo Accounts (built-in, no registration needed)

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin@123` | Admin |
| `user` | `User@123` | User |

---

## Running Tests

```bash
dotnet test
```

Or to run a specific project:
```bash
dotnet test EmployeeManagement.Tests
```

**Current test results:**
```
Total:   Passed   Failed  Skipped
  ✅       All      0        0
```

Tests cover:
- DTO validation (Data Annotations)
- Controller HTTP status codes
- Service layer business logic (mocked repository)
- Auth: register, login, duplicate username, invalid password

---

## Architecture

```
HTTP Request
     │
     ▼
┌─────────────────────────┐
│  Middleware Pipeline     │
│  GlobalExceptionMiddleware  ← catches all unhandled exceptions → RFC 7807 JSON
│  RequestLoggingMiddleware   ← logs method, route, response time
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  Controllers             │  ← HTTP concerns only (routing, status codes, model binding)
│  AuthController          │
│  EmployeeController      │
│  DepartmentController    │
└────────────┬────────────┘
             │ calls
             ▼
┌─────────────────────────┐
│  Services                │  ← Business logic, DTO mapping, JWT generation
│  AuthService             │
│  EmployeeService         │
└────────────┬────────────┘
             │ calls
             ▼
┌─────────────────────────┐
│  Repositories            │  ← EF Core queries only (no business logic here)
│  EmployeeRepository      │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│  SQL Server (Docker)     │  ← EmployeeManagementDB
│  dbo.Employees           │
│  dbo.Departments         │
│  dbo.Users               │
└─────────────────────────┘
```

---

## Common Issues & Fixes

| Problem | Fix |
|---------|-----|
| `Cannot connect to SQL Server` | Make sure Docker is running: `docker start sqlserver` |
| `Invalid object name 'Users'` | Run migrations: `dotnet ef database update --project EmployeeManagement.API` |
| `Login failed for user 'sa'` | Wrong password in connection string — update `appsettings.json` to match your Docker `MSSQL_SA_PASSWORD` |
| `.exe is locked` (build fails) | Stop the running app in Visual Studio first, then build |
| Port 5293 already in use | Change the port in `Properties/launchSettings.json` |

---

## Day-by-Day Progress

---

### Day 6 — DTOs, Validation & REST API Basics

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
Contains `Id`, `Name`, `Email`, `Department`, `Salary`, `JoiningDate`. Decouples what clients see from the internal domain model.

**Why DTOs matter:**
| Without DTOs | With DTOs |
|---|---|
| Client can send/receive internal fields (e.g. `Id` on create) | Full control over input/output shape |
| No validation — garbage in, garbage out | `[ApiController]` auto-returns `400` for invalid input |
| Breaking your API means changing your Entity | Entity and API contract evolve independently |

**Controller updated to:**
```csharp
// Before (Day 5)
public ActionResult<Employee> CreateEmployee([FromBody] Employee employee)

// After (Day 6)
public ActionResult<EmployeeResponseDto> CreateEmployee([FromBody] CreateEmployeeDto dto)
```

**Unit Tests added — 17 tests across 2 files:**

| Test File | Tests | What it covers |
|-----------|-------|---------------|
| `EmployeeValidationTests.cs` | 10 | DTO validation rules (`[Required]`, `[EmailAddress]`, `[Range]`) |
| `EmployeeControllerTests.cs` | 7 | HTTP status codes (200, 201, 204, 404, 400) |

```
Total tests:  17   Passed: 17   Failed: 0
```

---

### Day 7 — Dependency Injection & Middleware

- **Service Layer Separation**: Extracted business operations into `IEmployeeService` and concrete `EmployeeService`.
- **DI Container Lifetimes**:

| Lifetime | Method | Behaviour |
|----------|--------|-----------|
| Scoped | `AddScoped` | One instance per HTTP request (used for Services & DbContext) |
| Transient | `AddTransient` | Fresh instance every time it is injected |
| Singleton | `AddSingleton` | Single instance for the entire app lifetime |

- **Captive Dependency Rule**: Never inject a Scoped service into a Singleton — the Scoped instance gets "captured" and outlives its intended lifetime.
- **Custom Logging Middleware**: `RequestLoggingMiddleware` logs request method, route, and response time (ms) with appropriate log levels.
- **Mocking with NSubstitute**: Controller unit tests now use mocked `IEmployeeService` instead of shared in-memory lists — proper test isolation.

---

### Day 8 — Layered Architecture & Central Exception Handling

#### 1. Layered Architecture & Separation of Concerns

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

#### 2. Repository Pattern
- Introduced `IEmployeeRepository` and `EmployeeRepository` to isolate EF Core data access.
- Controllers depend on `IEmployeeService`; Services depend on `IEmployeeRepository`.
- Enables complete unit testing without real database dependencies.

#### 3. Central Exception Handling Middleware
- `GlobalExceptionMiddleware` wraps the entire pipeline.
- Standardizes errors to RFC 7807 `ProblemDetails` (`application/problem+json`).
- Translates known exceptions to appropriate HTTP status codes:

| Exception | HTTP Status |
|-----------|------------|
| `ArgumentException` | `400 Bad Request` |
| `KeyNotFoundException` | `404 Not Found` |
| `UnauthorizedAccessException` | `401 Unauthorized` |
| Any other | `500 Internal Server Error` |

---

### Day 9 — EF Core, Relationships & Pagination

#### 1. Entity Relationships (One-to-Many)
- Established relationship between `Department` (1) and `Employee` (Many).
- Configured via EF Core **Fluent API** in `AppDbContext.OnModelCreating`:

```csharp
modelBuilder.Entity<Employee>()
    .HasOne(e => e.DepartmentEntity)
    .WithMany(d => d.Employees)
    .HasForeignKey(e => e.DepartmentId)
    .OnDelete(DeleteBehavior.SetNull);
```

- Seeded initial departments (Engineering, Human Resources, Finance).

#### 2. Eager Loading & Query Tracking

| Strategy | When to use |
|----------|------------|
| `AsNoTracking()` | Read-only queries — better performance, no change-tracker overhead |
| Tracked (default) | Mutations (Update, Delete) — EF needs to detect changes |
| `.Include(d => d.Employees)` | Eager loading — fetches related entity in single SQL query, avoids N+1 |

#### 3. Filtering, Sorting & Pagination

Introduced `PagedResult<T>` wrapper returning:

```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true,
  "data": [ ... ]
}
```

`EmployeeQueryParameters` supports:

| Parameter | Description |
|-----------|-------------|
| `department` | Case-insensitive filter on department name |
| `searchTerm` | Substring match across name and email |
| `sortBy` | Column ordering: `name`, `salary`, `joiningdate`, `department` |
| `isDescending` | Toggle ascending / descending |
| `page` / `pageSize` | Pagination (max page size: 50) |

---

### Day 10 — Authentication, Authorization & Security (Gate 2)

#### 1. Authentication vs Authorization

| Concept | Question it answers | Implementation |
|---------|-------------------|----------------|
| **Authentication (AuthN)** | *Who are you?* | `POST /api/auth/login` verifies credentials, issues signed JWT |
| **Authorization (AuthZ)** | *What are you allowed to do?* | `[Authorize]` and `[Authorize(Roles="Admin")]` attributes |

#### 2. HTTP 401 vs 403

| Status Code | Meaning | Example |
|-------------|---------|---------|
| **401 Unauthorized** | No valid credentials provided | Calling a protected endpoint without a Bearer token, or with an expired/tampered token |
| **403 Forbidden** | Authenticated, but insufficient privileges | A `User` role trying to call `DELETE /api/employee/1` which requires `Admin` role |

#### 3. JWT Token Structure

```
Header.Payload.Signature
  │       │        │
  │       │        └── HMAC-SHA256 signed with secret key
  │       └── Claims: sub, name, role, exp
  └── Algorithm: HS256
```

#### 4. Secrets & Secure Configuration
- JWT signing key is read from `IConfiguration` (`builder.Configuration["Jwt:Key"]`).
- In production, override via environment variables — never commit secrets to source control.
- For local development, use `dotnet user-secrets` to override `appsettings.json` values safely.

---

### Latest — User Registration with Password Hashing

Added `POST /api/auth/register` endpoint with:

- **Cryptographic password hashing**: Passwords stored as `PBKDF2` hash + salt — never plain text.
- **Duplicate username check**: Returns `400 Bad Request` if username already exists.
- **Role assignment**: Accepts `"Admin"` or `"User"` role at registration time.
- **DB persistence**: Registered users stored in `dbo.Users` table via EF Core.

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "newdev",
  "email": "newdev@example.com",
  "password": "SecurePass@123",
  "role": "User"
}
```

Response (`201 Created`):
```json
{
  "id": 1,
  "username": "newdev",
  "email": "newdev@example.com",
  "role": "User",
  "message": "User registered successfully."
}
```

---

## 🎯 Week 2 Submission Checklist (Gate 2 Checkpoint)

- [x] **Source code with a clear project structure**: Split into Controllers, Services, Repositories, Data, Models, DTOs, and Middleware.
- [x] **README with setup and run instructions**: Complete documentation covering Days 6–10, request pipeline, and architecture diagrams.
- [x] **OpenAPI documentation and example requests**: Swagger UI with JWT Bearer support + 21 scenario HTTP test file (`EmployeeManagement.API.http`).
- [x] **Database setup or migration instructions**: EF Core with Docker SQL Server connection strings and code-first migrations.
- [x] **Automated test suite and results**: Comprehensive unit tests covering DTO validation, Controllers, Services (NSubstitute mocks), and Auth.
- [x] **Build/test evidence and security**: Role-based authorization (`Admin` vs `User`) demonstrating 200, 201, 204, 400, 401, 403, and 404 responses.
