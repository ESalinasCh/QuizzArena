using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/class-sources")]
public class ClassSourceController(
    IUploadSourceUseCase uploadSourceUseCase,
    IGetClassSourcesUseCase getClassSourcesUseCase
) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "teacher")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public async Task<ActionResult<UploadClassSourceResponseDto>> UploadClassSource([FromForm] UploadClassSourceRequestDto dto)
    {
        UploadClassSourceResponseDto response = await uploadSourceUseCase.Execute(dto);
        return Ok(response);
    }

    [HttpGet("/api/v{version:apiVersion}/users/me/class-sources")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<GetClassSourceResponseDto>>> GetMyClassSources()
    {
        string? userIdClaim = User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized();
        }

        List<GetClassSourceResponseDto> result = await getClassSourcesUseCase.Execute(userId);
        return Ok(result);
    }
}
