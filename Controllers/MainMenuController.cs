using System.Diagnostics;
using AppointmentSchedulerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MySql.Data.MySqlClient;
using System.Data;

namespace AppointmentSchedulerWeb.Controllers
{
    public class MainMenuController : Controller
    {
        private readonly string connectionString = "server=localhost;user=sqlUser;database=client_schedule;port=3306;password=Passw0rd!";
        private readonly IStringLocalizer<MainMenuController> _localizer;

        public MainMenuController(IStringLocalizer<MainMenuController> localizer)
        {
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

        // Action for the Customer page
        public IActionResult Customer()
        {
            return RedirectToAction("Index", "Customer");
        }

        // Action for the Appointment page
        public IActionResult Appointment()
        {
            return RedirectToAction("Index", "Appointment");
        }

        // Action to generate the Appointment Types by Month Report
        public IActionResult Report1()
        {
            string report = GenerateAppointmentTypesByMonthReport();
            return Content(report, "text/plain");
        }

        // Action to generate the Schedule for Each User Report
        public IActionResult Report2()
        {
            string report = GenerateScheduleForEachUser();
            return Content(report, "text/plain");
        }

        // Action to generate the Total Appointments Per Customer Report
        public IActionResult Report3()
        {
            string report = GenerateTotalAppointmentsPerCustomerReport();
            return Content(report, "text/plain");
        }

        // Action to logout
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Login");
        }

        // Report: Appointment Types by Month
        private string GenerateAppointmentTypesByMonthReport()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT type, MONTH(start) AS month FROM appointment";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    MySqlDataReader reader = command.ExecuteReader();

                    var appointmentsByMonth = new Dictionary<int, Dictionary<string, int>>();

                    while (reader.Read())
                    {
                        string type = reader["type"].ToString();
                        int month = Convert.ToInt32(reader["month"]);

                        if (!appointmentsByMonth.ContainsKey(month))
                        {
                            appointmentsByMonth[month] = new Dictionary<string, int>();
                        }

                        if (!appointmentsByMonth[month].ContainsKey(type))
                        {
                            appointmentsByMonth[month][type] = 1;
                        }
                        else
                        {
                            appointmentsByMonth[month][type]++;
                        }
                    }

                    // Prepare the report string
                    string report = "Appointment Types by Month:\n";
                    foreach (var kvp in appointmentsByMonth)
                    {
                        string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(kvp.Key);
                        report += $"Month: {monthName}\n";

                        foreach (var type in kvp.Value)
                        {
                            report += $"  {type.Key}: {type.Value}\n";
                        }
                    }

                    return report;
                }
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }

        // Report: Schedule for Each User
        private string GenerateScheduleForEachUser()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT u.userName, a.type, a.start, a.end 
                                     FROM appointment a 
                                     JOIN user u ON a.userId = u.userId 
                                     ORDER BY u.userName, a.start";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    var userSchedules = new Dictionary<string, List<(string Type, DateTime Start, DateTime End)>>();

                    while (reader.Read())
                    {
                        string userName = reader["userName"].ToString();
                        string type = reader["type"].ToString();
                        DateTime start = Convert.ToDateTime(reader["start"]).ToLocalTime();
                        DateTime end = Convert.ToDateTime(reader["end"]).ToLocalTime();

                        if (!userSchedules.ContainsKey(userName))
                        {
                            userSchedules[userName] = new List<(string, DateTime, DateTime)>();
                        }

                        userSchedules[userName].Add((type, start, end));
                    }

                    string report = "Schedule for Each User:\n";
                    foreach (var user in userSchedules)
                    {
                        report += $"User: {user.Key}\n";
                        foreach (var appointment in user.Value)
                        {
                            report += $"  {appointment.Type}: {appointment.Start:MM/dd/yyyy hh:mm tt} - {appointment.End:MM/dd/yyyy hh:mm tt}\n";
                        }
                    }

                    return report;
                }
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }

        // Report: Total Appointments Per Customer
        private string GenerateTotalAppointmentsPerCustomerReport()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT c.customerName, COUNT(a.appointmentId) AS totalAppointments
                                     FROM appointment a 
                                     JOIN customer c ON a.customerId = c.customerId 
                                     GROUP BY c.customerName 
                                     ORDER BY totalAppointments DESC";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    var report = "Total Appointments Per Customer:\n";
                    while (reader.Read())
                    {
                        string customerName = reader["customerName"].ToString();
                        int totalAppointments = Convert.ToInt32(reader["totalAppointments"]);
                        report += $"Customer: {customerName}, Total Appointments: {totalAppointments}\n";
                    }

                    return report;
                }
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }
    }
}
