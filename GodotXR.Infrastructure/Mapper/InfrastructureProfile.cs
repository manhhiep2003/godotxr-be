using AutoMapper;
using GodotXR.Application.DTOs.Response.Enrollment;
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
                new Claim(ClaimTypes.Role, src.Role != null ? src.Role.RoleName.ToString() : "Parent")

            });
            CreateMap<Enrollment, EnrollmentResponse>()
    .ForMember(dest => dest.ChildFullName, opt => opt.MapFrom(src => src.Child.FullName))
    .ForMember(dest => dest.ChildLearningLevel, opt => opt.MapFrom(src => src.Child.LearningLevel))
    .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Classroom.ClassName));
        }
    }
}