using AutoMapper;
using FluentValidation;
using MassTransit;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Helpers;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Application.Validators;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;
using Shared.Messaging.Events;

namespace QuizzArena.DocumentProcessing.Application.UseCases;

public class UploadSourceUseCase(
    UploadClassSourceRequestValidator uploadValidator,
    IMapper mapper,
    IStorageServiceRepository storageServiceRepository,
    IClassSourceRepository classSourceRepository,
    ICourseContract courseContract,
    ICurrentUser currentUser,
    IPublishEndpoint publishEnpoint
) : IUploadSourceUseCase
{
    public async Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto)
    {
        await uploadValidator.ValidateAndThrowAsync(dto);

        Guid userId = Guid.Parse(currentUser.UserId);
        ClassSource classSource = mapper.Map<ClassSource>(dto);
        classSource.UserId = userId;
        classSource.Type = SourceTypeResolver.Resolve(dto.File.FileName);

        CourseDto? course = await courseContract.GetCourseById(dto.CourseId) ?? throw new InvalidOperationException("Course not found");
        if (course.TeacherId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to upload sources for this course.");
        }

        using var stream = dto.File.OpenReadStream();
        string blobPath = $"class_{classSource.Id}/{classSource.Type}{Path.GetExtension(dto.File.FileName)}";
        string fileUrl = await storageServiceRepository.UploadFileAsync(stream, blobPath, "quiz-sources");
        classSource.FileUrl = fileUrl;

        if (classSource.Type == SourceType.Text)
        {
            classSource.TranscriptUrl = fileUrl;
            classSource.Status = SourceStatus.Completed;
        }

        ClassSource createdClass = await classSourceRepository.CreateAsync(classSource);

        if (createdClass.Type == SourceType.Text)
        {
            await publishEnpoint.Publish(new TranscriptionCompletedEvent
            {
                ClassSourceId = createdClass.Id,
                TranscriptUrl = createdClass.TranscriptUrl!
            });
        }
        else
        {
            await publishEnpoint.Publish(new TranscriptionRequestEvent
            {
                ClassSourceId = createdClass.Id,
                FileUrl = createdClass.FileUrl!
            });
        }

        var createdClaseRespose = mapper.Map<UploadClassSourceResponseDto>(createdClass);
        return createdClaseRespose;
    }
}

