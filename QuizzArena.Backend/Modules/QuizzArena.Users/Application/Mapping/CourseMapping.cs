using AutoMapper;
using QuizzArena.Users.Application.DTOs.Course;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Mapping;

internal sealed class CourseMapping : Profile
{
    public CourseMapping()
    {
        CreateMap<Course, CourseDto>();
    }
}
