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
        public async Task<ActionResult<UploadClassSourceResponseDto>> UploadClassSource(UploadClassSourceRequestDto dto)
        {
            UploadClassSourceResponseDto response = await uploadSourceUseCase.Execute(dto);
            return Ok(response);
        }
    }
}
