namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;

internal static class UserConstants
{
    public const string Schema = "users";

    public static Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static Guid TeacherId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static Guid ProviderTeacherId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static Guid StudentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static Guid ProviderStudentId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public static Guid CourseId = Guid.Parse("44444444-4444-4444-4444-444444444444");
}
