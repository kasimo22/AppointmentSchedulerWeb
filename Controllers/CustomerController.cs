using System.ComponentModel.DataAnnotations.Schema;
using AppointmentSchedulerWeb.Data;
using AppointmentSchedulerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSchedulerWeb.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DatabaseContext _context;

        public CustomerController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Customer
        public IActionResult Index(string searchString)
        {
            try
            {
                var customers = _context.Customers.Include(c => c.Address).AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    customers = customers.Where(c =>
                        c.CustomerName.Contains(searchString) ||
                        c.Address.AddressLine1.Contains(searchString));
                }

                System.Diagnostics.Debug.WriteLine($"Generated SQL: {customers.ToQueryString()}");
                return View(customers.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            // Remove errors for these fields
            ModelState.Remove("CreatedBy");
            ModelState.Remove("LastUpdateBy");

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the logged-in username from the session
                    string loggedInUser = HttpContext.Session.GetString("username") ?? "System";

                    if (string.IsNullOrEmpty(loggedInUser))
                    {
                        TempData["ErrorMessage"] = "User session expired. Please log in again.";
                        return RedirectToAction("Index", "Login");
                    }

                    // Ensure Address is not null
                    if (customer.Address == null)
                    {
                        ModelState.AddModelError("", "Address information is required.");
                        return View(customer);
                    }

                    // Set metadata and default PostalCode for Address
                    customer.Address.CreateDate = DateTime.UtcNow;
                    customer.Address.CreatedBy = loggedInUser;
                    customer.Address.LastUpdate = DateTime.UtcNow;
                    customer.Address.LastUpdateBy = loggedInUser;

                    

                    // Set a default PostalCode if none is provided
                    customer.Address.PostalCode = string.IsNullOrEmpty(customer.Address.PostalCode) ? "00000" : customer.Address.PostalCode;

                    // Set metadata for Customer
                    customer.Active = true;
                    customer.Address.CityId = 1; // Default CityId
                    customer.CreateDate = DateTime.UtcNow;
                    customer.CreatedBy = loggedInUser;
                    customer.LastUpdate = DateTime.UtcNow;
                    customer.LastUpdateBy = loggedInUser;

                    // Add the customer (EF Core will also add the Address)
                    _context.Customers.Add(customer);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Customer added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating customer: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                // Debugging ModelState Errors
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            TempData["ErrorMessage"] = "Failed to create customer. Please check your input.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Customer/Edit/5
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers
                .Include(c => c.Address)
                .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Customer customer)
        {
            if (id != customer.CustomerId)
                return NotFound();

            // Remove errors for these fields
            ModelState.Remove("CreatedBy");
            ModelState.Remove("LastUpdateBy");

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the logged-in username from the session
                    string loggedInUser = HttpContext.Session.GetString("username") ?? "System";

                    if (string.IsNullOrEmpty(loggedInUser))
                    {
                        TempData["ErrorMessage"] = "User session expired. Please log in again.";
                        return RedirectToAction("Index", "Login");
                    }

                    // Fetch the existing customer and related address
                    var existingCustomer = _context.Customers
                                                   .Include(c => c.Address)
                                                   .FirstOrDefault(c => c.CustomerId == id);

                    if (existingCustomer == null)
                    {
                        TempData["ErrorMessage"] = "Customer not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update customer fields
                    existingCustomer.CustomerName = customer.CustomerName;
                    existingCustomer.LastUpdate = DateTime.UtcNow;
                    existingCustomer.LastUpdateBy = loggedInUser;

                    // Update address fields
                    existingCustomer.Address.AddressLine1 = customer.Address.AddressLine1;
                    existingCustomer.Address.Phone = customer.Address.Phone;
                    existingCustomer.Address.LastUpdate = DateTime.UtcNow;
                    existingCustomer.Address.LastUpdateBy = loggedInUser;

                    // Save changes
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating customer: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            TempData["ErrorMessage"] = "Failed to update customer. Please check your input.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Delete/5
        public IActionResult Delete(int id)
        {
            // Check if the customer exists
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check for associated appointments
            var hasAppointments = _context.Appointments.Any(a => a.CustomerId == id);
            if (hasAppointments)
            {
                TempData["ErrorMessage"] = "Cannot delete this customer because they have associated appointments. Please delete the appointments first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Delete the customer
                _context.Customers.Remove(customer);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to check/create address
        private int GetOrCreateAddress(string address, string phone)
        {
            var existingAddress = _context.Addresses
                .FirstOrDefault(a => EF.Functions.ILike(a.AddressLine1, address) && a.Phone == phone);

            if (existingAddress != null)
                return existingAddress.AddressId;

            string loggedInUser = HttpContext.Session.GetString("Username") ?? "System";

            var newAddress = new Address
            {
                AddressLine1 = address,
                AddressLine2 = string.Empty,
                Phone = phone,
                CityId = 1, // Default to CityId = 1
                PostalCode = "00000",
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser,
                LastUpdate = DateTime.UtcNow,
                LastUpdateBy = loggedInUser
            };

            _context.Addresses.Add(newAddress);
            _context.SaveChanges();

            return newAddress.AddressId;
        }
    }
}