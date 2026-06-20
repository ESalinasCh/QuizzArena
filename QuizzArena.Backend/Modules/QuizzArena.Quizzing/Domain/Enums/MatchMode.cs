using NpgsqlTypes;

namespace QuizzArena.Quizzing.Domain.Enums;

public enum MatchMode
{
    // TODO
    [PgName("single")]
    Solo = 0,
    Multiple = 1,
}
