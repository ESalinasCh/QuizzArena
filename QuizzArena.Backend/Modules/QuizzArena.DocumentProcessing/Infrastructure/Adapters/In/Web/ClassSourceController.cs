using Microsoft.AspNetCore.Mvc;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ClassSourceController(
        IUploadSourceUseCase uploadSourceUseCase
    ) : ControllerBase
    {
        [HttpPost]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<ActionResult<UploadClassSourceResponseDto>> UploadClassSource( [FromForm] UploadClassSourceRequestDto dto)
        {
            UploadClassSourceResponseDto response = await uploadSourceUseCase.Execute(dto);
            return Ok(response);
        }
    }
}
