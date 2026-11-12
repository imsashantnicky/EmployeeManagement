namespace EmployeeDataAccess.Models;

public sealed class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal MonthlySalary { get; set; }
    public DateTime CreatedUtc { get; set; }
}
