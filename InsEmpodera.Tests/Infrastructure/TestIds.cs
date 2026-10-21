namespace InsEmpodera.Tests.Infrastructure;

internal static class TestIds
{
    // Ids fixos dos dados iniciais (ApplicationDbContext.HasData e Database/Empodera.schema.sql).
    internal static int Seed(int value) => value;

    internal static int Missing => Seed(999_999);
}
