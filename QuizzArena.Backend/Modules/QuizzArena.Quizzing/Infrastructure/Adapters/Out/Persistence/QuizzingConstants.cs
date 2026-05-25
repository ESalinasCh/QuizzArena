using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence
{
    internal static class QuizzingConstants
    {
        public const string Schema = "quizzing";

        public static Guid CourseId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        public static Guid QuizzId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public static Guid QuestionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public static Guid OptionFalse1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public static Guid OptionFalse2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

        public static Guid OptionFalse3Id = Guid.Parse("66666666-6666-6666-6666-666666666666");

        public static Guid OptionTrueId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        public static Guid MatchId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    }
}
