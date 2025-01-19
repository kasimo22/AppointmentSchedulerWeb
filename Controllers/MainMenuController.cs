using System.Diagnostics;
using AppointmentSchedulerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Data;
using AppointmentSchedulerWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSchedulerWeb.Controllers
{
    public class MainMenuController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IStringLocalizer<MainMenuController> _localizer;

        public MainMenuController(DatabaseContext context, IStringLocalizer<MainMenuController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            // Retrieve username and userId from session
            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(username) || userId == null)
            {
                // Redirect to login if session is expired
                return RedirectToAction("Index", "Login");
            }

            // Pass username and userId to the view
            ViewBag.Username = username;
            ViewBag.UserId = userId;

            return View();
        }

        public IActionResult Customer()
        {
            return RedirectToAction("Index", "Customer");
        }

        public IActionResult Appointment()
        {
            return RedirectToAction("Index", "Appointment");
        }

        public IActionResult Report1()
        {
            string report = GenerateAppointmentTypesByMonthReport();
            return Content(report, "text/plain");
        }

        public IActionResult Report2()
        {
            string report = GenerateScheduleForEachUser();
            return Content(report, "text/plain");
        }

        public IActionResult Report3()
        {
            string report = GenerateTotalAppointmentsPerCustomerReport();
            return Content(report, "text/plain");
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Login");
        }

        private string GenerateAppointmentTypesByMonthReport()
        {
            try
            {
                var appointmentsByMonth = _context.Appointments
                    .GroupBy(a => new { Month = a.Start.Month, a.Type })
                    .Select(g => new
                    {
                        Month = g.Key.Month,
                        Type = g.Key.Type,
                        Count = g.Count()
                    })
                    .ToList();

                string report = "Appointment Types by Month:\n";
                foreach (var group in appointmentsByMonth)
                {
                    string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Month);
                    report += $"Month: {monthName}, Type: {group.Type}, Count: {group.Count}\n";
                }

                return report;
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }

        private string GenerateScheduleForEachUser()
        {
            try
            {
                var schedules = _context.Appointments
                    .Include(a => a.User)
                    .Select(a => new
                    {
                        UserName = a.User.UserName,
                        a.Type,
                        a.Start,
                        a.End
                    })
                    .OrderBy(a => a.UserName)
                    .ThenBy(a => a.Start)
                    .ToList();

                string report = "Schedule for Each User:\n";
                foreach (var schedule in schedules.GroupBy(s => s.UserName))
                {
                    report += $"User: {schedule.Key}\n";
                    foreach (var appointment in schedule)
                    {
                        report += $"  {appointment.Type}: {appointment.Start:MM/dd/yyyy hh:mm tt} - {appointment.End:MM/dd/yyyy hh:mm tt}\n";
                    }
                }

                return report;
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }

        private string GenerateTotalAppointmentsPerCustomerReport()
        {
            try
            {
                var appointmentsPerCustomer = _context.Appointments
                    .Include(a => a.Customer)
                    .GroupBy(a => a.Customer.CustomerName)
                    .Select(g => new
                    {
                        CustomerName = g.Key,
                        TotalAppointments = g.Count()
                    })
                    .OrderByDescending(a => a.TotalAppointments)
                    .ToList();

                string report = "Total Appointments Per Customer:\n";
                foreach (var customer in appointmentsPerCustomer)
                {
                    report += $"Customer: {customer.CustomerName}, Total Appointments: {customer.TotalAppointments}\n";
                }

                return report;
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }
    }
}

