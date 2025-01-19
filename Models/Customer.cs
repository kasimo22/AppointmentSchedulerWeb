using System.ComponentModel.DataAnnotations;

namespace AppointmentSchedulerWeb.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int AddressId { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? LastUpdateBy { get; set; }

        // Navigation Property
        public Address Address { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
