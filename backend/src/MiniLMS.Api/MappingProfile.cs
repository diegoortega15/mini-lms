using AutoMapper;
using MiniLMS.Core.DTOs;
using MiniLMS.Core.Models;

namespace MiniLMS.Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Course, CourseDto>();
        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>();
        
        CreateMap<ImportJob, ImportJobDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title));
    }
}
