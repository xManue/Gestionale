using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public RoleEnum Role { get; set; } = RoleEnum.Employee;
        public bool IsActive { get; set; } = true;
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<InterventionLog> Logs { get; set; } = new List<InterventionLog>();
    }

    public enum RoleEnum
    {
        Admin,
        Employee,
    }
}
