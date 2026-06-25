using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotXR.Application.DTOs.Request.Result
{
    public class SubmitResultRequest
    {
        public string SessionId { get; set; } = null!;

        public int ChildId { get; set; }
        public int ExerciseId { get; set; }

        public string CompletionStatus { get; set; } = null!; 
        public float Score { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int DurationSeconds { get; set; }
        public string? AudioRecordUrl { get; set; }
        public string? ReplayDataUrl { get; set; }
        public string? InteractionLog { get; set; }
        public string? FeedbackText { get; set; }
        public List<SubmitPronunciationDetailRequest> PronunciationDetails { get; set; } = new();

        public List<SubmitEventLogRequest> EventLogs { get; set; } = new();
    }
}
