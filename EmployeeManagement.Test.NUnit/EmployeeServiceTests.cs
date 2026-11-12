using EmployeeManagement.Models;
using EmployeeManagement.Repositories;
using EmployeeManagement.Services;
using Moq;
using NUnit.Framework;

namespace EmployeeManagement.Tests.NUnit;

[TestFixture]
public class EmployeeServiceTests
{
    private Mock<IEmployeeRepository> _repositoryMock = null!;
    private EmployeeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _service = new EmployeeService(_repositoryMock.Object);
    }

    [Test]
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

        Assert.That(result, Is.SameAs(employee));
        _repositoryMock.Verify(
            r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetByIdAsync_WhenMissing_ThrowsKeyNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        Assert.That(async () => await _service.GetByIdAsync(99),
            Throws.TypeOf<KeyNotFoundException>());
        _repositoryMock.Verify(
            r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetByIdAsync_WhenIdInvalid_DoesNotCallRepository()
    {
        Assert.That(async () => await _service.GetByIdAsync(0),
            Throws.TypeOf<ArgumentException>());
        _repositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
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

        Assert.That(result, Is.SameAs(employee));
        _repositoryMock.Verify(
            r => r.GetByEmailAsync(employee.Email, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(
            r => r.AddAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateAsync_WhenEmailExists_DoesNotAdd()
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

        Assert.That(async () => await _service.CreateAsync(employee),
            Throws.TypeOf<InvalidOperationException>());
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void CreateAsync_WhenSalaryInvalid_DoesNotCallRepository()
    {
        var employee = new Employee
        {
            Name = "Ravi Patel",
            Email = "ravi@example.com",
            MonthlySalary = 0m
        };
        Assert.That(async () => await _service.CreateAsync(employee),
            Throws.TypeOf<ArgumentException>());
        _repositoryMock.Verify(
            r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
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

        Assert.That(employee.MonthlySalary, Is.EqualTo(60000m));
        _repositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Employee>(e => e.Id == 12 && e.MonthlySalary == 60000m),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetByIdAsync_WhenRepositoryThrows_PropagatesException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Storage unavailable"));

        Assert.That(async () => await _service.GetByIdAsync(5),
            Throws.TypeOf<InvalidOperationException>());
    }
}
