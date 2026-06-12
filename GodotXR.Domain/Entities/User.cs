using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodotXR.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Specialty { get; set; } = string.Empty;
        // Foreign Keys
        public int RoleId { get; set; }

        // Navigation Properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }
}
