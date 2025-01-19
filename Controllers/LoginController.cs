using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Resources;

namespace AppointmentSchedulerWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _connectionString;
        private readonly ResourceManager _resourceManager = new ResourceManager("AppointmentSchedulerWeb.Resources.LoginForm", typeof(LoginController).Assembly);

        public LoginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authenticate(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Username and password fields cannot be empty.";
                    return View("Index");
                }

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT \"userId\", \"password\" FROM \"user\" WHERE \"userName\" = @username";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["userId"]);
                                string storedHashedPassword = reader["password"].ToString();

                                string inputHashedPassword = HashPassword(password);

                                if (storedHashedPassword == inputHashedPassword)
                                {
                                    HttpContext.Session.SetInt32("UserId", userId);
                                    HttpContext.Session.SetString("Username", username);

                                    LogLogin(username);
                                    CheckUpcomingAppointments(userId);

                                    return RedirectToAction("Index", "MainMenu");
                                }
                            }
                        }
                    }

                    ViewBag.Error = "Invalid username or password. Please try again.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An unexpected error occurred: " + ex.Message;
                return View("Index");
            }
        }

        private void CheckUpcomingAppointments(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT a.type, a.start, c.customerName
                    FROM appointment a
                    JOIN customer c ON a.customerId = c.customerId
                    WHERE a.userId = @userId
                    AND a.start BETWEEN NOW() AND NOW() + INTERVAL '15 minutes'";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            string alertMessage = _resourceManager.GetString("UpcomingAppointments", CultureInfo.CurrentCulture) + "\n";

                            while (reader.Read())
                            {
                                string type = reader["type"].ToString();
                                DateTime utcStart = Convert.ToDateTime(reader["start"]);
                                DateTime localStart = utcStart.ToLocalTime();
                                string formattedTime = localStart.ToString("MM/dd/yyyy hh:mm tt");

                                string customerName = reader["customerName"].ToString();
                                alertMessage += $"- {type} with {customerName} at {formattedTime}\n";
                            }

                            TempData["UpcomingAlert"] = alertMessage;
                        }
                    }
                }
            }
        }

        private void LogLogin(string username)
        {
            string logPath = "wwwroot/Login_History.txt";
            string logEntry = $"{DateTime.Now}: {username} logged in.";
            System.IO.File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
    }
}
