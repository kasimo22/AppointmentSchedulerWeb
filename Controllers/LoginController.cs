﻿using System;
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
using System.Configuration;
using Microsoft.Extensions.Configuration;

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
            System.Diagnostics.Debug.WriteLine($"Attempting to authenticate user: {username}");
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Username and password fields cannot be empty.";
                    System.Diagnostics.Debug.WriteLine("Error: Username or password is empty.");
                    return View("Index");
                }

                // Retrieve the connection string from appsettings.json
                

                string connectionString = configuration.GetConnectionString("DefaultConnection");
                System.Diagnostics.Debug.WriteLine($"Connection String: {connectionString}");

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection established.");

                    // Query to get the stored hashed password
                    string query = "SELECT \"userid\", \"password\" FROM \"user\" WHERE \"username\" = @username";
                    using var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);

                    using var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["userid"]);
                        string storedHashedPassword = reader["password"].ToString();

                        // Hash the input password
                        string inputHashedPassword = HashPassword(password);
                        System.Diagnostics.Debug.WriteLine($"Stored password hash: {storedHashedPassword}");
                        System.Diagnostics.Debug.WriteLine($"Input password hash: {inputHashedPassword}");

                        // Compare hashed passwords
                        if (storedHashedPassword == inputHashedPassword)
                        {
                            System.Diagnostics.Debug.WriteLine("Password match. Logging in user.");
                            HttpContext.Session.SetInt32("userid", userId);
                            HttpContext.Session.SetString("username", username);

                            LogLogin(username);
                            CheckUpcomingAppointments(userId);

                            System.Diagnostics.Debug.WriteLine("Redirecting to MainMenu.");
                            return RedirectToAction("Index", "MainMenu");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Password mismatch.");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No user found with the given username.");
                    }

                    ViewBag.Error = "Invalid username or password. Please try again.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception occurred: {ex.Message}");
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
