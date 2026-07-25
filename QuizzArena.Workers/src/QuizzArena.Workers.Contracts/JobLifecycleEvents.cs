namespace QuizzArena.Workers.Contracts;

/// <summary>
/// Evento genérico de finalización exitosa. Se reutiliza para CUALQUIER tipo de job,
/// no solo para compresión de audio -> tu saga solo necesita un consumer para esto,
/// sin importar cuántos tipos de job agregues después.
/// </summary>
public interface IJobCompleted
{
    Guid JobId { get; }
    Guid CorrelationId { get; }
    string JobType { get; }
    string OutputBlobUrl { get; }
    DateTime CompletedAtUtc { get; }
}

public interface IJobFaulted
{
    Guid JobId { get; }
    Guid CorrelationId { get; }
    string JobType { get; }
    string Reason { get; }
    DateTime FailedAtUtc { get; }
}