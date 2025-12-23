using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }  // changed to Guid

        [Required]
        public int RoleId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}