using Microsoft.AspNetCore.Mvc;
using QuizzArena.Users.Application.Ports.In;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace QuizzArena.Users.Infrastructure.Adapters.In.Web
{
    [ApiController]
    [Route("api/users/professors")]
    internal class UserController : ControllerBase
    {
        private readonly IRegisterUserUseCase _registerUseCase;

        public UserController(IRegisterUserUseCase registerUseCase)
        {
            _registerUseCase = registerUseCase;
        }

        [HttpPost]
        public IActionResult RegisterProfessor()
        {
            return Ok("Profesor registrado");
        }
    }
}
