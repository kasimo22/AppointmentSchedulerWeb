using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSchedulerWeb.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        [Required]
        [Column("userName")]
        public string UserName { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("createDate")]
        public DateTime CreateDate { get; set; }

        [Column("createdBy")]
        public string CreatedBy { get; set; }

        [Column("lastUpdate")]
        public DateTime LastUpdate { get; set; }

        [Column("lastUpdateBy")]
        public string LastUpdateBy { get; set; }

        // 🔥 **Add this navigation property to fix the error**
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
