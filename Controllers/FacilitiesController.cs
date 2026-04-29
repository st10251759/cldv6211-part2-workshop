using Microsoft.AspNetCore.Mvc;
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
    // Handles all CRUD operations for Facility records.
    public class FacilitiesController : Controller
    {
        private readonly MediBookDbContext _context;

        public FacilitiesController(MediBookDbContext context)
        {
            _context = context;
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

        // POST: Facilities/Create — saves the new facility to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacilityId,Name,Location,Description,Capacity,ImageUrl")] Facility facility)
        {
            if (ModelState.IsValid)
            {
                _context.Add(facility);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

        // POST: Facilities/Edit/5 — updates the facility record in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FacilityId,Name,Location,Description,Capacity,ImageUrl")] Facility facility)
        {
            if (id != facility.FacilityId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facility);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacilityExists(facility.FacilityId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(facility);
        }

        // GET: Facilities/Delete/5 — displays delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.FacilityId == id);

            if (facility == null) return NotFound();

            return View(facility);
        }

        // POST: Facilities/Delete/5 — removes the facility from the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper — checks whether a facility with the given ID exists
        private bool FacilityExists(int id)
        {
            return _context.Facilities.Any(f => f.FacilityId == id);
        }
    }
}