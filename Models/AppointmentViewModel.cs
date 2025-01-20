namespace AppointmentSchedulerWeb.Models
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int UserId { get; set; }
    }
}
