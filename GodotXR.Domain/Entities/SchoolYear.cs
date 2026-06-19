using System.ComponentModel.DataAnnotations;

namespace GodotXR.Domain.Entities
{
    public class SchoolYear : BaseEntity
    {
        public int ClassCount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        // Navigation Properties
        public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    }
}