namespace AppointmentSchedulerWeb.Models
{
    public class Country
    {
        public int CountryId { get; set; }
        public string? Name { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? LastUpdateBy { get; set; }

        // Navigation Property
        public ICollection<City>? Cities { get; set; }
    }
}
