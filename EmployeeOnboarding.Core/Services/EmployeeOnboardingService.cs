using EmployeeOnboarding.Core.Abstractions;
using EmployeeOnboarding.Core.Models;
using EmployeeOnboarding.Core.Validation;

namespace EmployeeOnboarding.Core.Services;

public sealed class EmployeeOnboardingService
{
    private readonly IEmployeeRepository _repository;
    private readonly IAccountProvisioner _accountProvisioner;
    private readonly IWelcomeNotifier _notifier;
    private readonly EmployeeInputValidator _validator;

    public EmployeeOnboardingService(
        IEmployeeRepository repository,
        IAccountProvisioner accountProvisioner,
        IWelcomeNotifier notifier,
        EmployeeInputValidator validator)
    {
        _repository = repository;
        _accountProvisioner = accountProvisioner;
        _notifier = notifier;
        _validator = validator;
    }

    public async Task<int> OnboardAsync(
        EmployeeInput input,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(input);

        var employeeId = await _repository.AddAsync(input, cancellationToken);

        await _accountProvisioner.ProvisionAsync(input.Email, cancellationToken);

        await _notifier.SendAsync(
            input.Email, input.Name, cancellationToken);

        return employeeId;
    }
}


