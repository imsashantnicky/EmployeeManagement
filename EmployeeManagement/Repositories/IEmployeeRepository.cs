using EmployeeManagement.Models;

namespace EmployeeManagement.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
}

