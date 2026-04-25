using System.Collections.Generic;

namespace Backend.DTO
{
    public class WorkTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<string> Materials { get; set; } = new();
        public List<string> Tools { get; set; } = new();
    }
}

