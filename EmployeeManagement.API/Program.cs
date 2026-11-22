using System.Text;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.Middleware;
using EmployeeManagement.API.Repositories;
using EmployeeManagement.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ─── Service Registration (DI Container) ────────────────────────────────────
builder.Services.AddControllers();

// EF Core — Scoped by default (one DbContext per HTTP request)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Day 7: Register our Employee Service (Scoped)
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Day 8: Register the Repository (Scoped)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// Day 10: Register Auth Service (Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();

// Day 10: JWT Authentication & Authorization Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Week2TrainingSuperSecretKeyForJwtTokenGeneration2026!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EmployeeManagementAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EmployeeManagementClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Management API",
        Version = "v1",
        Description = "ASP.NET Core Web API — Week 2 Training (Gate 2 Checkpoint: Full Layered Architecture & Security)"
    });

    // Day 10: Add Bearer Authentication UI to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your valid JWT token.\n\nExample: Bearer eyJhbGciOi..."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
// Order matters! Each middleware wraps the next.
// Think of it as an onion — request goes in from outside, response comes back out.

// Day 8: Global Exception Handling middleware — catches ALL unhandled exceptions
// and returns RFC 7807 ProblemDetails JSON. Must be FIRST so it wraps everything.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Day 7: Custom logging middleware — logs request & response metadata safely
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Day 10: Authentication MUST come before Authorization!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
