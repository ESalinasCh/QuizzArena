
using AutoMapper;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;

namespace QuizzArena.DocumentProcessing.Application.Mapping;

public class UploadClassSourceMappingProfile : Profile
{
    public UploadClassSourceMappingProfile()
    {
        CreateMap<UploadClassSourceRequestDto, ClassSource>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(_ => Guid.NewGuid())
            )

            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Name.Trim())
            )

            .ForMember(
                dest => dest.Status,
                opt => opt.MapFrom(_ => SourceStatus.Pending)
            )

            .ForMember(
                dest => dest.CreatedAt,
                opt => opt.MapFrom(_ => DateTimeOffset.UtcNow)
            )

            .ForMember(
                dest => dest.UpdatedAt,
                opt => opt.MapFrom(_ => DateTimeOffset.UtcNow)
            )

            .ForMember(
                dest => dest.Type,
                opt => opt.Ignore()
            )

            .ForMember(
                dest => dest.FileUrl,
                opt => opt.Ignore()
            )

            .ForMember(
                dest => dest.TranscriptUrl,
                opt => opt.Ignore()
            )

            .ForMember(
                dest => dest.DocumentChunks,
                opt => opt.Ignore()
            ); 

        CreateMap<ClassSource, UploadClassSourceResponseDto>()
            .ForMember(
                dest => dest.SourceType,
                opt => opt.MapFrom(src => src.Type)
                );

    }
}
