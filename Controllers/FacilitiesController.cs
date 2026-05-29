using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;
using MediBook.Services;
using MediBook.ViewModels;

/*
==============================Code Attribution==================================
ASP.NET MVC Controllers with Azure Blob Storage
Author: Microsoft
Link: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Controllers
{
    // Handles all CRUD operations for Facility records.
    // Images are stored in Azure Blob Storage via BlobService.
    // Index supports advanced filtering by text and availability within a selected date/time range.
    public class FacilitiesController : Controller
    {
        private readonly MediBookDbContext _context;
        private readonly BlobService _blobService;

        public FacilitiesController(MediBookDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Facilities
        // Supports filtering by search text and availability in a selected date/time range.
        public async Task<IActionResult> Index(
            string? searchText,
            string? availability,
            DateTime? startDateTime,
            DateTime? endDateTime)
        {
            var query = _context.Facilities
                .Include(f => f.Reservations)
                .AsQueryable();

            // Free-text search on facility fields
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var term = searchText.Trim().ToLower();

                query = query.Where(f =>
                    f.Name.ToLower().Contains(term) ||
                    f.Location.ToLower().Contains(term) ||
                    (f.Description != null && f.Description.ToLower().Contains(term)));
            }

            // Only apply availability filtering if both dates are provided and valid
            if (startDateTime.HasValue && endDateTime.HasValue)
            {
                if (startDateTime.Value < endDateTime.Value)
                {
                    // A reservation overlaps when:
                    // existing.Start < selected.End && existing.End > selected.Start
                    if (!string.IsNullOrWhiteSpace(availability))
                    {
                        var normalizedAvailability = availability.Trim().ToLower();

                        if (normalizedAvailability == "available")
                        {
                            query = query.Where(f =>
                                !f.Reservations.Any(r =>
                                    r.StartDate < endDateTime.Value &&
                                    r.EndDate > startDateTime.Value));
                        }
                        else if (normalizedAvailability == "unavailable")
                        {
                            query = query.Where(f =>
                                f.Reservations.Any(r =>
                                    r.StartDate < endDateTime.Value &&
                                    r.EndDate > startDateTime.Value));
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("endDateTime",
                        "End date/time must be after the start date/time.");
                }
            }

            var viewModel = new FacilityFilterViewModel
            {
                Facilities = await query
                    .OrderBy(f => f.Name)
                    .ToListAsync(),
                SearchText = searchText,
                Availability = availability,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            return View(viewModel);
        }

        // GET: Facilities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facility == null) return NotFound();

            return View(facility);
        }

        // GET: Facilities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Facilities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FacilityId,Name,Location,Description,Capacity,ImageFile")] Facility facility)
        {
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (facility.ImageFile != null && facility.ImageFile.Length > 0)
                    {
                        facility.ImageUrl = await _blobService.UploadImageAsync(facility.ImageFile);
                    }
                    else
                    {
                        facility.ImageUrl = "/images/placeholder-facility.jpg";
                    }

                    _context.Add(facility);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Facility '{facility.Name}' was added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                }
            }

            return View(facility);
        }

        // GET: Facilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null) return NotFound();

            return View(facility);
        }

        // POST: Facilities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("FacilityId,Name,Location,Description,Capacity,ImageUrl,ImageFile")] Facility facility)
        {
            if (id != facility.FacilityId) return NotFound();

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Facilities
                        .AsNoTracking()
                        .FirstOrDefaultAsync(f => f.FacilityId == id);

                    if (facility.ImageFile != null && facility.ImageFile.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(existing?.ImageUrl))
                        {
                            await _blobService.DeleteImageAsync(existing.ImageUrl);
                        }

                        facility.ImageUrl = await _blobService.UploadImageAsync(facility.ImageFile);
                    }
                    else
                    {
                        facility.ImageUrl = existing?.ImageUrl ?? "/images/placeholder-facility.jpg";
                    }

                    _context.Update(facility);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Facility '{facility.Name}' was updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacilityExists(facility.FacilityId)) return NotFound();
                    throw;
                }
            }

            return View(facility);
        }

        // GET: Facilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities
                .Include(f => f.Reservations)
                    .ThenInclude(r => r.MedicalSession)
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facility == null) return NotFound();

            return View(facility);
        }

        // POST: Facilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities
                .Include(f => f.Reservations)
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facility == null) return NotFound();

            if (facility.Reservations.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete '{facility.Name}' — it has {facility.Reservations.Count} active reservation(s). Please remove all linked reservations before deleting this facility.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            if (!string.IsNullOrWhiteSpace(facility.ImageUrl))
            {
                await _blobService.DeleteImageAsync(facility.ImageUrl);
            }

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Facility '{facility.Name}' was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(f => f.FacilityId == id);
        }
    }
}