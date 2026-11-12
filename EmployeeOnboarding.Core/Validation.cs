using EmployeeOnboarding.Core.Models;

namespace EmployeeOnboarding.Core.Validation;

public sealed class EmployeeInputValidator
{
    public void Validate(EmployeeInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("Employee name is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.Email) || !input.Email.Contains('@'))
            throw new ArgumentException("A valid employee email is required.", nameof(input));

        if (string.IsNullOrWhiteSpace(input.Department))
            throw new ArgumentException("Department is required.", nameof(input));
    }
}


