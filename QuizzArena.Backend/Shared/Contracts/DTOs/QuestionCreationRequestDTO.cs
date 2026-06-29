
namespace Shared.Contracts.DTOs;

public record QuestionCreationRequestDTO
{
    public Guid ProcessingJobId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new List<string>();
    public int CorrectAnswer { get; set; }
    public string Justification { get; set; } = string.Empty;
}
