using AutoMapper;
using GodotXR.Application.DTOs.Response.Lesson;
using GodotXR.Application.DTOs.Response.Program;
using GodotXR.Application.DTOs.Response.SchoolYear;
using GodotXR.Domain.Entities;
using System.Security.Claims;

namespace GodotXR.Infrastructure.Mappings
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<User, IEnumerable<Claim>>().ConvertUsing(src => new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, src.Id.ToString()),
                new Claim(ClaimTypes.Email, src.Email),
                new Claim(ClaimTypes.Name, src.FullName),
                new Claim(ClaimTypes.Role, src.Role != null ? src.Role.RoleName.ToString() : "Lecture")
            });
            CreateMap<Program, ProgramResponse>()
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src =>
                    src.Lessons.Where(l => !l.IsDeleted).ToList()));
            CreateMap<Lesson, LessonSummaryResponse>();
            CreateMap<Lesson, LessonResponse>();
            CreateMap<SchoolYear, SchoolYearResponse>()
       .ForMember(dest => dest.SemesterCount, opt => opt.MapFrom(src =>
           src.Semesters.Count(s => !s.IsDeleted)));
        }
    }
}