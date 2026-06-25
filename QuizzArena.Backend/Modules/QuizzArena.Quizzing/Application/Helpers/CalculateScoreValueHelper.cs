namespace QuizzArena.Quizzing.Application.Helpers;

internal static class CalculateScoreValueHelper
{
    internal static int Resolve(int questionsAmount, int score)
    {
        int _questionsAmount = questionsAmount;
        int _score = score;

        return _score / _questionsAmount;
    }

}
