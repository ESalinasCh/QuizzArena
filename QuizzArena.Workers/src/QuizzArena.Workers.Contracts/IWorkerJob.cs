namespace QuizzArena.Workers.Contracts;

/// <summary>
/// Contrato base que debe implementar cualquier evento de "trabajo" que este
/// ecosistema de workers pueda procesar (compresión de audio, transcodeo de video,
/// generación de thumbnails, etc.)
/// </summary>
public interface IWorkerJob
{
    Guid JobId { get; }
    Guid CorrelationId { get; }
}