using EmployeeManagement.Models;
using EmployeeManagement.Repositories;
using EmployeeManagement.Services;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.xUnit;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsEmployee()
    {
        var employee = new Employee
        {
            Id = 7,
            Name = "Asha Sharma",
            Email = "asha@example.com",
            MonthlySalary = 50000m
        };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _service.GetByIdAsync(7);

        Assert.Same(employee, result);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(99));
        Assert.Contains("99", ex.Message);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdInvalid_DoesNotCallRepository()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetByIdAsync(0));
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_AddsEmployee()
    {
        var employee = new Employee
        {
            Name = "Ravi Patel",
            Email = "ravi@example.com",
            MonthlySalary = 42000m
        };
        _repositoryMock
            .Setup(r => r.GetByEmailAsync(employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);
        _repositoryMock
            .Setup(r => r.AddAsync(employee, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(employee);

        Assert.Same(employee, result);
        _repositoryMock.Verify(
            r => r.GetByEmailAsync(employee.Email, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(
            r => r.AddAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailExists_DoesNotAdd()
    {
        var employee = new Employee
        {
            Name = "Ravi Patel",
            Email = "ravi@example.com",
            MonthlySalary = 42000m
        };
        var existing = new Employee
        {
            Id = 3,
            Name = "Existing Ravi",
            Email = employee.Email,
            MonthlySalary = 39000m
        };
        _repositoryMock
            .Setup(r => r.GetByEmailAsync(employee.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(employee));
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSalaryInvalid_DoesNotCallRepository()
    {
        var employee = new Employee
        {
            Name = "Ravi Patel",
            Email = "ravi@example.com",
            MonthlySalary = 0m
        };
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(employee));

        _repositoryMock.Verify(
            r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateSalaryAsync_WhenEmployeeExists_UpdatesEmployee()
    {
        var employee = new Employee
        {
            Id = 12,
            Name = "Neha Verma",
            Email = "neha@example.com",
            MonthlySalary = 45000m
        };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _repositoryMock
            .Setup(r => r.UpdateAsync(employee, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.UpdateSalaryAsync(12, 60000m);

        Assert.Equal(60000m, employee.MonthlySalary);
        _repositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Employee>(e => e.Id == 12 && e.MonthlySalary == 60000m),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Storage unavailable"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByIdAsync(5));
        Assert.Equal("Storage unavailable", ex.Message);
    }
}
