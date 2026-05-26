using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IClassSourceRepository
{
    public Task<ClassSource> Create(UploadClassSourceRequestDto dto);
}
