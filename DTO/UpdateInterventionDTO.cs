using Backend.Models;

namespace Backend.DTO
{
    public class UpdateInterventionDTO
    {
        public string? TitleOverride { get; set; }
        public string? DescriptionOverride { get; set; }
        public string? Location { get; set; }
        public InterventionPriority? Priority { get; set; }
        public DateTimeOffset? DateStart { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
        public List<int>? UserIds { get; set; }
    }
}
