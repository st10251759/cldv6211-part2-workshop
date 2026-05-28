// Models/SessionCategory.cs
// Defines predefined category types for a MedicalSession.
// Using an enum ensures only valid, consistent values are stored in the database.
// Author: MediBook Dev | CLDV6211 Part 3

namespace MediBook.Models
{
    public enum SessionCategory
    {
        Surgery = 1,
        Consultation = 2,
        Diagnostics = 3,
        Therapy = 4,
        Rehabilitation = 5,
        Emergency = 6,
        Screening = 7,
        Procedure = 8
    }
}