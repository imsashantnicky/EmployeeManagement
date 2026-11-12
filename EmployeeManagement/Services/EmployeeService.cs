using EmployeeManagement.Models;
using EmployeeManagement.Repositories;

namespace EmployeeManagement.Services;

public sealed class EmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Employee> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Employee ID must be greater than zero.", nameof(id));

        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        return employee ?? throw new KeyNotFoundException(
            $"Employee with ID {id} was not found.");
    }

    public async Task<Employee> CreateAsync(
        Employee employee, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (string.IsNullOrWhiteSpace(employee.Name))
            throw new ArgumentException("Employee name is required.", nameof(employee));
        if (string.IsNullOrWhiteSpace(employee.Email))
            throw new ArgumentException("Employee email is required.", nameof(employee));
        if (employee.MonthlySalary <= 0)
            throw new ArgumentException("Monthly salary must be greater than zero.", nameof(employee));

        var existing = await _repository.GetByEmailAsync(employee.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("An employee with this email already exists.");

        await _repository.AddAsync(employee, cancellationToken);
        return employee;
    }

    public async Task UpdateSalaryAsync(
        int employeeId, decimal newMonthlySalary,
        CancellationToken cancellationToken = default)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Employee ID must be greater than zero.", nameof(employeeId));
        if (newMonthlySalary <= 0)
            throw new ArgumentException("Monthly salary must be greater than zero.", nameof(newMonthlySalary));

        var employee = await _repository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            throw new KeyNotFoundException($"Employee with ID {employeeId} was not found.");

        employee.MonthlySalary = newMonthlySalary;
        await _repository.UpdateAsync(employee, cancellationToken);
    }
}
