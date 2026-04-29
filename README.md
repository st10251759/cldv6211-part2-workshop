# MediBook - Clinical Facility Reservation System
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server%20LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Azure Blob](https://img.shields.io/badge/Azure%20Blob%20Storage-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Azurite](https://img.shields.io/badge/Azurite%20Emulator-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap%205-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![Status](https://img.shields.io/badge/Part%202-Complete-28a745?style=for-the-badge)

---

## Table of Contents

- [About the Project](#about-the-project)
- [Scenario](#scenario)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Database Design](#database-design)
- [Part 2 Features](#part-2-features)
  - [Feature 1: Local Blob Storage with Azurite](#feature-1-local-blob-storage-with-azurite)
  - [Feature 2: Double-Booking Validation](#feature-2-double-booking-validation)
  - [Feature 3: Block Deletion of Active Records](#feature-3-block-deletion-of-active-records)
  - [Feature 4: Enhanced Display and Search](#feature-4-enhanced-display-and-search)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Project](#running-the-project)
- [Image Uploads and Azurite](#image-uploads-and-azurite)
- [Configuration Reference](#configuration-reference)
- [Known Limitations](#known-limitations)
- [Part 2 Checklist](#part-2-checklist)

---

## About the Project

MediBook is an ASP.NET Core MVC web application built for CLDV6211 Cloud Development A.
It is a clinical facility reservation system that allows administrators to manage medical
facilities, schedule medical sessions, and link them through reservations.

The project is developed in three parts across the module:

| Part | Focus | Status |
|------|-------|--------|
| Part 1 | Local MVC App with SQL LocalDB and full CRUD | Complete |
| Part 2 | Azurite blob storage, validation, search, deletion guard | Complete |
| Part 3 | Full Azure cloud deployment | Upcoming |

This README covers **Part 2** in detail.

---

## Scenario

MediBook represents a hospital resource booking platform. The system is designed
for clinical coordinators who need to:

- Register and manage clinical **facilities** (operating theatres, consultation rooms, wards)
- Create and manage **medical sessions** (scheduled procedures or clinical events)
- Make **reservations** that link a facility to a session for a specific time window

The core business rule is that a single facility cannot be used by more than one
session at the same time. MediBook enforces this through server-side double-booking
validation on every reservation.

This scenario is the MediBook equivalent of the EventEase Venue Booking System
described in the CLDV6211 POE brief.

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core 8 MVC |
| Language | C# 12 |
| ORM | Entity Framework Core 8 |
| Database | SQL Server LocalDB |
| Blob Storage | Azurite (local Azure Blob Storage emulator) |
| Blob SDK | Azure.Storage.Blobs NuGet package |
| Frontend | Bootstrap 5, HTML5, CSS3, JavaScript |
| Icons | Font Awesome 6 |
| Fonts | Google Fonts (Inter, Playfair Display) |
| IDE | Visual Studio 2022 |

---

## Project Structure

```
MediBook/
|
|-- Controllers/
|   |-- HomeController.cs
|   |-- FacilitiesController.cs
|   |-- MedicalSessionsController.cs
|   |-- ReservationsController.cs
|
|-- Models/
|   |-- Facility.cs
|   |-- MedicalSession.cs
|   |-- Reservation.cs
|
|-- Services/
|   |-- BlobService.cs
|
|-- Data/
|   |-- MediBookDbContext.cs
|
|-- Views/
|   |-- Shared/
|   |   |-- _Layout.cshtml
|   |-- Home/
|   |   |-- Index.cshtml
|   |-- Facilities/
|   |   |-- Index.cshtml
|   |   |-- Create.cshtml
|   |   |-- Edit.cshtml
|   |   |-- Details.cshtml
|   |   |-- Delete.cshtml
|   |-- MedicalSessions/
|   |   |-- Index.cshtml
|   |   |-- Create.cshtml
|   |   |-- Edit.cshtml
|   |   |-- Details.cshtml
|   |   |-- Delete.cshtml
|   |-- Reservations/
|       |-- Index.cshtml
|       |-- Create.cshtml
|       |-- Edit.cshtml
|       |-- Details.cshtml
|       |-- Delete.cshtml
|
|-- wwwroot/
|   |-- css/
|   |   |-- site.css
|   |-- js/
|   |   |-- site.js
|   |-- images/
|       |-- logo.png
|       |-- hero-1.jpg
|       |-- hero-2.jpg
|       |-- hero-3.jpg
|       |-- about.jpg
|       |-- placeholder-facility.jpg
|       |-- placeholder-session.jpg
|       |-- reservation-bg.jpg
|
|-- appsettings.json
|-- Program.cs
```

---

## Database Design

MediBook uses three tables with the following relationships:

```
Facility (1) ----< Reservation (many)
MedicalSession (1) ----< Reservation (many)
```

### Facility

| Column | Type | Notes |
|--------|------|-------|
| FacilityId | int | Primary Key, auto-increment |
| Name | string | Required, max 100 chars |
| Location | string | Required |
| Capacity | int | Required |
| Description | string | Optional |
| ImageUrl | string | Optional, blob URL stored here |

### MedicalSession

| Column | Type | Notes |
|--------|------|-------|
| SessionId | int | Primary Key, auto-increment |
| Name | string | Required, max 100 chars |
| Description | string | Optional |
| StartDate | DateTime | Required |
| EndDate | DateTime | Required |
| ImageUrl | string | Optional, blob URL stored here |

### Reservation

| Column | Type | Notes |
|--------|------|-------|
| ReservationId | int | Primary Key, auto-increment |
| FacilityId | int | Foreign Key to Facility |
| SessionId | int | Foreign Key to MedicalSession |
| StartDate | DateTime | Required |
| EndDate | DateTime | Required |

The `Reservation` table is the join table. It links a Facility to a MedicalSession
and stores the time window for that booking.

---

## Part 2 Features

---

### Feature 1: Local Blob Storage with Azurite

![Feature](https://img.shields.io/badge/Feature%201-Blob%20Storage-0078D4?style=flat-square&logo=microsoftazure)

**What this does:**
Facilities and Medical Sessions can now have images uploaded directly through
the Create and Edit forms. Images are stored in Azurite, a local Azure Blob
Storage emulator. The blob URL is saved to the database and used to display
the image in views.

**Files changed or created:**

| File | Change |
|------|--------|
| `Services/BlobService.cs` | New service class handling upload and delete |
| `Program.cs` | Registered BlobService as a singleton |
| `appsettings.json` | Added connection string and container name |
| `Models/Facility.cs` | Added `ImageUrl` and `[NotMapped] ImageFile` |
| `Models/MedicalSession.cs` | Added `ImageUrl` and `[NotMapped] ImageFile` |
| `Controllers/FacilitiesController.cs` | Injected BlobService, updated Create/Edit POST |
| `Controllers/MedicalSessionsController.cs` | Same as above |
| `Views/Facilities/Create.cshtml` | Drag-and-drop upload zone with preview |
| `Views/Facilities/Edit.cshtml` | Current image display plus replace zone |
| `Views/MedicalSessions/Create.cshtml` | Same as Facilities Create |
| `Views/MedicalSessions/Edit.cshtml` | Same as Facilities Edit |

**How it works:**

1. User selects or drags an image onto the upload zone in the form
2. JavaScript shows a live preview before the form is submitted
3. On POST, the controller calls `BlobService.UploadImageAsync(file)`
4. BlobService uploads the file to the Azurite container and returns a URL
5. The URL is saved to the `ImageUrl` column in the database
6. If no image is uploaded, a placeholder image path is used instead
7. On Edit, the old blob is deleted before the new one is uploaded

**Key implementation note:**

The model has two separate image properties:

```csharp
// Saved to the database
public string? ImageUrl { get; set; }

// NOT saved to the database - receives the uploaded file only
[NotMapped]
public IFormFile? ImageFile { get; set; }
```

The form must include `enctype="multipart/form-data"` for file uploads to reach
the server. Without this attribute, the file will never be received by the controller.

---

### Feature 2: Double-Booking Validation

![Feature](https://img.shields.io/badge/Feature%202-Double%20Booking%20Guard-dc3545?style=flat-square)

**What this does:**
Prevents a facility from being reserved by two overlapping sessions. If a new
reservation's time window conflicts with an existing one for the same facility,
the save is blocked and an error message is shown to the user.

**Files changed:**

| File | Change |
|------|--------|
| `Controllers/ReservationsController.cs` | Added `HasDoubleBooking` private method |

**Overlap detection logic:**

Two time windows A and B overlap when:

```
A.StartDate < B.EndDate  AND  A.EndDate > B.StartDate
```

This single condition covers all overlap scenarios: partial overlap at start,
partial overlap at end, one window fully inside the other, and identical windows.

**Implementation:**

```csharp
private bool HasDoubleBooking(int facilityId, DateTime start,
    DateTime end, int? excludeReservationId = null)
{
    return _context.Reservations.Any(r =>
        r.FacilityId == facilityId &&
        r.ReservationId != excludeReservationId &&
        r.StartDate < end &&
        r.EndDate > start);
}
```

The `excludeReservationId` parameter is used in the Edit action to prevent a
reservation from being flagged as conflicting with itself when dates are unchanged.

**Client-side validation layer:**

In addition to the server-side check, both the Create and Edit views include
JavaScript that validates required fields and date order before the form is
submitted. Errors are shown in a toast notification in the top-right corner
so users get immediate feedback without a page reload.

---

### Feature 3: Block Deletion of Active Records

![Feature](https://img.shields.io/badge/Feature%203-Deletion%20Guard-fd7e14?style=flat-square)

**What this does:**
Prevents a Facility or Medical Session from being deleted if it still has
active reservations linked to it. Deleting a parent record that has child
reservations would leave those reservations pointing to a record that no
longer exists, which breaks data integrity.

**Files changed:**

| File | Change |
|------|--------|
| `Controllers/FacilitiesController.cs` | GET and POST Delete updated |
| `Controllers/MedicalSessionsController.cs` | GET and POST Delete updated |
| `Views/Facilities/Delete.cshtml` | Split into blocked and safe states |
| `Views/MedicalSessions/Delete.cshtml` | Split into blocked and safe states |

**How the Delete view works:**

The Delete view checks whether the record has linked reservations and renders
one of two layouts:

```
Has reservations?
|
|-- YES: Blocked state
|        Navy header, warning banner, table of linked reservations,
|        direct "Remove" links for each reservation, NO delete button
|
|-- NO:  Safe state
         Red header, warning message, delete confirmation button shown
```

**Controller safety net:**

The POST Delete action also checks for linked reservations server-side.
If reservations exist, it adds a ModelState error and redirects back to
the blocked Delete view. This prevents a bypass via a direct HTTP POST request.

**Why this matters:**

Without this guard, a user could delete a Facility that has three reservations
linked to it. Those reservations would still exist in the database with a
`FacilityId` that no longer points to anything. This is called an orphaned
record and causes errors or incorrect data throughout the application.

---

### Feature 4: Enhanced Display and Search

![Feature](https://img.shields.io/badge/Feature%204-Search%20%26%20Filter-198754?style=flat-square)

**What this does:**
The Reservations Index page now supports search and filtering. Users can
search by Reservation ID (exact numeric match) or by Medical Session name
(partial text match). Results are filtered at the database level using
Entity Framework, not in memory.

**Files changed:**

| File | Change |
|------|--------|
| `Controllers/ReservationsController.cs` | Index action accepts `string? searchQuery` |
| `Views/Reservations/Index.cshtml` | Search bar, result count, clear button added |

**Search logic:**

```csharp
public async Task<IActionResult> Index(string? searchQuery)
{
    var query = _context.Reservations
        .Include(r => r.Facility)
        .Include(r => r.MedicalSession)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchQuery))
    {
        bool isNumeric = int.TryParse(searchQuery, out int id);
        query = isNumeric
            ? query.Where(r => r.ReservationId == id)
            : query.Where(r => r.MedicalSession.Name
                .Contains(searchQuery));
    }

    ViewBag.SearchQuery = searchQuery;
    return View(await query.ToListAsync());
}
```

The search term is preserved in the search bar after submission so users
can see what they searched for. A result count is displayed below the
search bar. A clear button resets the list back to all records.

---

## Prerequisites

Before running this project, make sure the following are installed on your machine:

| Tool | Version | Download |
|------|---------|----------|
| Visual Studio 2022 | 17.x or later | [visualstudio.microsoft.com](https://visualstudio.microsoft.com) |
| .NET SDK | 8.0 or later | [dotnet.microsoft.com](https://dotnet.microsoft.com) |
| SQL Server LocalDB | Included with VS 2022 | Included in VS installer |
| Node.js | 18.x or later (for Azurite) | [nodejs.org](https://nodejs.org) |
| Azurite | Latest | Install via npm (see below) |

---

## Getting Started

### Step 1: Clone or Download the Repository

```bash
git clone https://github.com/yourusername/MediBook.git
cd MediBook
```

Or download the ZIP from your repository and extract it.

### Step 2: Install the Azure Blob Storage NuGet Package

This should already be in the project file. If it is missing, run this in
the Package Manager Console (Tools > NuGet Package Manager > Package Manager Console):

```
Install-Package Azure.Storage.Blobs
```

### Step 3: Install Azurite Globally

Open a terminal (View > Terminal in Visual Studio) and run:

```bash
npm install -g azurite
```

### Step 4: Create the Azurite Storage Folder

Create a folder on your machine where Azurite will store its data:

```bash
mkdir C:\azurite
```

You only need to do this once.

### Step 5: Restore NuGet Packages

In Visual Studio, right-click the solution in Solution Explorer and select
**Restore NuGet Packages**. Or run:

```bash
dotnet restore
```

---

## Running the Project

**Important:** Azurite must be running BEFORE you start the MVC application.
Image uploads will not work without Azurite running in the background.

### Step 1: Start Azurite

Open a terminal and run:

```bash
azurite --location C:\azurite --debug C:\azurite\debug.log
```

Leave this terminal open. Do not close it while developing.

You should see output similar to:

```
Azurite Blob service is starting at http://127.0.0.1:10000
Azurite Blob service is successfully listening at http://127.0.0.1:10000
```

### Step 2: Apply Database Migrations

Open the Package Manager Console and run:

```
Update-Database
```

This creates the LocalDB database and all tables based on the existing migrations.
If you need to create a new migration after model changes:

```
Add-Migration YourMigrationName
Update-Database
```

### Step 3: Run the Application

Press **F5** in Visual Studio or click the green run button. The application
will open in your browser at `https://localhost:[port]`.

---

## Image Uploads and Azurite

### How Images Are Stored

When a user uploads an image through the Facility or Medical Session forms,
the following happens:

```
User selects file
       |
       v
Form submitted (multipart/form-data)
       |
       v
Controller receives IFormFile
       |
       v
BlobService.UploadImageAsync() called
       |
       v
File uploaded to Azurite container "facility-images"
       |
       v
Blob URL returned (e.g. http://127.0.0.1:10000/devstoreaccount1/facility-images/filename.jpg)
       |
       v
URL saved to ImageUrl column in database
       |
       v
Image displayed in views using the stored URL
```

### Viewing Uploaded Blobs

You can inspect uploaded blobs using **Azure Storage Explorer**:

1. Download Azure Storage Explorer from [azure.microsoft.com/products/storage/storage-explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/)
2. Connect to **Local Storage Emulator** (Azurite)
3. Navigate to Blob Containers > facility-images
4. All uploaded images will be listed there

### Container Name

The container name is configured in `appsettings.json`:

```json
"BlobContainerName": "facility-images"
```

BlobService creates this container automatically if it does not exist when
the first image is uploaded.

---

## Configuration Reference

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MediBookDb;Trusted_Connection=True;"
  },
  "AzureBlobStorage": "UseDevelopmentStorage=true",
  "BlobContainerName": "facility-images",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Key settings:**

| Key | Value | Purpose |
|-----|-------|---------|
| `DefaultConnection` | LocalDB connection string | SQL database for all app data |
| `AzureBlobStorage` | `UseDevelopmentStorage=true` | Connects to Azurite instead of real Azure |
| `BlobContainerName` | `facility-images` | The blob container where images are stored |

### Program.cs Registrations

The following services must be registered in `Program.cs`:

```csharp
// Entity Framework
builder.Services.AddDbContext<MediBookDbContext>(options =>
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// Blob Storage Service
builder.Services.AddSingleton<BlobService>();
```

---

## Known Limitations

| Limitation | Detail |
|------------|--------|
| Azurite must be running manually | There is no auto-start. Start Azurite before the app every time. |
| Images are local only | Blob URLs use `http://127.0.0.1:10000` which only works on your machine. Part 3 will move this to real Azure Blob Storage. |
| No authentication | The app has no login system in Part 2. All users have full access to all CRUD operations. |
| No pagination | The Index pages load all records. Large datasets may slow down the page. |
| Image file size | BlobService does not enforce a file size limit server-side. The drag-and-drop zone shows a 5 MB guideline but this is not enforced in code. |

---

## Part 2 Checklist

Use this checklist to verify all Part 2 requirements are met before submission:

### Blob Storage

- [ ] Azurite installs and starts without errors
- [ ] `Azure.Storage.Blobs` NuGet package is installed
- [ ] `appsettings.json` has blob connection string and container name
- [ ] `BlobService.cs` exists in the `Services/` folder
- [ ] `BlobService` is registered in `Program.cs`
- [ ] Facility Create form uploads an image successfully
- [ ] Facility Edit form shows current image and allows replacement
- [ ] Medical Session Create form uploads an image successfully
- [ ] Medical Session Edit form shows current image and allows replacement
- [ ] Uploaded images display correctly in Details and Index views
- [ ] If no image is uploaded, placeholder image is shown

### Double-Booking Validation

- [ ] Creating a reservation that overlaps with an existing one is blocked
- [ ] Error message is shown to the user explaining the conflict
- [ ] Editing a reservation to overlap with another is also blocked
- [ ] Editing a reservation without changing dates saves successfully (no self-conflict)
- [ ] Client-side toast notification appears for empty required fields

### Block Deletion of Active Records

- [ ] Attempting to delete a Facility with reservations shows the blocked view
- [ ] Blocked view lists all linked reservations with remove links
- [ ] No delete button appears in the blocked state
- [ ] Facility with no reservations shows the normal delete confirmation
- [ ] Same behaviour applies to Medical Sessions
- [ ] POST bypass attempt is caught by the controller and redirected

### Search and Filter

- [ ] Searching by Reservation ID returns the correct record
- [ ] Searching by session name (partial match) returns matching records
- [ ] Search term is preserved in the search bar after submission
- [ ] Result count is displayed
- [ ] Clear button resets the list to all records
- [ ] Empty search returns all records without errors

---

## Acknowledgements

- CLDV6211 Cloud Development A Module -- Emeris
- Microsoft ASP.NET Core Documentation
- Microsoft Azure Blob Storage SDK Documentation
- Bootstrap 5 Component Library
- Font Awesome 6 Icon Library
- Google Fonts (Inter, Playfair Display)

---

![Part 2](https://img.shields.io/badge/CLDV6211-Part%202%20Complete-0F2854?style=for-the-badge)
![Local Dev](https://img.shields.io/badge/Environment-Local%20Development-1C4D8D?style=for-the-badge)
![Next](https://img.shields.io/badge/Next-Part%203%20Azure%20Deployment-4988C4?style=for-the-badge)
