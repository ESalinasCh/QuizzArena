namespace QuizzArena.Quizzing.Domain.Exceptions;

public class ActiveAttemptExistsException() : DomainException("ACTIVE_ATTEMPT_EXISTS", "User already has an active attempt for this match.");
