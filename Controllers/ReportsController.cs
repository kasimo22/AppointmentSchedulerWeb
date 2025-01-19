using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentSchedulerWeb.Data;
using System.Globalization;

namespace AppointmentSchedulerWeb.Controllers
{
    public class ReportsController : Controller
    {
        private readonly DatabaseContext _context;

        public ReportsController(DatabaseContext context)
        {
            _context = context;
        }

        // Appointment Types by Month
        public IActionResult AppointmentTypesByMonth()
        {
            var rawData = _context.Appointments
                .GroupBy(a => new { a.Type, Month = a.Start.Month })
                .Select(g => new
                {
                    MonthNumber = g.Key.Month,
                    Type = g.Key.Type,
                    Appointments = g.Select(a => new
                    {
                        a.Start,
                        a.End
                    }).ToList(),
                    Count = g.Count()
                })
                .ToList();

            var report = rawData
                .Select(r => new
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(r.MonthNumber),
                    r.Type,
                    r.Count,
                    Appointments = r.Appointments.Select(a => new
                    {
                        Start = a.Start.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
                        End = a.End.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt")
                    }).ToList()
                })
                .OrderBy(r => r.Month)
                .ToList();

            return View(report);
        }

        // Schedule for Each User
        public IActionResult UserSchedules()
        {
            var schedules = _context.Appointments
                .Include(a => a.User)
                .GroupBy(a => a.User.UserName)
                .Select(g => new
                {
                    UserName = g.Key,
                    Appointments = g.Select(a => new
                    {
                        a.Type,
                        Start = a.Start.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
                        End = a.End.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt")
                    }).ToList()
                })
                .OrderBy(s => s.UserName)
                .ToList();

            return View(schedules);
        }

        // Total Appointments Per Customer
        public IActionResult AppointmentsPerCustomer()
        {
            var customerAppointments = _context.Appointments
                .Include(a => a.Customer)
                .GroupBy(a => a.Customer.CustomerName)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    TotalAppointments = g.Count(),
                    Appointments = g.Select(a => new
                    {
                        a.Type,
                        Start = a.Start.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt"),
                        End = a.End.ToLocalTime().ToString("MM/dd/yyyy hh:mm tt")
                    }).ToList()
                })
                .OrderBy(c => c.CustomerName)
                .ToList();

            return View(customerAppointments);
        }
    }
}
