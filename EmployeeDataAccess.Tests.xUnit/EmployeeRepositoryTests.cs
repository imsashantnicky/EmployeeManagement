using EmployeeDataAccess.Models;
using EmployeeDataAccess.Repositories;
using Xunit;

namespace EmployeeDataAccess.Tests.xUnit;

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

    [Fact]
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

        Assert.NotNull(saved);
        Assert.Equal("Asha Sharma", saved.Name);
        Assert.Equal("asha@example.com", saved.Email);
        Assert.Equal(50000m, saved.MonthlySalary);
    }

    [Fact]
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

        await using var queryContext = database.CreateContext();
        var repository = new EmployeeRepository(queryContext);

        var result = await repository.GetByIdAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.Id);
        Assert.Equal(employee.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeDoesNotExist_ReturnsNull()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
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

        Assert.Equal(3, result.Count);
        Assert.Equal(
            new[] { "Aman", "Neha", "Zoya" },
            result.Select(e => e.Name).ToArray());
    }

    [Fact]
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

        Assert.NotNull(saved);
        Assert.Equal(65000m, saved.MonthlySalary);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeExists_RemovesRecord()
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
            var deleted = await repository.DeleteAsync(employee.Id);
            Assert.True(deleted);
        }

        await using var verifyContext = database.CreateContext();
        var saved = await verifyContext.Employees.FindAsync(employee.Id);
        Assert.Null(saved);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeDoesNotExist_ReturnsFalse()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);

        var deleted = await repository.DeleteAsync(404);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddAsync_WhenEmailAlreadyExists_ThrowsDbUpdateException()
    {
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using var context = database.CreateContext();
        var repository = new EmployeeRepository(context);

        await repository.AddAsync(NewEmployee("Asha", "same@example.com"));

        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(
            () => repository.AddAsync(NewEmployee("Ravi", "same@example.com")));
    }
}
