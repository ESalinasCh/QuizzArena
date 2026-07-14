namespace QuizzArena.Quizzing.Domain.Exceptions;

public class AttemptAlreadyCompletedException() : DomainException("ATTEMPT_ALREADY_COMPLETED", "User already completed this match attempt.");
