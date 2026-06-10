using GodotXR.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GodotXR.Application.DTOs.Request.User
{
    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? Specialty { get; set; }

        public UserRole? RoleName { get; set; }

        public bool? IsActive { get; set; }
    }
}