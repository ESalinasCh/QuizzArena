using Shared.Contracts.DTOs;

namespace Shared.Contracts;

public interface ICourseContract
{
    public Task<List<CourseSummaryDTO>> GetCoursesByStudent(Guid studentId);
    public Task<List<CourseSummaryDTO>> GetCoursesByTeacherId(Guid teacherId);
    public Task<List<CourseSummaryDTO>> GetCoursesByIds(List<Guid> courseIds);
    public Task<CourseDto?> GetCourseById(Guid courseId);
}
