using EmployeeDataAccess.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDataAccess.Tests.NUnit;

public sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public DbContextOptions<AppDbContext> Options { get; }

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        Options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public AppDbContext CreateContext()
    {
        return new AppDbContext(Options);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
