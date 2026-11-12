using EmployeeOnboarding.Core.Services;
using EmployeeOnboarding.Core.Validation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<EmployeeInputValidator>();
services.AddTransient<EmployeeOnboardingService>();

// Register concrete infrastructure adapters here:
// services.AddScoped<IEmployeeRepository, SqlEmployeeRepository>();
// services.AddTransient<IAccountProvisioner, EntraAccountProvisioner>();
// services.AddTransient<IWelcomeNotifier, SmtpWelcomeNotifier>();

using var provider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateScopes = true });

// Resolve only after all concrete adapters have been registered.
// var onboarding = provider.GetRequiredService<EmployeeOnboardingService>();
