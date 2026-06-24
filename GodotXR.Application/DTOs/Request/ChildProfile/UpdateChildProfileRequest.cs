namespace GodotXR.Application.DTOs.Request.ChildProfile
{
    public sealed class UpdateChildProfileRequest
    {
        public int? UserId { get; set; }

        public string? FullName { get; set; }

        public int? Age { get; set; }

        public string? Gender { get; set; }

        public string? LearningLevel { get; set; }

        public string? Note { get; set; }

        public string? Status { get; set; }
    }
}
