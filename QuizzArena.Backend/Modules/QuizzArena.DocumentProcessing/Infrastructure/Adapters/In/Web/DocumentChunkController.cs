using Microsoft.AspNetCore.Mvc;
using QuizzArena.DocumentProcessing.Application.DTOs.DocumentChunk;
using QuizzArena.DocumentProcessing.Application.Ports.In;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class DocumentChunkController(
    ICreateDocumentUseCase createDocumentUseCase
) : ControllerBase
{
    // Placeholders Endpoints
    [HttpPost]
    public async Task<ActionResult<DocumentChunkDto>> CreateDocumentChunk(CreateDocumentDto dto)
    {
        DocumentChunkDto response = await createDocumentUseCase.Execute(dto);
        return Ok(response);
    }
}
