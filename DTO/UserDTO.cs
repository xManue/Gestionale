using Backend.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.ModelsDTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public RoleEnum Role { get; set; }
        public string Email { get; set; } = null!;
    }
}
