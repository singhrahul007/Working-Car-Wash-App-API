using System;
using System.Collections.Generic;

namespace CarWash.Api.DTOs
{
    using System;
    using System.Collections.Generic;

    namespace CarWash.Api.DTOs
    {
        public class ServiceDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string SubCategory { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int DurationInMinutes { get; set; }
            public List<string> Includes { get; set; } = new List<string>();
            public string ImageUrl { get; set; } = string.Empty;
            public bool IsPopular { get; set; }
            public double Rating { get; set; }
            public int ReviewCount { get; set; }
            // Remove this line: public string Reviews { get; set; } // This shouldn't be here
            public List<string> AvailableSlots { get; set; } = new List<string>();
            public List<DateTime> UnavailableDates { get; set; } = new List<DateTime>();
        }
    }
    public class ServiceCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public List<string> Includes { get; set; } = new List<string>();
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public List<string> AvailableSlots { get; set; } = new List<string>();
    }

    public class ServiceAvailabilityDto
    {
        public DateTime Date { get; set; }
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
    }

    public class TimeSlotDto
    {
        public string Time { get; set; } = string.Empty; // "15:00"
        public bool IsAvailable { get; set; }
        public int AvailableCount { get; set; }
    }
}