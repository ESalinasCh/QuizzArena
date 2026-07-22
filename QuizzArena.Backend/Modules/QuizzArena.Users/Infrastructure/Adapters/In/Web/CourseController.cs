using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Users.Application.DTOs.Course;
using QuizzArena.Users.Application.Ports.In;

namespace QuizzArena.Users.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class CourseController(IGetTeacherCoursesUseCase getTeacherCoursesUseCase) : ControllerBase
{
    [HttpGet("users/me/courses")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<CourseDto>>> GetTeacherCourses()
    {
        string? userIdClaim = User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out Guid teacherId))
        {
            return Unauthorized();
        }

        List<CourseDto> courses = await getTeacherCoursesUseCase.Execute(teacherId);
        return Ok(courses);
    }
}
