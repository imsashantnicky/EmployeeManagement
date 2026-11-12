using BonusEngine.Models;
using BonusEngine.Services;
using Xunit;

namespace BonusEngine.Tests.xUnit;

public sealed class BonusEligibilityServiceTests
{
    [Fact]
    public void IsEligible_WhenEmployeeMeetsMinimumRules_ReturnsTrue()
    {
        // Arrange
        IBonusEligibilityService sut = new BonusEligibilityService();
        var employee = new EmployeeBonusInput(
            EmployeeId: 101,
            MonthlySalary: 50000m,
            CompletedServiceMonths: 12,
            PerformanceRating: 3);

        // Act
        var result = sut.IsEligible(employee);

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void IsEligible_WhenServiceIsBelowTwelveMonths_ReturnsFalse()
    {
        var sut = new BonusEligibilityService();
        var employee = new EmployeeBonusInput(102, 50000m, 11, 5);

        var result = sut.IsEligible(employee);

        Assert.False(result);
    }


    [Theory]
    [InlineData(12, 3, true)]
    [InlineData(12, 4, true)]
    [InlineData(12, 5, true)]
    [InlineData(11, 5, false)]
    [InlineData(24, 2, false)]
    [InlineData(0, 5, false)]
    public void IsEligible_UsesServiceAndRatingRules(
    int months, int rating, bool expected)
    {
        var sut = new BonusEligibilityService();
        var employee = new EmployeeBonusInput(103, 40000m, months, rating);

        Assert.Equal(expected, sut.IsEligible(employee));
    }


    [Theory]
    [InlineData(0, 50000, 12, 3)]
    [InlineData(101, 0, 12, 3)]
    [InlineData(101, -1, 12, 3)]
    [InlineData(101, 50000, -1, 3)]
    [InlineData(101, 50000, 12, 0)]
    [InlineData(101, 50000, 12, 6)]
    public void IsEligible_WhenInputIsInvalid_ThrowsArgumentException(
    int id, decimal salary, int months, int rating)
    {
        var sut = new BonusEligibilityService();
        var employee = new EmployeeBonusInput(id, salary, months, rating);

        Assert.Throws<ArgumentException>(() => sut.IsEligible(employee));
    }

    [Theory]
    [InlineData(50000, 12, 3, 30000)]
    [InlineData(50000, 12, 4, 60000)]
    [InlineData(50000, 12, 5, 90000)]
    public void CalculateBonus_WhenEligible_UsesRatingRate(
    decimal monthlySalary, int months, int rating, decimal expected)
    {
        var sut = new BonusEligibilityService();
        var employee = new EmployeeBonusInput(104, monthlySalary, months, rating);

        var result = sut.CalculateBonus(employee);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(11, 5)]
    [InlineData(24, 2)]
    public void CalculateBonus_WhenNotEligible_ReturnsZero(int months, int rating)
    {
        var sut = new BonusEligibilityService();
        var employee = new EmployeeBonusInput(105, 50000m, months, rating);

        Assert.Equal(0m, sut.CalculateBonus(employee));
    }


}
