using EmployeeManagement.API.Data;
using EmployeeManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Repositories
{
    /// <summary>
    /// EF Core implementation of IEmployeeRepository.
    ///
    /// Day 8 — Repository Pattern:
    ///   All direct DbContext calls are centralised here. The service layer
    ///   no longer knows about EF Core at all — it only calls this interface.
    ///
    ///   Registered as SCOPED in DI (same lifetime as AppDbContext) to avoid
    ///   captive-dependency issues.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── GET ALL ──────────────────────────────────────────────────────────────
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .AsNoTracking()      // read-only → no change-tracking overhead
                .ToListAsync();
        }

        // ── GET BY ID ────────────────────────────────────────────────────────────
        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // ── ADD ──────────────────────────────────────────────────────────────────
        public async Task<Employee> AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;    // EF populates the generated Id after SaveChanges
        }

        // ── UPDATE ───────────────────────────────────────────────────────────────
        public async Task UpdateAsync(Employee employee)
        {
            // The entity is already tracked by EF Core (fetched via FindAsync in
            // the service); calling SaveChanges persists all property changes.
            await _context.SaveChangesAsync();
        }

        // ── DELETE ───────────────────────────────────────────────────────────────
        public async Task DeleteAsync(Employee employee)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        // ── EXISTS ───────────────────────────────────────────────────────────────
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }

        // ── GET TRACKED ──────────────────────────────────────────────────────────
        public async Task<Employee?> GetTrackedByIdAsync(int id)
        {
            // FindAsync checks the local cache first, then hits the DB.
            // Does NOT use AsNoTracking, so EF detects property changes.
            return await _context.Employees.FindAsync(id);
        }
    }
}
