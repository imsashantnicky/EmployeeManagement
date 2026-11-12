using EmployeeOnboarding.Core.Models;

namespace EmployeeOnboarding.Core.Abstractions;

public interface IEmployeeRepository
{
    Task<int> AddAsync(EmployeeInput input, CancellationToken cancellationToken = default);
}

public interface IAccountProvisioner
{
    Task ProvisionAsync(string email, CancellationToken cancellationToken = default);
}

public interface IWelcomeNotifier
{
    Task SendAsync(string email, string employeeName, CancellationToken cancellationToken = default);
}
