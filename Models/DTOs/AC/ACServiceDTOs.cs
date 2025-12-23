namespace CarWash.Api.Models.DTOs.AC
{
    public class ACServiceDTOs
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public string DurationDisplay { get; set; } = string.Empty;
        public List<string> Includes { get; set; } = new List<string>();
        public bool IsPopular { get; set; }
        public int DisplayOrder { get; set; }
    }
}
