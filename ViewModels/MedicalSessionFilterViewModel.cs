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
    /// ViewModel that packages the list of filtered sessions
    /// alongside the current filter criteria for view binding.
    /// </summary>
    public class MedicalSessionFilterViewModel
    {
        // The filtered list of sessions to display
        public IEnumerable<MedicalSession> Sessions { get; set; } = new List<MedicalSession>();

        // Filter: free-text search on name or description
        public string? SearchText { get; set; }

        // Filter: session category
        public SessionCategory? SelectedCategory { get; set; }

        // Filter: inclusive start date bound
        public DateTime? StartDateFrom { get; set; }

        // Filter: inclusive end date bound
        public DateTime? EndDateTo { get; set; }

        // Total results count for display in the toolbar
        public int TotalCount => Sessions.Count();
    }
}