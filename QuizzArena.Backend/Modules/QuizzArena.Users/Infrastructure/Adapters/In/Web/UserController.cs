using Microsoft.AspNetCore.Mvc;
using QuizzArena.Users.Application.Ports.In;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace QuizzArena.Users.Infrastructure.Adapters.In.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
        ISignUpUserUseCase registerUseCase,
        ILogInUserUseCase loginUseCase
    ) : ControllerBase
    {

        [HttpGet]
        public IActionResult RegisterProfessor()
        {
            return Ok("Profesor registrado");
        }
    }
}
