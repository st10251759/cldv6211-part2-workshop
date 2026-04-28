using System.ComponentModel.DataAnnotations;

/*
==============================Code Attribution==================================
ASP.NET MVC Pattern
Author: Microsoft
Link: https://dotnet.microsoft.com/en-us/apps/aspnet/mvc
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Models
{
    // Represents a physical clinical space that can be reserved to host medical sessions.
    // A facility has a name, location, description, capacity and an optional image.
    // It can be linked to multiple reservations, each covering a different time slot.
    public class Facility
    {
        // Primary key — uniquely identifies each facility record
        [Key]
        public int FacilityId { get; set; }

        // The name of the facility.
        // Required and limited to 100 characters.
        [Required(ErrorMessage = "Facility name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // The physical address or location of the facility.
        // Required and limited to 200 characters.
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        // A brief description of the facility and its features.
        // Optional, limited to 300 characters.
        [StringLength(300)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // The maximum number of people the facility can accommodate.
        // Required and must be at least 1.
        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        // Optional URL pointing to an image representing the facility.
        // Defaults to the local placeholder image in wwwroot/images.
        // Limited to 500 characters to accommodate long URLs.
        [Display(Name = "Image URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; } = "/images/placeholder-facility.jpg";

        // Navigation property — collection of all reservations linked to this facility.
        // Used to check for associated reservations before allowing deletion,
        // preventing orphaned reservation records in the database.
        // Initialised as an empty list to avoid null reference exceptions.
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}