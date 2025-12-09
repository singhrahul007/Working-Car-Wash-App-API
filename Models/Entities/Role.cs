using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // "admin", "customer", "staff"

        [MaxLength(200)]
        public string Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}