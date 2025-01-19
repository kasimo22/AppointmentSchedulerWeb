using System.Diagnostics;
using AppointmentSchedulerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.IO;
using System.Resources;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace AppointmentSchedulerWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly string connectionString = "server=localhost;user=sqlUser;database=client_schedule;port=3306;password=Passw0rd!";
        private readonly ResourceManager resourceManager = new ResourceManager("AppointmentSchedulerWeb.Resources.LoginForm", typeof(LoginController).Assembly);

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string.
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Split the stored hash into salt and hash
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string storedPasswordHash = parts[1];

            // Hash the entered password with the stored salt
            string enteredPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return storedPasswordHash == enteredPasswordHash;
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

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to get the stored hashed password
                    string query = "SELECT userId, password FROM user WHERE userName = @username";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = Convert.ToInt32(reader["userId"]);
                            string storedHashedPassword = reader["password"].ToString();

                            // Hash the input password
                            string inputHashedPassword = HashPassword(password);

                            // Compare hashed passwords
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
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT a.type, a.start, c.customerName 
                                 FROM appointment a 
                                 JOIN customer c ON a.customerId = c.customerId 
                                 WHERE a.userId = @userId 
                                 AND a.start BETWEEN UTC_TIMESTAMP() AND DATE_ADD(UTC_TIMESTAMP(), INTERVAL 15 MINUTE)";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        string alertMessage = resourceManager.GetString("UpcomingAppointments", CultureInfo.CurrentCulture) + "\n";

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

        private void LogLogin(string username)
        {
            string logPath = "wwwroot/Login_History.txt";
            string logEntry = $"{DateTime.Now}: {username} logged in.";
            System.IO.File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
    }
}
