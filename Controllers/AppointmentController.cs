﻿using System;
using AppointmentSchedulerWeb.Models;
using AppointmentSchedulerWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppointmentSchedulerWeb.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly DatabaseContext _context;

        public AppointmentController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Appointment
        public IActionResult Index(string? searchString = null)
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            var appointments = _context.Appointments
                .Include(a => a.Customer)
                .AsQueryable();

            // Search by Type or Customer Name
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                appointments = appointments.Where(a =>
                    EF.Functions.ILike(a.Type, $"%{searchString}%") ||
                    EF.Functions.ILike(a.Customer.CustomerName, $"%{searchString}%") ||
                    EF.Functions.ILike(a.User.UserName, $"%{searchString}%")
                );
            }

            // Convert to ViewModel with time conversion
            var appointmentViewModels = appointments
                .ToList()
                .Select(a => new AppointmentViewModel
                {
                    AppointmentId = a.AppointmentId,
                    CustomerName = a.Customer.CustomerName,
                    Type = a.Type,
                    Start = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(a.Start, DateTimeKind.Utc), estZone),
                    End = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(a.End, DateTimeKind.Utc), estZone)
                })
                .ToList();

            return View(appointmentViewModels);
        }

        // GET: Appointment/Create
        public IActionResult Create()
        {
            PopulateCustomersDropdown();
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            // Remove validation for auto-populated fields
            ModelState.Remove("CreatedBy");
            ModelState.Remove("LastUpdateBy");
            ModelState.Remove("Title");
            ModelState.Remove("Url");
            ModelState.Remove("Contact");
            ModelState.Remove("Location");
            ModelState.Remove("Description");

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the UserId and Username from the session
                    int? userId = HttpContext.Session.GetInt32("userid");
                    string loggedInUser = HttpContext.Session.GetString("username") ?? "System";

                    if (userId == null)
                    {
                        ModelState.AddModelError("", "User session expired. Please log in again.");
                        return RedirectToAction("Index", "Login");
                    }

                    // Ensure DateTime.Kind is set to UTC for conversion
                    TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    appointment.Start = DateTime.SpecifyKind(appointment.Start, DateTimeKind.Unspecified);
                    appointment.End = DateTime.SpecifyKind(appointment.End, DateTimeKind.Unspecified);

                    // Convert Eastern Time to UTC for storage
                    appointment.Start = TimeZoneInfo.ConvertTimeToUtc(appointment.Start, estZone);
                    appointment.End = TimeZoneInfo.ConvertTimeToUtc(appointment.End, estZone);

                    // Validate that End Date is after Start Date
                    if (appointment.End <= appointment.Start)
                    {
                        ModelState.AddModelError("", "End date and time must be after the start date and time.");
                        PopulateCustomersDropdown(appointment.CustomerId);
                        return View(appointment);
                    }

                    // Business Hours Validation (9 AM - 5 PM EST)
                    DateTime startEST = TimeZoneInfo.ConvertTimeFromUtc(appointment.Start, estZone);
                    DateTime endEST = TimeZoneInfo.ConvertTimeFromUtc(appointment.End, estZone);

                    if (startEST.Hour < 9 || endEST.Hour > 17 ||
                        startEST.DayOfWeek == DayOfWeek.Saturday || startEST.DayOfWeek == DayOfWeek.Sunday)
                    {
                        ModelState.AddModelError("", "Appointments must be scheduled between 9:00 AM - 5:00 PM EST on weekdays.");
                        PopulateCustomersDropdown(appointment.CustomerId);
                        return View(appointment);
                    }

                    // Check for overlapping appointments
                    bool isOverlapping = _context.Appointments.Any(a =>
                        a.UserId == userId &&
                        ((appointment.Start < a.End) && (appointment.End > a.Start))
                    );

                    if (isOverlapping)
                    {
                        ModelState.AddModelError("", "This appointment overlaps with another existing appointment.");
                        PopulateCustomersDropdown(appointment.CustomerId);
                        return View(appointment);
                    }

                    // Set defaults for optional fields
                    appointment.Title = string.IsNullOrEmpty(appointment.Title) ? "not needed" : appointment.Title;
                    appointment.Description = string.IsNullOrEmpty(appointment.Description) ? "not needed" : appointment.Description;
                    appointment.Location = string.IsNullOrEmpty(appointment.Location) ? "not needed" : appointment.Location;
                    appointment.Contact = string.IsNullOrEmpty(appointment.Contact) ? "not needed" : appointment.Contact;
                    appointment.Url = string.IsNullOrEmpty(appointment.Url) ? "not needed" : appointment.Url;

                    // Add the appointment
                    appointment.UserId = userId.Value;
                    appointment.CreatedBy = loggedInUser;
                    appointment.CreateDate = DateTime.UtcNow;
                    appointment.LastUpdateBy = loggedInUser;
                    appointment.LastUpdate = DateTime.UtcNow;

                    _context.Appointments.Add(appointment);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Appointment added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating appointment: " + ex.Message);
                }
            }

            PopulateCustomersDropdown(appointment.CustomerId);
            return View(appointment);
        }

        // GET: Appointment/Edit/5
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Ensure the DateTime.Kind is set to Utc for the values retrieved from the database
            appointment.Start = DateTime.SpecifyKind(appointment.Start, DateTimeKind.Utc);
            appointment.End = DateTime.SpecifyKind(appointment.End, DateTimeKind.Utc);

            // Convert UTC to Eastern Time for display
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var startInEst = TimeZoneInfo.ConvertTimeFromUtc(appointment.Start, estZone);
            var endInEst = TimeZoneInfo.ConvertTimeFromUtc(appointment.End, estZone);

            // Map the Appointment entity to the AppointmentViewModel
            var viewModel = new AppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                CustomerName = appointment.Customer.CustomerName,
                CustomerId = appointment.CustomerId,
                Type = appointment.Type,
                Start = startInEst,
                End = endInEst,
                UserId = appointment.UserId
            };

            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "CustomerName", viewModel.CustomerId);
            return View(viewModel);
        }

        // POST: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AppointmentViewModel viewModel)
        {
            // Remove validation for auto-populated fields
            ModelState.Remove("CreatedBy");
            ModelState.Remove("LastUpdateBy");
            ModelState.Remove("Title");
            ModelState.Remove("Url");
            ModelState.Remove("Contact");
            ModelState.Remove("Location");
            ModelState.Remove("Description");
            ModelState.Remove(nameof(AppointmentViewModel.CustomerName));

            if (id != viewModel.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the logged-in username from the session
                    var loggedInUser = HttpContext.Session.GetString("username") ?? "System";

                    // Fetch the existing appointment
                    var existingAppointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
                    if (existingAppointment == null)
                    {
                        return NotFound();
                    }

                    // Ensure DateTime.Kind is set to Unspecified before conversion
                    TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    viewModel.Start = DateTime.SpecifyKind(viewModel.Start, DateTimeKind.Unspecified);
                    viewModel.End = DateTime.SpecifyKind(viewModel.End, DateTimeKind.Unspecified);

                    // Convert Eastern Time to UTC for storage
                    viewModel.Start = TimeZoneInfo.ConvertTimeToUtc(viewModel.Start, estZone);
                    viewModel.End = TimeZoneInfo.ConvertTimeToUtc(viewModel.End, estZone);

                    // Validate that End Date is after Start Date
                    if (viewModel.End <= viewModel.Start)
                    {
                        ModelState.AddModelError("", "End date and time must be after the start date and time.");
                        PopulateCustomersDropdown(viewModel.CustomerId);
                        return View(viewModel);
                    }

                    // Business Hours Validation (9 AM - 5 PM EST, Monday-Friday)
                    DateTime startEST = TimeZoneInfo.ConvertTimeFromUtc(viewModel.Start, estZone);
                    DateTime endEST = TimeZoneInfo.ConvertTimeFromUtc(viewModel.End, estZone);

                    // Validate business hours
                    if (startEST.Hour < 9 || endEST.Hour > 17 ||
                        startEST.DayOfWeek == DayOfWeek.Saturday || startEST.DayOfWeek == DayOfWeek.Sunday)
                    {
                        ModelState.AddModelError("", "Appointments must be scheduled between 9:00 AM - 5:00 PM EST, Monday-Friday.");
                        PopulateCustomersDropdown(viewModel.CustomerId);
                        return View(viewModel);
                    }

                    // Check for overlapping appointments
                    bool isOverlapping = _context.Appointments.Any(a =>
                        a.AppointmentId != id &&
                        a.UserId == viewModel.UserId &&
                        ((viewModel.Start < a.End) && (viewModel.End > a.Start))
                    );

                    if (isOverlapping)
                    {
                        ModelState.AddModelError("", "This appointment overlaps with another existing appointment.");
                        PopulateCustomersDropdown(viewModel.CustomerId);
                        return View(viewModel);
                    }

                    // Update appointment details
                    existingAppointment.Type = viewModel.Type;
                    existingAppointment.CustomerId = viewModel.CustomerId;
                    existingAppointment.Start = viewModel.Start;
                    existingAppointment.End = viewModel.End;
                    existingAppointment.LastUpdate = DateTime.UtcNow;
                    existingAppointment.LastUpdateBy = loggedInUser;

                    _context.Update(existingAppointment);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Appointment updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating appointment: {ex.Message}");
                }
            }

            PopulateCustomersDropdown(viewModel.CustomerId);
            return View(viewModel);
        }


        // GET: Appointment/Delete/5
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Appointment deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateCustomersDropdown(int? selectedCustomerId = null)
        {
            var customers = _context.Customers.ToList();
            ViewBag.Customers = new SelectList(customers, "CustomerId", "CustomerName", selectedCustomerId);
        }
    }
}
