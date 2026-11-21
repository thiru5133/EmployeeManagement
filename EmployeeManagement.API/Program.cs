using EmployeeManagement.API.Data;
using EmployeeManagement.API.Middleware;
using EmployeeManagement.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Service Registration (DI Container) ────────────────────────────────────
builder.Services.AddControllers();

// EF Core — Scoped by default (one DbContext per HTTP request)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Day 7: Register our Employee Service
//
// AddScoped  → one instance per HTTP request       ✅ correct for DB-touching services
// AddTransient → new instance every time injected  (use for lightweight, stateless)
// AddSingleton → one instance for app lifetime     (use for caches, config readers)
//
// ⚠️ Captive dependency warning: Never inject a Scoped service into a Singleton.
//    The Scoped service would be held alive longer than its intended lifetime.
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Employee Management API",
        Version = "v1",
        Description = "ASP.NET Core Web API — Week 2 Training (Day 7: DI + Middleware)"
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────────────────────
// Order matters! Each middleware wraps the next.
// Think of it as an onion — request goes in from outside, response comes back out.

// Day 7: Our custom logging middleware — registered FIRST so it logs ALL requests
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
