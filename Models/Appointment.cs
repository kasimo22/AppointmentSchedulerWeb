using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSchedulerWeb.Models
{
    [Table("appointment")]
    public class Appointment
    {
        [Key]
        [Column("appointmentid")]
        public int AppointmentId { get; set; }

        [Column("customerid")]
        public int CustomerId { get; set; }

        [Column("userid")]
        public int UserId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("contact")]
        public string Contact { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("start")]
        public DateTime Start { get; set; }

        [Column("end")]
        public DateTime End { get; set; }

        [Column("createdate")]
        public DateTime CreateDate { get; set; }

        [Column("createdby")]
        public string CreatedBy { get; set; }

        [Column("lastupdate")]
        public DateTime LastUpdate { get; set; }

        [Column("lastupdateby")]
        public string LastUpdateBy { get; set; }

        // Navigation Properties
        public virtual Customer? Customer { get; set; }
        public virtual User? User { get; set; }
    }
}
