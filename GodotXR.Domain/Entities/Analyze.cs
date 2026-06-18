using System.ComponentModel.DataAnnotations.Schema;

namespace GodotXR.Domain.Entities
{
    public class Analyze : BaseEntity
    {
        public int ChildId { get; set; }

        public int TotalExercises { get; set; }

        public int CompletedExercises { get; set; }

        public int TotalPracticeTime { get; set; }

        public float AverageScore { get; set; }

        public string? ProgressLevel { get; set; }

        public string? Strengths { get; set; }

        public string? Weaknesses { get; set; }

        public string? Recommendation { get; set; }

        public DateTime? LastAnalyzedAt { get; set; }

        public DateTime? PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        // Navigation Properties
        [ForeignKey("ChildId")]
        public ChildProfile Child { get; set; } = null!;

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}