namespace AppointmentSchedulerWeb.Models
{
    public class City
    {
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public int CountryId { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? LastUpdateBy { get; set; }

        // Navigation Property
        public Country? Country { get; set; }
        public ICollection<Address>? Addresses { get; set; }
    }
}
