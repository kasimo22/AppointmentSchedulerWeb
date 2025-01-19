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
using Microsoft.EntityFrameworkCore;
using AppointmentSchedulerWeb.Data;

namespace AppointmentSchedulerWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly DatabaseContext _context;
        public LoginController(DatabaseContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
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

                // Fetch user from the database
                var user = _context.Users.FirstOrDefault(u => u.UserName == username);

                if (user == null)
                {
                    ViewBag.Error = "Invalid username or password. Please try again.";
                    return View("Index");
                }

                // Hash the entered password and compare with the stored hash
                string inputHashedPassword = HashPassword(password);
                if (user.Password != inputHashedPassword)
                {
                    ViewBag.Error = "Invalid username or password. Please try again.";
                    return View("Index");
                }

                // Set session variables for the authenticated user
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.UserName);

                // Optional: Check for upcoming appointments or log the login
                LogLogin(user.UserName);
                CheckUpcomingAppointments(user.UserId);

                return RedirectToAction("Index", "MainMenu");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An unexpected error occurred: " + ex.Message;
                return View("Index");
            }
        }

        private void CheckUpcomingAppointments(int userId)
        {
            var upcomingAppointments = _context.Appointments
                .Where(a => a.UserId == userId && a.Start >= DateTime.UtcNow && a.Start <= DateTime.UtcNow.AddMinutes(15))
                .Include(a => a.Customer)
                .Select(a => new
                {
                    a.Type,
                    a.Start,
                    CustomerName = a.Customer.CustomerName
                })
                .ToList();

            if (upcomingAppointments.Any())
            {
                string alertMessage = "Upcoming Appointments:\n";
                foreach (var appointment in upcomingAppointments)
                {
                    alertMessage += $"- {appointment.Type} with {appointment.CustomerName} at {appointment.Start.ToLocalTime():MM/dd/yyyy hh:mm tt}\n";
                }

                TempData["UpcomingAlert"] = alertMessage;
            }
        }

        private void LogLogin(string username)
        {
            string logPath = Path.Combine("wwwroot", "Login_History.txt");
            string logEntry = $"{DateTime.Now}: {username} logged in.";
            System.IO.File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
    }
}
