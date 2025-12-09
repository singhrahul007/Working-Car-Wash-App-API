// Entities/Review.cs
using System;

namespace CarWash.Api.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Service Service { get; set; }
        public virtual User User { get; set; }
    }
}