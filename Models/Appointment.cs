using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSchedulerWeb.Models
{
    [Table("appointment")]
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = "not needed";
        public string Description { get; set; } = "not needed";
        public string Location { get; set; } = "not needed";
        public string Contact { get; set; } = "not needed";
        public string Url { get; set; } = "not needed";
        public string? Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? LastUpdateBy { get; set; }

        // Navigation Properties
        public Customer? Customer { get; set; }
        public User? User { get; set; }
    }
}
