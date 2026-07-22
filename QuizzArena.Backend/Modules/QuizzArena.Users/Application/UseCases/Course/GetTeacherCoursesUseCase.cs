using AutoMapper;
using QuizzArena.Users.Application.DTOs.Course;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;

namespace QuizzArena.Users.Application.UseCases.Course;

internal sealed class GetTeacherCoursesUseCase(
    ICourseQueriesRepository repository,
    IMapper mapper
) : IGetTeacherCoursesUseCase
{
    public async Task<List<CourseDto>> Execute(Guid teacherId)
    {
        List<Domain.Entities.Course> courses = await repository.GetCoursesByTeacherId(teacherId);
        return mapper.Map<List<CourseDto>>(courses);
    }
}
