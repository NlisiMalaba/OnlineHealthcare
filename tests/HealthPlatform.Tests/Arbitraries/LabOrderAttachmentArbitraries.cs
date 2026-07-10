using FsCheck;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record LabOrderAttachmentCase(
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes);

public static class LabOrderAttachmentArbitraries
{
    public static Arbitrary<LabOrderAttachmentCase> LabOrderAttachmentCase()
    {
        var partnerCodeGen = Gen.Choose(3, 10)
            .Select(length => new string('P', length));
        var testCodeGen = Gen.Choose(2, 8)
            .Select(length => new string('T', length));
        var notesGen = Gen.Choose(5, 30)
            .Select(length => new string('N', length));

        return (from partnerCode in partnerCodeGen
                from testCode in testCodeGen
                from clinicalNotes in notesGen
                select new LabOrderAttachmentCase(partnerCode, testCode, clinicalNotes))
            .ToArbitrary();
    }
}
