namespace QuizzArena.Quizzing.Domain.Exceptions;

public class MaxAttemptsReachedException() : DomainException("MAX_ATTEMPTS_REACHED", "Maximum number of attempts reached for this match.");
