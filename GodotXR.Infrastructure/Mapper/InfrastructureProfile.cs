using AutoMapper;
using GodotXR.Application.DTOs.Response.Lesson;
using GodotXR.Application.DTOs.Response.Program;
using GodotXR.Application.DTOs.Response.SchoolYear;
using GodotXR.Application.DTOs.Response.Semester;
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

            CreateMap<Semester, SemesterResponse>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? src.Teacher.FullName : string.Empty))
                .ForMember(dest => dest.SchoolYearName, opt => opt.MapFrom(src =>
                    src.SchoolYear != null ? src.SchoolYear.YearName : string.Empty))
                .ForMember(dest => dest.SchoolYearStartDate, opt => opt.MapFrom(src =>
                    src.SchoolYear.StartDate))
                .ForMember(dest => dest.SchoolYearEndDate, opt => opt.MapFrom(src =>
                    src.SchoolYear.EndDate))
                .ForMember(dest => dest.SchoolYearStatus, opt => opt.MapFrom(src =>
                    src.SchoolYear.Status))
                .ForMember(dest => dest.ClassroomCount, opt => opt.MapFrom(src =>
                    src.Classrooms.Count(c => !c.IsDeleted)));
        }
    }
}