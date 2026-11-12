using EmployeeDataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EmployeeDataAccess.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.MonthlySalary)
                .HasPrecision(18, 2);
        });
    }
}
