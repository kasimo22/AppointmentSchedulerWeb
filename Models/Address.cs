using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSchedulerWeb.Models
{
    [Table("address")]
    public class Address
    {
        [Key]
        [Column("addressId")]
        public int AddressId { get; set; }

        [Column("address")]
        [Required(ErrorMessage = "Address is required.")]
        public string? AddressLine1 { get; set; }

        [Column("address2")]
        public string? AddressLine2 { get; set; } = string.Empty;

        [Column("cityId")]
        public int CityId { get; set; }

        [Column("postalCode")]
        public string? PostalCode { get; set; } = "00000";

        [Column("phone")]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string? Phone { get; set; }

        [Column("createDate")]
        public DateTime CreateDate { get; set; }

        [Column("createdBy")]
        public string? CreatedBy { get; set; }

        [Column("lastUpdate")]
        public DateTime LastUpdate { get; set; }

        [Column("lastUpdateBy")]
        public string? LastUpdateBy { get; set; }

        // Navigation Property
        public City? City { get; set; }
        public ICollection<Customer>? Customers { get; set; }
    }
}
