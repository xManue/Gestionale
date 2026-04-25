using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class InterventionMaterial
    {
        public int Id { get; set; }
        public int InterventionId { get; set; }
        public Intervention Intervention { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double Quantity { get; set; }
    }
}
