# MediBook - Clinical Facility Reservation System  
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server%20LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Azure Blob](https://img.shields.io/badge/Azure%20Blob%20Storage-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Azurite](https://img.shields.io/badge/Azurite%20Emulator-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap%205-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![Status](https://img.shields.io/badge/Part%202-Complete-28a745?style=for-the-badge)

***

## Table of Contents

- [About the Project](#about-the-project)
- [Scenario](#scenario)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Database Design](#database-design)
- [Part 2 Features](#part-2-features)
- [Part 3 Features](#part-3-features)
  - [Feature 5: Session Category Enum & Classification](#feature-5-session-category-enum--classification)
  - [Feature 6: Advanced Filtering with View Models](#feature-6-advanced-filtering-with-view-models)
  - [Feature 7: Full Azure Cloud Deployment](#feature-7-full-azure-cloud-deployment)
  - [Feature 8: Documentation, UX Polish & Reflection](#feature-8-documentation-ux-polish--reflection)
- [Prerequisites](#prerequisites)
- [Getting Started (Local)](#getting-started-local)
- [Running the Project Locally](#running-the-project-locally)
- [Azure Deployment Overview](#azure-deployment-overview)
- [Image Uploads and Blob Storage](#image-uploads-and-blob-storage)
- [Configuration Reference](#configuration-reference)
- [Known Limitations](#known-limitations)
- [Part 3 Checklist](#part-3-checklist)
- [Acknowledgements](#acknowledgements)

***

## About the Project

MediBook is an ASP.NET Core MVC web application built for **CLDV6211 Cloud Development A**.  
It is a clinical facility reservation system that allows administrators to manage medical facilities, schedule medical sessions, and link them through reservations.

The project is developed in three parts across the module:

| Part | Focus | Status |
|------|-------|--------|
| Part 1 | Local MVC app with SQL LocalDB and full CRUD | ✅ Complete |
| Part 2 | Azurite blob storage, validation, search, deletion guard | ✅ Complete |
| Part 3 | Advanced filtering, classification and full Azure cloud deployment | ✅ Complete |

This README now covers **Part 2 and Part 3** in detail.

***

## Scenario

MediBook represents a **hospital resource booking platform**. The system is designed for clinical coordinators who need to:

- Register and manage clinical **facilities** (operating theatres, consultation rooms, wards)
- Create and manage **medical sessions** (scheduled procedures or clinical events)
- Make **reservations** that link a facility to a session for a specific time window

Key business rules:

- A single facility cannot be used by more than one session at the same time (double-booking prevented)
- Facilities and sessions must be searchable and filterable for quick administration
- Session types are grouped by **Session Category** for easier classification and reporting

This is the MediBook equivalent of the **EventEase Venue Booking System** scenario in the CLDV6211 POE.

***

## Technology Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core 8 MVC |
| Language | C# 12 |
| ORM | Entity Framework Core 8 |
| Database (Local) | SQL Server LocalDB |
| Database (Cloud) | Azure SQL Database |
| Blob Storage (Cloud) | Azure Blob Storage |
| Frontend | Bootstrap 5, HTML5, CSS3, JavaScript |
| Icons | Font Awesome 6 |
| Fonts | Google Fonts (Inter, Playfair Display) |
| Hosting | Azure App Service |
| IDE | Visual Studio 2022 |

***

## Project Structure

_This structure is representative; some folders/files have been omitted for brevity._

```text
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
|   |-- Enums/
|       |-- SessionCategory.cs      // Part 3: session category enum
|
|-- ViewModels/
|   |-- FacilityFilterViewModel.cs   // Part 3: advanced filters
|   |-- MedicalSessionFilterViewModel.cs
|   |-- ReservationFilterViewModel.cs
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
|   |   |-- Index.cshtml           // includes advanced filters
|   |-- MedicalSessions/
|   |   |-- Index.cshtml           // includes advanced filters & categories
|   |-- Reservations/
|       |-- Index.cshtml           // includes advanced filters
|
|-- wwwroot/
|   |-- css/
|   |   |-- site.css
|   |-- js/
|   |   |-- site.js
|
|-- appsettings.json
|-- appsettings.Development.json
|-- appsettings.Production.json     // Azure connection strings
|-- Program.cs
```

***

## Database Design

MediBook uses three core tables with the following relationships:

```text
Facility (1) ----< Reservation (many)
MedicalSession (1) ----< Reservation (many)
```

### Facility

| Column | Type | Notes |
|--------|------|-------|
| FacilityId | int | Primary Key |
| Name | string | Required, max 100 chars |
| Location | string | Required |
| Capacity | int | Required |
| Description | string | Optional |
| ImageUrl | string | Optional, blob URL |

### MedicalSession

| Column | Type | Notes |
|--------|------|-------|
| SessionId | int | Primary Key |
| Name | string | Required, max 100 chars |
| Description | string | Optional |
| StartDate | DateTime | Required |
| EndDate | DateTime | Required |
| SessionCategory | enum | Required, **Part 3** classification |
| ImageUrl | string | Optional, blob URL |

### Reservation

| Column | Type | Notes |
|--------|------|-------|
| ReservationId | int | Primary Key |
| FacilityId | int | Foreign Key to Facility |
| SessionId | int | Foreign Key to MedicalSession |
| StartDate | DateTime | Required |
| EndDate | DateTime | Required |

***

## Part 2 Features

Your existing Part 2 section already documents:

- **Feature 1:** Local Blob Storage with Azurite  
- **Feature 2:** Double-booking validation  
- **Feature 3:** Deletion guard for active records  
- **Feature 4:** Reservations search and basic filter  

Keep that section as-is; it remains valid for **local development**.

***

## Part 3 Features



Part 3 extends MediBook from a local, Azurite-backed MVC application into a **cloud-hosted system** with richer search and filtering, session classification, and refined documentation.

### Feature 5: Session Category Enum & Classification



**What this does:**  
Introduces a strongly-typed **`SessionCategory` enum** for medical sessions, allowing each session to be classified under a clear category rather than free-text. This improves data consistency and supports advanced filters.

**Examples of categories:**

- Consultation
- Checkup
- Surgery
- Therapy
- Emergency

**Implementation highlights:**

- `SessionCategory` enum defined under `Models/Enums/SessionCategory.cs`
- `MedicalSession` includes a `SessionCategory` property
- Create/Edit views use a dropdown bound to the enum, ensuring only valid categories are selected
- Index view for Medical Sessions displays the category alongside session details

**Why this matters:**  
Enum-based classification makes it easier to:

- Filter sessions by category
- Avoid typos and inconsistent labels
- Support future reporting and analytics

***

### Feature 6: Advanced Filtering with View Models



**What this does:**  
Adds **advanced filtering and search capabilities** for **Facilities**, **Medical Sessions**, and **Reservations**, implemented using dedicated **View Models** to keep controllers and views clean.

Each listing page now includes a compact **filter bar** above the table, enabling users to combine multiple filters:

#### Medical Sessions

- Search by **session name** (partial text)
- Filter by **Session Category** (enum)
- Filter by **date range** (Start Date / End Date)

#### Facilities

- Search by **facility name** or location
- Optional filters (e.g., capacity ranges or other criteria, depending on your implementation)

#### Reservations

- Search by **Reservation ID**
- Search by **Medical Session name**
- Filter by **facility**
- Filter by **date range**

**Implementation highlights:**

- `MedicalSessionFilterViewModel`, `FacilityFilterViewModel`, `ReservationFilterViewModel` encapsulate:
  - Search text
  - Selected category / facility
  - Start and end dates
  - Result lists to display in the view
- Controllers now:
  - Accept the filter view model as a parameter
  - Build an `IQueryable` with conditional `Where` clauses
  - Apply filters server-side using Entity Framework
- Views:
  - Render filter controls bound to the view model
  - Preserve filter values after search
  - Show a clear “Reset filters” option

**User experience:**

- Filters are displayed in a **Bootstrap card** above the data table
- Badges or small labels indicate when filters are active
- The result count updates based on current filters

***

### Feature 7: Full Azure Cloud Deployment



**What this does:**  
Moves MediBook from a purely local environment into the **Azure cloud**, aligning with the Part 3 requirement for a fully hosted solution.

**Azure resources used:**

- **Azure App Service**  
  - Hosts the ASP.NET Core MVC application
  - Uses deployment from Visual Studio or Git-based deployment
- **Azure SQL Database**  
  - Stores Facilities, MedicalSessions, Reservations, and related data
  - Connection string stored in `appsettings.Production.json` / Azure configuration
- **Azure Blob Storage**  
  - Stores production images for facilities and sessions
  - Replaces Azurite in the deployed environment

**Configuration highlights:**

- Separate **local** and **production** configuration:
  - Local: `UseDevelopmentStorage=true` (Azurite)
  - Production: Azure Storage connection string
- `DefaultConnection` updated to point to Azure SQL in production
- Azure App Service application settings store connection strings and keys securely

**Deployment workflow:**

1. Apply EF Core migrations locally against Azure SQL
2. Publish from Visual Studio to Azure App Service
3. Verify:
   - Facility and session CRUD
   - Image uploads (using Azure Blob)
   - Advanced filters and search
   - Double-booking and deletion guards

***

### Feature 8: Documentation, UX Polish & Reflection



**What this does:**  
Ensures that the **documentation, visual design, and user experience** reflect a complete, professional cloud application, in line with Part 3 expectations.

Key improvements:

- Updated **README** with:
  - Part 3 feature descriptions
  - Azure deployment details
  - Advanced filter documentation
- Cleaned up **navigation and layout** using Bootstrap 5
- Clear **validation messages** and confirmation feedback for users
- Consistent **branding, typography, and icons** across all views

***

## Prerequisites

(As in your original README – keep this section, you can add “Azure subscription” as optional for deployment.)

***

## Getting Started (Local)

(Reuse your existing “Getting Started” and “Running the Project” sections for local development with Azurite.)

***

## Running the Project Locally

(Reuse your existing steps here, including Azurite, `Update-Database`, F5.)

***

## Azure Deployment Overview

A short subsection you can add under this heading (if you want to expand later):

- Configure **Azure SQL** connection string in Azure App Service
- Configure **Azure Blob Storage** connection string and container
- Publish from Visual Studio using “Publish to Azure” for the Web App
- Test advanced filters, uploads, and validation on the live URL

***

## Image Uploads and Blob Storage

(Keep your existing section, but you can add a short note:)

> In production, the same `BlobService` is configured to use **Azure Blob Storage**.  
> The local Azurite configuration is only used during development.

***

## Configuration Reference

(Existing section still applies; you can add a second table or code block for production settings if you want.)

***

## Known Limitations

(You can keep your list and optionally add:)

- Filtering is designed for typical administrator usage; extreme datasets may require pagination or API endpoints in a future enhancement.

***

## Part 3 Checklist

Use this checklist to verify all **Part 3** requirements are met:

### Session Category & Classification

- [ ] `SessionCategory` enum defined and used in `MedicalSession`
- [ ] Create/Edit forms render a category dropdown
- [ ] Index view shows category for each session
- [ ] Category-based filtering tested

### Advanced Filtering

- [ ] Facilities Index uses a filter view model
- [ ] Medical Sessions Index supports:
  - [ ] Search by name
  - [ ] Filter by Session Category
  - [ ] Filter by date range
- [ ] Reservations Index supports:
  - [ ] Search by ID
  - [ ] Search by session name
  - [ ] Filter by facility
  - [ ] Filter by date range
- [ ] Filters preserve values and provide a clear “Reset” option

### Azure Deployment

- [ ] Azure App Service deployed and reachable
- [ ] Azure SQL Database configured and seeded via migrations
- [ ] Azure Blob Storage container created and in use
- [ ] Connection strings configured securely in Azure
- [ ] Core scenarios (CRUD, upload, filtering, validation) tested on the cloud URL

***

## Acknowledgements

- CLDV6211 Cloud Development A Module – Emeris  
- Microsoft ASP.NET Core Documentation  
- Microsoft Azure App Service, Azure SQL, Azure Blob Storage  
- Bootstrap 5 Component Library  
- Font Awesome 6 Icon Library  
- Google Fonts (Inter, Playfair Display)  

***
