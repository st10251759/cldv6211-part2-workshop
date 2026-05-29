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
    // Handles all CRUD operations for MedicalSession records.
    // Images are stored in Azure Blob Storage via BlobService.
    // Index supports advanced filtering by text, category, and date range.
    public class MedicalSessionsController : Controller
    {
        private readonly MediBookDbContext _context;
        private readonly BlobService _blobService;

        public MedicalSessionsController(MediBookDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: MedicalSessions
        // Supports optional query parameters: searchText, category, startDateFrom, endDateTo
        public async Task<IActionResult> Index(
            string? searchText,
            SessionCategory? category,
            DateTime? startDateFrom,
            DateTime? endDateTo)
        {
            // Start with the full unfiltered session list
            var query = _context.MedicalSessions.AsQueryable();

            // Apply free-text filter on name or description (case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var term = searchText.Trim().ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    (s.Description != null && s.Description.ToLower().Contains(term)));
            }

            // Apply session category filter
            if (category.HasValue)
            {
                query = query.Where(s => s.Category == category.Value);
            }

            // Apply start date lower bound (sessions starting on or after this date)
            if (startDateFrom.HasValue)
            {
                query = query.Where(s => s.StartDate >= startDateFrom.Value);
            }

            // Apply end date upper bound (sessions ending on or before this date)
            if (endDateTo.HasValue)
            {
                // Include the full day by pushing to end of day
                var endOfDay = endDateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.EndDate <= endOfDay);
            }

            // Build and return the ViewModel
            var viewModel = new MedicalSessionFilterViewModel
            {
                Sessions = await query.OrderBy(s => s.StartDate).ToListAsync(),
                SearchText = searchText,
                SelectedCategory = category,
                StartDateFrom = startDateFrom,
                EndDateTo = endDateTo
            };

            return View(viewModel);
        }

        // GET: MedicalSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // GET: MedicalSessions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MedicalSessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("SessionId,Name,Description,StartDate,EndDate,Category,ImageFile")] MedicalSession session)
        {
            if (session.StartDate >= session.EndDate)
                ModelState.AddModelError("EndDate", "End date must be after the start date.");

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (session.ImageFile != null && session.ImageFile.Length > 0)
                    {
                        session.ImageUrl = await _blobService.UploadImageAsync(session.ImageFile);
                    }
                    else
                    {
                        session.ImageUrl = "/images/placeholder-session.jpg";
                    }

                    _context.Add(session);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Medical session '{session.Name}' was added successfully.";
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

            return View(session);
        }

        // GET: MedicalSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions.FindAsync(id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: MedicalSessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("SessionId,Name,Description,StartDate,EndDate,Category,ImageUrl,ImageFile")] MedicalSession session)
        {
            if (id != session.SessionId) return NotFound();

            if (session.StartDate >= session.EndDate)
                ModelState.AddModelError("EndDate", "End date must be after the start date.");

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.MedicalSessions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.SessionId == id);

                    if (session.ImageFile != null && session.ImageFile.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(existing?.ImageUrl))
                        {
                            await _blobService.DeleteImageAsync(existing.ImageUrl);
                        }

                        session.ImageUrl = await _blobService.UploadImageAsync(session.ImageFile);
                    }
                    else
                    {
                        session.ImageUrl = existing?.ImageUrl ?? "/images/placeholder-session.jpg";
                    }

                    _context.Update(session);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Medical session '{session.Name}' was updated successfully.";
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
                    if (!SessionExists(session.SessionId)) return NotFound();
                    throw;
                }
            }

            return View(session);
        }

        // GET: MedicalSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions
                .Include(s => s.Reservations)
                    .ThenInclude(r => r.Facility)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: MedicalSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.MedicalSessions
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            if (session.Reservations.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete '{session.Name}' because it has {session.Reservations.Count} active reservation(s). " +
                    "Please remove all linked reservations before deleting this session.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            if (!string.IsNullOrWhiteSpace(session.ImageUrl))
            {
                await _blobService.DeleteImageAsync(session.ImageUrl);
            }

            _context.MedicalSessions.Remove(session);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Medical session '{session.Name}' was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return _context.MedicalSessions.Any(s => s.SessionId == id);
        }
    }
}