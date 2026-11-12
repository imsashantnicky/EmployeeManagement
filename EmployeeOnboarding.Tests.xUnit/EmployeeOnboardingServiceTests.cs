using EmployeeOnboarding.Core.Abstractions;
using EmployeeOnboarding.Core.Models;
using EmployeeOnboarding.Core.Services;
using EmployeeOnboarding.Core.Validation;
using Moq;
using Xunit;

namespace EmployeeOnboarding.Tests.xUnit;

public sealed class EmployeeOnboardingServiceTests
{
    private readonly Mock<IEmployeeRepository> _repository = new();
    private readonly Mock<IAccountProvisioner> _provisioner = new();
    private readonly Mock<IWelcomeNotifier> _notifier = new();

    private EmployeeOnboardingService CreateSut() =>
        new(_repository.Object, _provisioner.Object,
            _notifier.Object, new EmployeeInputValidator());

    [Fact]
    public async Task OnboardAsync_WhenAllStepsSucceed_ReturnsEmployeeId()
    {
        var input = new EmployeeInput(
            "Asha Rao", "asha@example.com", "Engineering");

        _repository.Setup(x => x.AddAsync(
            input, It.IsAny<CancellationToken>())).ReturnsAsync(501);

        var sut = CreateSut();

        var result = await sut.OnboardAsync(input);

        Assert.Equal(501, result);
        _repository.Verify(x => x.AddAsync(
            input, It.IsAny<CancellationToken>()), Times.Once);
        _provisioner.Verify(x => x.ProvisionAsync(
            input.Email, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(x => x.SendAsync(
            input.Email, input.Name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnboardAsync_WhenInputIsInvalid_DoesNotCallDependencies()
    {
        var input = new EmployeeInput("", "bad-email", "Engineering");
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.OnboardAsync(input));

        _repository.Verify(x => x.AddAsync(
            It.IsAny<EmployeeInput>(), It.IsAny<CancellationToken>()), Times.Never);
        _provisioner.Verify(x => x.ProvisionAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnboardAsync_WhenRepositoryFails_DoesNotProvisionOrNotify()
    {
        var input = new EmployeeInput(
            "Asha Rao", "asha@example.com", "Engineering");

        _repository.Setup(x => x.AddAsync(
            input, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage unavailable"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.OnboardAsync(input));

        _provisioner.Verify(x => x.ProvisionAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(x => x.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnboardAsync_PassesCancellationTokenToDependencies()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var input = new EmployeeInput(
            "Asha Rao", "asha@example.com", "Engineering");

        _repository.Setup(x => x.AddAsync(input, token)).ReturnsAsync(501);

        var sut = CreateSut();

        await sut.OnboardAsync(input, token);

        _repository.Verify(x => x.AddAsync(input, token), Times.Once);
        _provisioner.Verify(x => x.ProvisionAsync(input.Email, token), Times.Once);
        _notifier.Verify(x => x.SendAsync(input.Email, input.Name, token), Times.Once);
    }
}
