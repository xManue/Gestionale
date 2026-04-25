using Backend.Models;

namespace Backend.DTO
{
    public class InterventionDTO
    {
        public int WorkTypeId { get; set; }
        public DateTimeOffset DateStart { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
        public InterventionPriority Priority { get; set; } = InterventionPriority.Low;
        public string? Location { get; set; }
        public string? TitleOverride { get; set; }
        public string? DescriptionOverride { get; set; }
    }
}
