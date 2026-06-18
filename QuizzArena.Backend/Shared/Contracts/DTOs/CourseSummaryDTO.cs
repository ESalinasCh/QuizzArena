namespace Shared.Contracts.DTOs;

public record CourseSummaryDTO
{
    public Guid Id { get; set; }
    public string CourseName { get; set; } = "";
    public string ProfessorName { get; set; } = "";

}
