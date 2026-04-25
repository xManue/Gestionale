using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public int InterventionId { get; set; }
        public Intervention Intervention { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
