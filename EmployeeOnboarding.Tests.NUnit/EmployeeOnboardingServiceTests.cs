using EmployeeOnboarding.Core.Abstractions;
using EmployeeOnboarding.Core.Models;
using EmployeeOnboarding.Core.Services;
using EmployeeOnboarding.Core.Validation;
using Moq;
using NUnit.Framework;

namespace EmployeeOnboarding.Tests.NUnit;

[TestFixture]
public sealed class EmployeeOnboardingServiceTests
{
    private Mock<IEmployeeRepository> _repository = null!;
    private Mock<IAccountProvisioner> _provisioner = null!;
    private Mock<IWelcomeNotifier> _notifier = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IEmployeeRepository>();
        _provisioner = new Mock<IAccountProvisioner>();
        _notifier = new Mock<IWelcomeNotifier>();
    }

    private EmployeeOnboardingService CreateSut() =>
        new(_repository.Object, _provisioner.Object,
            _notifier.Object, new EmployeeInputValidator());

    [Test]
    public async Task OnboardAsync_WhenAllStepsSucceed_ReturnsEmployeeId()
    {
        var input = new EmployeeInput(
            "Asha Rao", "asha@example.com", "Engineering");
        _repository.Setup(x => x.AddAsync(
            input, It.IsAny<CancellationToken>())).ReturnsAsync(501);

        var result = await CreateSut().OnboardAsync(input);

        Assert.That(result, Is.EqualTo(501));
        _repository.Verify(x => x.AddAsync(
            input, It.IsAny<CancellationToken>()), Times.Once);
        _provisioner.Verify(x => x.ProvisionAsync(
            input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(x => x.SendAsync(
            input.Email, input.Name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void OnboardAsync_WhenInputIsInvalid_ThrowsAndDoesNotPersist()
    {
        var input = new EmployeeInput("", "bad-email", "Engineering");

        Assert.ThrowsAsync<ArgumentException>(
            async () => await CreateSut().OnboardAsync(input));

        _repository.Verify(x => x.AddAsync(
            It.IsAny<EmployeeInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void OnboardAsync_WhenRepositoryFails_PropagatesFailure()
    {
        var input = new EmployeeInput(
            "Asha Rao", "asha@example.com", "Engineering");
        _repository.Setup(x => x.AddAsync(
            input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await CreateSut().OnboardAsync(input));

        _provisioner.Verify(x => x.ProvisionAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
