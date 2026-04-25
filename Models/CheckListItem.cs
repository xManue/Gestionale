namespace Backend.Models
{
    public class CheckListItem
    {
        public int Id { get; set; }

        public int InterventionId { get; set; }
        public Intervention Intervention { get; set; } = null!;

        public string Title { get; set; } = null!;

        public bool IsCompleted { get; set; } = false;
    }
}
