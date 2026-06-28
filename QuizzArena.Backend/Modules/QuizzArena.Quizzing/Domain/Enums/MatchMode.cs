using NpgsqlTypes;

namespace QuizzArena.Quizzing.Domain.Enums;

public enum MatchMode
{
    [PgName("single")]
    Solo = 0,
    Multiple = 1,
    Exam = 2
}
