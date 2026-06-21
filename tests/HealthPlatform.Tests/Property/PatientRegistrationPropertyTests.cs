using FsCheck.Xunit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class PatientRegistrationPropertyTests
{
    // Feature: online-healthcare-platform, Property 1: Patient Registration Creates Linked Health Record
    [Property(Arbitrary = [typeof(PatientRegistrationArbitraries)], MaxTest = 100)]
    public bool Registration_always_creates_exactly_one_linked_health_record(ValidPatientRegistration input) =>
        RunHealthRecordLinkedInvariantAsync(input).GetAwaiter().GetResult();

    // Feature: online-healthcare-platform, Property 3: Duplicate Registration Rejection
    [Property(Arbitrary = [typeof(PatientRegistrationArbitraries)], MaxTest = 100)]
    public bool Duplicate_phone_or_email_registration_is_rejected_with_identity_conflict(
        DuplicateRegistrationCase duplicateCase) =>
        RunDuplicateRejectionInvariantAsync(duplicateCase).GetAwaiter().GetResult();

    private static async Task<bool> RunHealthRecordLinkedInvariantAsync(ValidPatientRegistration registration)
    {
        await using var host = new PatientRegistrationTestHost();

        var response = await host.Sender.Send(registration.ToCommand(), CancellationToken.None);

        var patientExists = await host.DbContext.Patients
            .AnyAsync(p => p.Id == response.PatientId);
        if (!patientExists)
        {
            return false;
        }

        var linkedRecords = await host.DbContext.HealthRecords
            .Where(r => r.PatientId == response.PatientId)
            .ToListAsync();

        if (linkedRecords.Count != 1)
        {
            return false;
        }

        var record = linkedRecords[0];
        return record.Id == response.HealthRecordId
            && record.PatientId == response.PatientId;
    }

    private static async Task<bool> RunDuplicateRejectionInvariantAsync(DuplicateRegistrationCase duplicateCase)
    {
        await using var host = new PatientRegistrationTestHost();

        await host.Sender.Send(duplicateCase.FirstCommand(), CancellationToken.None);

        try
        {
            await host.Sender.Send(duplicateCase.SecondCommand(), CancellationToken.None);
            return false;
        }
        catch (ConflictException ex)
        {
            return ex.Code == IdentityErrorCodes.IdentityConflict;
        }
    }
}
