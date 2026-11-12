using BonusEngine.Models;

namespace BonusEngine.Services;

public interface IBonusEligibilityService
{
    bool IsEligible(EmployeeBonusInput employee);
    decimal CalculateBonus(EmployeeBonusInput employee);
}
