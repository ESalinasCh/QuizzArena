using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class ClassSourceController(
    IUploadSourceUseCase uploadSourceUseCase,
    IGetClassSourcesUseCase getClassSourcesUseCase
) : ControllerBase
{
    [HttpPost("class-sources")]
    [Authorize(Roles = "teacher")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public async Task<ActionResult<UploadClassSourceResponseDto>> UploadClassSource([FromForm] UploadClassSourceRequestDto dto)
    {
        UploadClassSourceResponseDto response = await uploadSourceUseCase.Execute(dto);
        return Ok(response);
    }

    [HttpGet("users/me/class-sources")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<GetClassSourceResponseDto>>> GetMyClassSources(
        [FromQuery] PagedRequest query
    )
    {
        string? userIdClaim = User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized();
        }

        List<GetClassSourceResponseDto> result = await getClassSourcesUseCase.Execute(userId, query);
        return Ok(result);
    }
}
