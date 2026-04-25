using System.Collections.Generic;

namespace Backend.DTO
{
    public class CreateWorkTypeDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<string>? Materials { get; set; }
        public List<string>? Tools { get; set; }
    }
}

