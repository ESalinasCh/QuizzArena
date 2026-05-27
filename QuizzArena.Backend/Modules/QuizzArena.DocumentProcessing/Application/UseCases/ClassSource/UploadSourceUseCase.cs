using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Ports.In;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Application.Services;
using QuizzArena.DocumentProcessing.Application.Validators;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Application.UseCases;

public class UploadSourceUseCase(
    UploadClassSourceRequestValidator uploadValidator,
    IMapper mapper,
    IClassSourceRepository classSourceRepository) : IUploadSourceUseCase
{
    public async Task<UploadClassSourceResponseDto> Execute(UploadClassSourceRequestDto dto)
    {
        await uploadValidator.ValidateAndThrowAsync(dto);
        ClassSource classSource = mapper.Map<ClassSource>(dto);

        //metodo para identificar type
        classSource.Type = SourceTypeResolver.Resolve(dto.File.FileName);

        //subir al blob y get fileurl para despues :)
        // agregarlo a base de datos
        ClassSource createdClass = await classSourceRepository.Create(classSource);

        var createdClaseRespose = mapper.Map<UploadClassSourceResponseDto>(createdClass);
        // syncronizar base de datos
        // devolver respuesta created y el objeto
        return createdClaseRespose;
    }
}

// Port In -> UseCase
// Port Out -> Out Adapters
// In adapters (controlladores)
