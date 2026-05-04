namespace Backend.DTO
{
    public class InterventionResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTimeOffset DateStart { get; set; }
        public DateTimeOffset? DateEnd { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public List<AssignedUserDTO> AssignedTo { get; set; } = new();
    }
}
