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

namespace QuizzArena.DocumentProcessing.Application.UseCases;

public class UploadSourceUseCase(
    UploadClassSourceRequestValidator uploadValidator,
    IMapper mapper,
    IBlobRepository blobRepository,
    IClassSourceRepository classSourceRepository,
    IPublishEndpoint publishEnpoint
) : IUploadSourceUseCase
{
    public async Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto)
    {
        await uploadValidator.ValidateAndThrowAsync(dto);
        ClassSource classSource = mapper.Map<ClassSource>(dto);

        classSource.Type = SourceTypeResolver.Resolve(dto.File.FileName);


        using var stream = dto.File.OpenReadStream();

        string blobPath = $"class_{classSource.Id}/{classSource.Type}{Path.GetExtension(dto.File.FileName)}";

        string fileUrl = await blobRepository.UploadFileAsync(stream, blobPath, "quiz-sources");

        classSource.FileUrl = fileUrl;


        ClassSource createdClass = await classSourceRepository.CreateAsync(classSource);


        await publishEnpoint.Publish(new TranscriptionRequestEvent
        {
            ClassSourceId = createdClass.Id,
            FileUrl = createdClass.FileUrl!
        });


        var createdClaseRespose = mapper.Map<UploadClassSourceResponseDto>(createdClass);
        return createdClaseRespose;
    }
}

