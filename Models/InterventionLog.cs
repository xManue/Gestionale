using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class InterventionLog
    {
        public int Id { get; set; }
        public int InterventionId { get; set; }
        public Intervention Intervention { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
