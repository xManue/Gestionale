using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class Intervention
    {
        public int Id { get; set; }
        public string TitleOverride { get; set; } = null!;
        public int? WorkTypeId { get; set; }
        public WorkType? WorkType { get; set; } = null;
        public DateTimeOffset DateStart { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
        public string? Location { get; set; }
        public InterventionPriority Priority { get; set; }
        public string? DescriptionOverride { get; set; }
        public InterventionStatus Stato { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? CompletedAt { get;set; }
        public User? CompletedByUser { get; set; }
        public int? CompletedByUserId { get; set; }
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<InterventionMaterial> Materials { get; set; } = new List<InterventionMaterial>();
        public ICollection<InterventionLog> Logs { get; set; } = new List<InterventionLog>();
        public ICollection<CheckListItem> ChecklistItems { get; set; } = new List<CheckListItem>();
    }

    public enum InterventionStatus
    {
        Planned = 0,
        Assigned = 1,
        InProgress = 2,
        OnHold = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum InterventionPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
