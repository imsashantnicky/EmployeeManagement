using BonusEngine.Models;

namespace BonusEngine.Services;

public sealed class BonusEligibilityService : IBonusEligibilityService
{
    public bool IsEligible(EmployeeBonusInput employee)
    {
        Validate(employee);

        return employee.CompletedServiceMonths >= 12
            && employee.PerformanceRating >= 3;
    }

    public decimal CalculateBonus(EmployeeBonusInput employee)
    {
        Validate(employee);

        if (!IsEligibleAfterValidation(employee))
            return 0m;

        var rate = employee.PerformanceRating switch
        {
            3 => 0.05m,
            4 => 0.10m,
            5 => 0.15m,
            _ => 0m
        };

        var annualBaseSalary = employee.MonthlySalary * 12m;
        return decimal.Round(
            annualBaseSalary * rate,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static bool IsEligibleAfterValidation(EmployeeBonusInput employee) =>
        employee.CompletedServiceMonths >= 12
        && employee.PerformanceRating >= 3;

    private static void Validate(EmployeeBonusInput employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (employee.EmployeeId <= 0)
            throw new ArgumentException("Employee ID must be greater than zero.", nameof(employee));

        if (employee.MonthlySalary <= 0m)
            throw new ArgumentException("Monthly salary must be greater than zero.", nameof(employee));

        if (employee.CompletedServiceMonths < 0)
            throw new ArgumentException("Completed service months cannot be negative.", nameof(employee));

        if (employee.PerformanceRating is < 1 or > 5)
            throw new ArgumentException("Performance rating must be between 1 and 5.", nameof(employee));
    }
}
