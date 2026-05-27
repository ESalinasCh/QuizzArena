using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.Ports.Out;

public interface IClassSourceRepository
{
    public Task<ClassSource> Create(ClassSource classSource);
}
