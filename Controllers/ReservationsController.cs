using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;

/*
==============================Code Attribution==================================
ASP.NET MVC Controllers
Author: Microsoft
Link: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Controllers
{
    // Handles all CRUD operations for Reservation records.
    // A reservation links a Facility to a MedicalSession for a specific time slot.
    // Includes double-booking validation to prevent conflicting reservations.
    public class ReservationsController : Controller
    {
        private readonly MediBookDbContext _context;

        public ReservationsController(MediBookDbContext context)
        {
            _context = context;
        }

        // GET: Reservations — retrieves all reservations with optional search
        public async Task<IActionResult> Index(string? searchQuery)
        {
            ViewData["SearchQuery"] = searchQuery;

            var reservations = _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.MedicalSession)
                .AsQueryable();

            // SEARCH: filter by ReservationId or MedicalSession Name
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                bool isIdSearch = int.TryParse(searchQuery.Trim(), out int searchId);

                reservations = reservations.Where(r =>
                    (isIdSearch && r.ReservationId == searchId) ||
                    r.MedicalSession!.Name.Contains(searchQuery));
            }

            return View(await reservations.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.MedicalSession)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // GET: Reservations/Create — populates dropdowns for Facility and MedicalSession
        public IActionResult Create()
        {
            ViewData["FacilityId"] = new SelectList(_context.Facilities, "FacilityId", "Name");
            ViewData["SessionId"] = new SelectList(_context.MedicalSessions, "SessionId", "Name");
            return View();
        }

        // POST: Reservations/Create — validates dates, checks for double booking, saves record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ReservationId,FacilityId,SessionId,StartDate,EndDate")] Reservation reservation)
        {
            // Validate that StartDate is before EndDate
            if (reservation.StartDate >= reservation.EndDate)
                ModelState.AddModelError("EndDate",
                    "End date must be after the start date.");

            // VALIDATION: Prevent double booking — only check if dates are valid first
            if (reservation.StartDate < reservation.EndDate &&
                HasDoubleBooking(reservation.FacilityId,
                    reservation.StartDate, reservation.EndDate))
            {
                ModelState.AddModelError("",
                    "This facility is already reserved during the selected time window. " +
                    "Please choose a different time slot or select a different facility.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reservation created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["FacilityId"] = new SelectList(_context.Facilities,
                "FacilityId", "Name", reservation.FacilityId);
            ViewData["SessionId"] = new SelectList(_context.MedicalSessions,
                "SessionId", "Name", reservation.SessionId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null) return NotFound();

            ViewData["FacilityId"] = new SelectList(_context.Facilities,
                "FacilityId", "Name", reservation.FacilityId);
            ViewData["SessionId"] = new SelectList(_context.MedicalSessions,
                "SessionId", "Name", reservation.SessionId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5 — validates dates, checks double booking, updates record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ReservationId,FacilityId,SessionId,StartDate,EndDate")] Reservation reservation)
        {
            if (id != reservation.ReservationId) return NotFound();

            // Validate that StartDate is before EndDate
            if (reservation.StartDate >= reservation.EndDate)
                ModelState.AddModelError("EndDate",
                    "End date must be after the start date.");

            // VALIDATION: Prevent double booking — exclude this record's own ID
            // so editing without changing the time does not falsely trigger the check
            if (reservation.StartDate < reservation.EndDate &&
                HasDoubleBooking(reservation.FacilityId,
                    reservation.StartDate, reservation.EndDate,
                    reservation.ReservationId))
            {
                ModelState.AddModelError("",
                    "This facility is already reserved during the selected time window. " +
                    "Please choose a different time slot or select a different facility.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Reservation updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.ReservationId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["FacilityId"] = new SelectList(_context.Facilities,
                "FacilityId", "Name", reservation.FacilityId);
            ViewData["SessionId"] = new SelectList(_context.MedicalSessions,
                "SessionId", "Name", reservation.SessionId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5 — displays delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.MedicalSession)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // POST: Reservations/Delete/5 — removes the reservation from the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reservation deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── Private: Checks if a facility is already booked during the requested window ──
        // Uses overlap logic: two ranges overlap when StartA < EndB AND EndA > StartB.
        // excludeReservationId is passed during Edit so the record does not conflict with itself.
        private bool HasDoubleBooking(int facilityId, DateTime startDate, DateTime endDate,
            int? excludeReservationId = null)
        {
            return _context.Reservations.Any(r =>
                r.FacilityId == facilityId &&
                (excludeReservationId == null || r.ReservationId != excludeReservationId) &&
                r.StartDate < endDate &&
                r.EndDate > startDate);
        }

        // Helper — checks whether a reservation with the given ID exists
        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(r => r.ReservationId == id);
        }
    }
}