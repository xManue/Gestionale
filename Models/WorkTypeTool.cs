using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class WorkTypeTool
    {
        public int Id { get; set; }
        public int WorkTypeId { get; set; }
        public WorkType WorkType { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
