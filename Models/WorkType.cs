using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class WorkType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<WorkTypeMaterial> Materials { get; set; } = new List<WorkTypeMaterial>();
        public ICollection<WorkTypeTool> Tools { get; set; } = new List<WorkTypeTool>();
    }
}
