namespace EmployeeOnboarding.Core.Models;

public sealed record Employee(
    int Id,
    string Name,
    string Email,
    string Department);
