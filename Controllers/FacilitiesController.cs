using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;
using MediBook.Services;

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
    // Image uploads are handled via BlobService, which stores images in Azurite.
    public class FacilitiesController : Controller
    {
        private readonly MediBookDbContext _context;
        private readonly BlobService _blobService;

        public FacilitiesController(MediBookDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Facilities — retrieves and displays all facilities
        public async Task<IActionResult> Index()
        {
            return View(await _context.Facilities.ToListAsync());
        }

        // GET: Facilities/Details/5 — displays full details of a single facility
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facility == null) return NotFound();

            return View(facility);
        }

        // GET: Facilities/Create — returns the blank create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Facilities/Create — uploads image to Azurite, saves facility to database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FacilityId,Name,Location,Description,Capacity,ImageFile")] Facility facility)
        {
            // Remove ImageUrl from validation — it is set programmatically after upload
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (facility.ImageFile != null && facility.ImageFile.Length > 0)
                    {
                        // Upload image to Azurite and store the returned blob URL
                        facility.ImageUrl = await _blobService.UploadImageAsync(facility.ImageFile);
                    }
                    else
                    {
                        // Fall back to the default placeholder if no file was uploaded
                        facility.ImageUrl = "/images/placeholder-facility.jpg";
                    }

                    _context.Add(facility);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        $"Facility '{facility.Name}' was added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    // Validation errors from BlobService (file type, size, etc.)
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    // Azurite connection or upload errors
                    ModelState.AddModelError("",
                        $"Image upload failed: {ex.Message}");
                }
            }

            return View(facility);
        }

        // GET: Facilities/Edit/5 — returns the edit form pre-filled with existing data
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null) return NotFound();

            return View(facility);
        }

        // POST: Facilities/Edit/5 — uploads new image if provided, updates facility record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("FacilityId,Name,Location,Description,Capacity,ImageUrl,ImageFile")] Facility facility)
        {
            if (id != facility.FacilityId) return NotFound();

            // Remove ImageUrl from validation — it is managed by the controller
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (facility.ImageFile != null && facility.ImageFile.Length > 0)
                    {
                        // Delete the old blob from Azurite if it was a previously uploaded image
                        var existing = await _context.Facilities
                            .AsNoTracking()
                            .FirstOrDefaultAsync(f => f.FacilityId == id);

                        if (existing?.ImageUrl != null &&
                            existing.ImageUrl.StartsWith("http://127.0.0.1"))
                        {
                            await _blobService.DeleteImageAsync(existing.ImageUrl);
                        }

                        // Upload the new image and update the URL
                        facility.ImageUrl = await _blobService.UploadImageAsync(facility.ImageFile);
                    }
                    // If no new file uploaded, ImageUrl is posted back from the hidden field
                    // and keeps its existing value — no change needed

                    _context.Update(facility);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        $"Facility '{facility.Name}' was updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    // Validation errors from BlobService (file type, size, etc.)
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    // Azurite connection or upload errors
                    ModelState.AddModelError("",
                        $"Image upload failed: {ex.Message}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacilityExists(facility.FacilityId)) return NotFound();
                    else throw;
                }
            }

            return View(facility);
        }

        // GET: Facilities/Delete/5 — displays delete confirmation with reservation check
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

        // POST: Facilities/Delete/5 — blocks deletion if active reservations exist
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities
                .Include(f => f.Reservations)
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facility == null) return NotFound();

            // VALIDATION: Block deletion if active reservations are linked to this facility
            // This is a safety net — the view already prevents the button from showing
            if (facility.Reservations.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete '{facility.Name}' — it has " +
                    $"{facility.Reservations.Count} active reservation(s). " +
                    "Please remove all linked reservations before deleting this facility.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            // Delete the blob image from Azurite if it was uploaded there
            if (facility.ImageUrl != null &&
                facility.ImageUrl.StartsWith("http://127.0.0.1"))
            {
                await _blobService.DeleteImageAsync(facility.ImageUrl);
            }

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Facility '{facility.Name}' was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        // Helper — checks whether a facility with the given ID exists
        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(f => f.FacilityId == id);
        }
    }
}