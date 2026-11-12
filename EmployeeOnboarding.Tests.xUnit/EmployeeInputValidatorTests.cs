using EmployeeOnboarding.Core.Models;
using EmployeeOnboarding.Core.Validation;
using Xunit;

namespace EmployeeOnboarding.Tests.xUnit;

public sealed class EmployeeInputValidatorTests
{
    private readonly EmployeeInputValidator _sut = new();

    [Theory]
    [InlineData("", "asha@example.com", "Engineering")]
    [InlineData("Asha", "invalid-email", "Engineering")]
    [InlineData("Asha", "asha@example.com", "")]
    public void Validate_WhenInputHasMissingOrInvalidField_Throws(
        string name, string email, string department)
    {
        var input = new EmployeeInput(name, email, department);

        Assert.Throws<ArgumentException>(() => _sut.Validate(input));
    }

    [Fact]
    public void Validate_WhenInputIsValid_DoesNotThrow()
    {
        var input = new EmployeeInput(
            "Asha Rao", "asha@example.com", "Engineering");

        _sut.Validate(input);
    }
}


