using System.ComponentModel.DataAnnotations;

namespace GodotXR.Domain.Entities
{
    public class ExerciseType : BaseEntity
    {
        [Required, MaxLength(200)]
        public string TypeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}