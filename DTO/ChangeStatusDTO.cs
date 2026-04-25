using Backend.Models;

namespace Backend.DTO
{
    public class ChangeStatusDTO
    {
        public int InterventionId { get; set; }
        public InterventionStatus newStatus {  get; set; }
        public int UserId { get; set; }
    }
}
