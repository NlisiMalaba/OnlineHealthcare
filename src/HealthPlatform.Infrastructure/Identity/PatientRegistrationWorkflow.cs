using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class PatientRegistrationWorkflow(
    UserManager<ApplicationUser> userManager,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    ISocialIdentityVerifier socialIdentityVerifier,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    ILogger<PatientRegistrationWorkflow> logger) : IPatientRegistrationWorkflow
{
    public async Task<PatientRegistrationResponseDto> RegisterAsync(
        RegisterPatientCommand command,
        CancellationToken ct)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var normalizedPhone = NormalizePhone(command.PhoneNumber);

        await EnsureNoIdentityConflictAsync(command.AuthProvider, normalizedPhone, normalizedEmail, ct);

        var (user, patient) = command.AuthProvider switch
        {
            PatientAuthProvider.Phone => await RegisterWithPhoneAsync(
                command.FullName,
                normalizedPhone!,
                command.Password!,
                ct),
            PatientAuthProvider.Email => await RegisterWithEmailAsync(
                command.FullName,
                normalizedEmail!,
                command.Password!,
                ct),
            PatientAuthProvider.Google => await RegisterWithSocialAsync(
                command,
                PatientAuthProvider.Google,
                SocialLoginProviders.Google,
                ct),
            PatientAuthProvider.Apple => await RegisterWithSocialAsync(
                command,
                PatientAuthProvider.Apple,
                SocialLoginProviders.Apple,
                ct),
            _ => throw new DomainException("UNSUPPORTED_AUTH_PROVIDER", "The auth provider is not supported.")
        };

        await patientRepository.AddAsync(patient, ct);

        var domainEvent = patient.DomainEvents.OfType<PatientRegisteredDomainEvent>().Single();
        await outboxRepository.EnqueueAsync(domainEvent, ct);
        await domainEventPublisher.PublishAsync(domainEvent, ct);

        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new InvalidOperationException("Health record was not created for the registered patient.");

        logger.LogInformation(
            "Registered patient {PatientId} with user {UserId} via {AuthProvider}.",
            patient.Id,
            user.Id,
            command.AuthProvider);

        return new PatientRegistrationResponseDto(patient.Id, healthRecord.Id, patient.CreatedAtUtc);
    }

    private async Task EnsureNoIdentityConflictAsync(
        PatientAuthProvider authProvider,
        string? phoneNumber,
        string? email,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber)
            && await patientRepository.ExistsByPhoneAsync(phoneNumber, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A patient account with this phone number already exists. Please sign in instead.");
        }

        if (!string.IsNullOrWhiteSpace(email)
            && await patientRepository.ExistsByEmailAsync(email, ct))
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A patient account with this email already exists. Please sign in instead.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser is not null)
            {
                throw new ConflictException(
                    IdentityErrorCodes.IdentityConflict,
                    "A patient account with this email already exists. Please sign in instead.");
            }
        }

        if (authProvider == PatientAuthProvider.Phone && !string.IsNullOrWhiteSpace(phoneNumber))
        {
            var existingUser = await userManager.FindByNameAsync(phoneNumber);
            if (existingUser is not null)
            {
                throw new ConflictException(
                    IdentityErrorCodes.IdentityConflict,
                    "A patient account with this phone number already exists. Please sign in instead.");
            }
        }
    }

    private async Task<(ApplicationUser User, Patient Patient)> RegisterWithPhoneAsync(
        string fullName,
        string phoneNumber,
        string password,
        CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = phoneNumber,
            PhoneNumber = phoneNumber,
            Email = ToPhoneIdentityEmail(phoneNumber)
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw MapIdentityFailure(createResult);
        }

        await userManager.AddToRoleAsync(user, ApplicationRoles.Patient);
        return (user, Patient.RegisterWithPhone(user.Id, fullName, phoneNumber));
    }

    private async Task<(ApplicationUser User, Patient Patient)> RegisterWithEmailAsync(
        string fullName,
        string email,
        string password,
        CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw MapIdentityFailure(createResult);
        }

        await userManager.AddToRoleAsync(user, ApplicationRoles.Patient);
        return (user, Patient.RegisterWithEmail(user.Id, fullName, email));
    }

    private async Task<(ApplicationUser User, Patient Patient)> RegisterWithSocialAsync(
        RegisterPatientCommand command,
        PatientAuthProvider provider,
        string loginProvider,
        CancellationToken ct)
    {
        var verified = await socialIdentityVerifier.VerifyAsync(provider, command.IdToken!, ct);

        var existingLogin = await userManager.FindByLoginAsync(loginProvider, verified.ProviderKey);
        if (existingLogin is not null)
        {
            throw new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "This social account is already registered. Please sign in instead.");
        }

        var normalizedEmail = NormalizeEmail(verified.Email);
        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser is not null)
            {
                throw new ConflictException(
                    IdentityErrorCodes.IdentityConflict,
                    "A patient account with this email already exists. Please sign in instead.");
            }
        }

        var userName = normalizedEmail ?? $"{loginProvider}:{verified.ProviderKey}";
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = userName,
            Email = normalizedEmail,
            EmailConfirmed = !string.IsNullOrWhiteSpace(normalizedEmail)
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            throw MapIdentityFailure(createResult);
        }

        var loginResult = await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(loginProvider, verified.ProviderKey, loginProvider));
        if (!loginResult.Succeeded)
        {
            throw MapIdentityFailure(loginResult);
        }

        await userManager.AddToRoleAsync(user, ApplicationRoles.Patient);

        var fullName = string.IsNullOrWhiteSpace(command.FullName)
            ? verified.FullName ?? userName
            : command.FullName;

        return (
            user,
            Patient.RegisterWithSocial(user.Id, fullName, provider, normalizedEmail, command.PhoneNumber));
    }

    private static Exception MapIdentityFailure(IdentityResult result)
    {
        var duplicate = result.Errors.Any(e =>
            e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)
            || e.Code.Contains("Already", StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            return new ConflictException(
                IdentityErrorCodes.IdentityConflict,
                "A patient account with this identifier already exists. Please sign in instead.");
        }

        var message = string.Join(" ", result.Errors.Select(e => e.Description));
        return new DomainException("REGISTRATION_FAILED", message);
    }

    private static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static string? NormalizePhone(string? phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

    private static string ToPhoneIdentityEmail(string phoneNumber) =>
        $"{phoneNumber.TrimStart('+')}@phone.healthplatform.local";

    private static class SocialLoginProviders
    {
        public const string Google = "Google";
        public const string Apple = "Apple";
    }
}
