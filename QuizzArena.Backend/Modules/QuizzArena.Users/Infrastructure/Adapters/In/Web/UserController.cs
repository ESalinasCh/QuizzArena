using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.In;

namespace QuizzArena.Users.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController(
    ISignUpUserUseCase signupUseCase,
    ILogInUserUseCase loginUseCase
) : ControllerBase
{
    // Placeholders Endpoints

    [HttpPost]
    [Route("signup")]
    public async Task<ActionResult<SignUpResponseDto>> SignUpProfessor(SignUpRequestDto dto)
    {
        SignUpResponseDto response = await signupUseCase.Execute(new SignUpRequestDto());
        return Ok(response);
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<LogInResponseDto>> LoginProfessor(LogInRequestDto dto)
    {
        LogInResponseDto response = await loginUseCase.Execute(dto);
        return Ok(response);
    }
}
