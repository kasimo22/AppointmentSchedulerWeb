using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSchedulerWeb.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("username")]
        public string UserName { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }

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

        // Navigation
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
