using CarWash.Api.Models;
using CarWash.Api.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Entities
{
    public class ServiceReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }    // changed to Guid

        [Required]
        public int ServiceId { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(ServiceId))]
        public virtual Service Service { get; set; } = null!;
    }
}