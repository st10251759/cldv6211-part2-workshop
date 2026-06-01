using MediBook.Models;

/*
==============================Code Attribution==================================
ASP.NET Core MVC ViewModels for Filtered Index Pages
Author: Microsoft
Link: https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview
Date Accessed: 29 May 2026
==============================Code Attribution==================================
*/

namespace MediBook.ViewModels
{
    /// <summary>
    /// Single facility row plus its availability for the selected date range.
    /// </summary>
    public class FacilityAvailabilityItem
    {
        public Facility Facility { get; set; } = null!;
        public bool? IsAvailable { get; set; }
    }

    /// <summary>
    /// Carries facilities and the current filter values to the index view.
    /// </summary>
    public class FacilityFilterViewModel
    {
        // Filtered facilities with availability information
        public IEnumerable<FacilityAvailabilityItem> Facilities { get; set; } = new List<FacilityAvailabilityItem>();

        // Free-text search on name, location, or description
        public string? SearchText { get; set; }

        // Availability filter: "", "available", or "unavailable"
        public string? Availability { get; set; }

        // Selected booking window start
        public DateTime? StartDateTime { get; set; }

        // Selected booking window end
        public DateTime? EndDateTime { get; set; }

        // Count for toolbar display
        public int TotalCount => Facilities.Count();
    }
}