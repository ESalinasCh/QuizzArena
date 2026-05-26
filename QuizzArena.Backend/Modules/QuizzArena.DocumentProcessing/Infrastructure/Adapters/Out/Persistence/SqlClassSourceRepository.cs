using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Persistence;

public class SqlClassSourceRepository : IClassSourceRepository
{
    public async Task<ClassSource> Create(UploadClassSourceRequestDto dto)
    {
        return new ClassSource
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CourseId = dto.CourseId,
            UserId = dto.UserId,
            Status = SourceStatus.Pending,
            Type = SourceType.Text,
            // UploadedAt = DateTimeOffset.UtcNow, falta poner esto en el class source 
            UpdatedAt = DateTimeOffset.UtcNow,
            Deleted = false
        };
    }
}
