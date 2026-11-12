using EmployeeDataAccess.Models;
using EmployeeDataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EmployeeDataAccess.Tests.NUnit;

[TestFixture]
public class EmployeeRepositoryTests
{
    private static Employee NewEmployee(
        string name = "Asha Sharma",
        string email = "asha@example.com")
    {
        return new Employee
        {
            Name = name,
            Email = email,
            MonthlySalary = 50000m,
            CreatedUtc = DateTime.UtcNow
        };
    }

    [Test]
    public async Task AddAsync_PersistsEmployee()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        var employee = NewEmployee();

        await using (var writeContext = database.CreateContext())
        {
            var repository = new EmployeeRepository(writeContext);
            await repository.AddAsync(employee);
        }

        await using var readContext = database.CreateContext();
        var saved = await readContext.Employees.FindAsync(employee.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Name, Is.EqualTo("Asha Sharma"));
        Assert.That(saved.Email, Is.EqualTo("asha@example.com"));
        Assert.That(saved.MonthlySalary, Is.EqualTo(50000m));
    }

    [Test]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsEmployee()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        var employee = NewEmployee();

        await using (var seedContext = database.CreateContext())
        {
            seedContext.Employees.Add(employee);
            await seedContext.SaveChangesAsync();
        }

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);
        var result = await repository.GetByIdAsync(employee.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo(employee.Email));
    }

    [Test]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsEmployeesOrderedByName()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using (var seedContext = database.CreateContext())
        {
            seedContext.Employees.AddRange(
                NewEmployee("Zoya", "zoya@example.com"),
                NewEmployee("Aman", "aman@example.com"),
                NewEmployee("Neha", "neha@example.com"));
            await seedContext.SaveChangesAsync();
        }

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);
        var result = await repository.GetAllAsync();

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Select(e => e.Name),
            Is.EqualTo(new[] { "Aman", "Neha", "Zoya" }));
    }

    [Test]
    public async Task UpdateAsync_PersistsChangedSalary()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        var employee = NewEmployee();

        await using (var seedContext = database.CreateContext())
        {
            seedContext.Employees.Add(employee);
            await seedContext.SaveChangesAsync();
        }

        await using (var updateContext = database.CreateContext())
        {
            var repository = new EmployeeRepository(updateContext);
            employee.MonthlySalary = 65000m;
            await repository.UpdateAsync(employee);
        }

        await using var verifyContext = database.CreateContext();
        var saved = await verifyContext.Employees.FindAsync(employee.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.MonthlySalary, Is.EqualTo(65000m));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesRecord()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        var employee = NewEmployee();

        await using (var seedContext = database.CreateContext())
        {
            seedContext.Employees.Add(employee);
            await seedContext.SaveChangesAsync();
        }

        await using (var deleteContext = database.CreateContext())
        {
            var repository = new EmployeeRepository(deleteContext);
            Assert.That(await repository.DeleteAsync(employee.Id), Is.True);
        }

        await using var verifyContext = database.CreateContext();
        Assert.That(await verifyContext.Employees.FindAsync(employee.Id), Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);

        Assert.That(await repository.DeleteAsync(404), Is.False);
    }

    [Test]
    public async Task AddAsync_WhenEmailAlreadyExists_ThrowsDbUpdateException()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);

        await repository.AddAsync(NewEmployee("Asha", "same@example.com"));

        Assert.ThrowsAsync<DbUpdateException>(
            async () => await repository.AddAsync(
                NewEmployee("Ravi", "same@example.com")));
    }
}
