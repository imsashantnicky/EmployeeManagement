using BonusEngine.Models;
using BonusEngine.Services;
using NUnit.Framework;

namespace BonusEngine.Tests.NUnit;

[TestFixture]
public sealed class BonusEligibilityServiceTests
{
    private BonusEligibilityService _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new BonusEligibilityService();

    [TestCase(12, 3, true)]
    [TestCase(12, 5, true)]
    [TestCase(11, 5, false)]
    [TestCase(24, 2, false)]
    [TestCase(0, 5, false)]
    public void IsEligible_UsesServiceAndRatingRules(
        int months, int rating, bool expected)
    {
        var employee = new EmployeeBonusInput(201, 40000m, months, rating);

        Assert.That(_sut.IsEligible(employee), Is.EqualTo(expected));
    }

    [TestCase(50000, 12, 3, 30000)]
    [TestCase(50000, 12, 4, 60000)]
    [TestCase(50000, 12, 5, 90000)]
    public void CalculateBonus_UsesRatingRate(
        decimal salary, int months, int rating, decimal expected)
    {
        var employee = new EmployeeBonusInput(202, salary, months, rating);

        Assert.That(_sut.CalculateBonus(employee), Is.EqualTo(expected));
    }

    [Test]
    public void IsEligible_WhenEmployeeIdIsInvalid_ThrowsArgumentException()
    {
        var employee = new EmployeeBonusInput(0, 50000m, 12, 3);

        Assert.Throws<ArgumentException>(() => _sut.IsEligible(employee));
    }
}
