using QuizzArena.Users.Application.DTOs.Course;

namespace QuizzArena.Users.Application.Ports.In;

public interface IGetTeacherCoursesUseCase
{
    Task<List<CourseDto>> Execute(Guid teacherId);
}
