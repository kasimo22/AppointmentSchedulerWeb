using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSchedulerWeb.Models
{
    [Table("customer")]
    public class Customer
    {
        [Key]
        [Column("customerid")]
        public int CustomerId { get; set; }

        [Column("customername")]
        public string CustomerName { get; set; }

        [Column("addressid")]
        public int AddressId { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("createdate")]
        public DateTime CreateDate { get; set; }

        [Column("createdby")]
        public string CreatedBy { get; set; }

        [Column("lastupdate")]
        public DateTime LastUpdate { get; set; }

        [Column("lastupdateby")]
        public string LastUpdateBy { get; set; }

        // Navigation Property
        public virtual Address Address { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
