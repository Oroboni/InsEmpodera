namespace InsEmpodera.Tests.Infrastructure;

internal static class TestIds
{
    // Matches the deterministic seed keys configured in ApplicationDbContext.
    internal static Guid Seed(int value) =>
        Guid.ParseExact($"000000000000000000000000{value:X8}", "N");

    internal static Guid Missing => Seed(999_999);
}
