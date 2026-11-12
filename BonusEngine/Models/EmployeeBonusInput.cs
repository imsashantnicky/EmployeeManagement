namespace BonusEngine.Models;

public sealed record EmployeeBonusInput(
    int EmployeeId,
    decimal MonthlySalary,
    int CompletedServiceMonths,
    int PerformanceRating
    
);

