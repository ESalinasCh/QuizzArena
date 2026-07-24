namespace QuizzArena.Quizzing.Domain.Exceptions;

public class InvalidSelectedOptionException(Guid optionId, Guid questionId) : DomainException("INVALID_SELECTED_OPTION", $"Option {optionId} does not belong to question {questionId}.");
