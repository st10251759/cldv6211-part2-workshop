-- =============================================
-- MEDIBOOK DATABASE SETUP SCRIPT
-- MediBookDB - Full Setup with Sample Data
-- Updated for Session Category
-- =============================================

-- =============================================
-- 1. CREATE DATABASE
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MediBookDB')
BEGIN
    CREATE DATABASE MediBookDB;
END
GO

-- =============================================
-- 2. USE DATABASE
-- =============================================
USE MediBookDB;
GO

-- =============================================
-- 3. DROP TABLES (if re-running script)
-- =============================================
IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID('dbo.MedicalSessions', 'U') IS NOT NULL DROP TABLE dbo.MedicalSessions;
IF OBJECT_ID('dbo.Facilities', 'U') IS NOT NULL DROP TABLE dbo.Facilities;
GO

-- =============================================
-- 4. CREATE TABLES
-- =============================================

-- FACILITIES TABLE
CREATE TABLE dbo.Facilities
(
    FacilityId  INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Location    NVARCHAR(200)   NOT NULL,
    Description NVARCHAR(300)   NULL,
    Capacity    INT             NOT NULL,
    ImageUrl    NVARCHAR(500)   NULL,

    CONSTRAINT PK_Facilities PRIMARY KEY (FacilityId),
    CONSTRAINT CK_Facilities_Cap CHECK (Capacity >= 1)
);
GO

-- MEDICAL SESSIONS TABLE
CREATE TABLE dbo.MedicalSessions
(
    SessionId   INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL,
    Description NVARCHAR(300)   NULL,
    StartDate   DATETIME2       NOT NULL,
    EndDate     DATETIME2       NOT NULL,
    ImageUrl    NVARCHAR(500)   NULL,
    Category    INT             NOT NULL,

    CONSTRAINT PK_MedicalSessions PRIMARY KEY (SessionId),
    CONSTRAINT CK_MedicalSessions_Dates CHECK (EndDate > StartDate)
);
GO

-- RESERVATIONS TABLE
CREATE TABLE dbo.Reservations
(
    ReservationId   INT         NOT NULL IDENTITY(1,1),
    FacilityId      INT         NOT NULL,
    SessionId       INT         NOT NULL,
    StartDate       DATETIME2   NOT NULL,
    EndDate         DATETIME2   NOT NULL,

    CONSTRAINT PK_Reservations PRIMARY KEY (ReservationId),
    CONSTRAINT FK_Reservations_Facilities
        FOREIGN KEY (FacilityId) REFERENCES dbo.Facilities(FacilityId),
    CONSTRAINT FK_Reservations_MedicalSessions
        FOREIGN KEY (SessionId) REFERENCES dbo.MedicalSessions(SessionId),
    CONSTRAINT CK_Reservations_Dates
        CHECK (EndDate > StartDate)
);
GO

-- =============================================
-- 5. INSERT FACILITIES (6 Records)
-- =============================================
INSERT INTO dbo.Facilities (Name, Location, Description, Capacity, ImageUrl)
VALUES
(
    'Operating Theatre 1',
    'Building A, Floor 2, MediBook Central Hospital, Durban, KwaZulu-Natal',
    'A fully equipped primary operating theatre featuring state-of-the-art surgical lighting, integrated anaesthesia systems, and a sterile environment compliant with international surgical standards. Suitable for major and minor surgical procedures.',
    12,
    '/images/placeholder-facility.jpg'
),
(
    'Cardiac Procedure Suite',
    'Building B, Floor 3, MediBook Heart Centre, Johannesburg, Gauteng',
    'A specialised cardiac catheterisation and intervention suite equipped with advanced imaging technology, haemodynamic monitoring systems, and dedicated cardiac nursing support. Designed for interventional cardiology and electrophysiology procedures.',
    10,
    '/images/placeholder-facility.jpg'
),
(
    'Consultation Room Block C',
    'Building C, Ground Floor, MediBook Outpatient Centre, Cape Town, Western Cape',
    'A block of modern, private consultation rooms designed for outpatient appointments, specialist referrals and patient assessments. Each room is equipped with examination beds, diagnostic tools and electronic patient record terminals.',
    4,
    '/images/placeholder-facility.jpg'
),
(
    'Radiology & Imaging Suite',
    'Building A, Floor 1, MediBook Diagnostic Centre, Pretoria, Gauteng',
    'A comprehensive imaging suite housing MRI, CT and X-ray equipment within lead-lined rooms. Supports diagnostic imaging for a full range of clinical specialties with on-site radiologist interpretation and digital report delivery.',
    8,
    '/images/placeholder-facility.jpg'
),
(
    'Paediatric Ward — Bay 4',
    'Building D, Floor 2, MediBook Childrens Unit, Durban, KwaZulu-Natal',
    'A child-friendly, brightly decorated ward bay accommodating paediatric in-patients from newborns to adolescents. Equipped with age-appropriate monitoring equipment, dedicated nursing stations and family waiting areas adjacent to each bay.',
    20,
    '/images/placeholder-facility.jpg'
),
(
    'Rehabilitation Gym & Physio Studio',
    'Building E, Ground Floor, MediBook Rehab Centre, Port Elizabeth, Eastern Cape',
    'A spacious rehabilitation facility featuring physiotherapy treatment bays, hydrotherapy equipment, parallel bars, resistance training apparatus and dedicated occupational therapy zones. Used for post-surgical and neurological recovery programmes.',
    25,
    '/images/placeholder-facility.jpg'
);
GO

-- =============================================
-- 6. INSERT MEDICAL SESSIONS (6 Records)
--    Category values must match your SessionCategory enum
-- =============================================
INSERT INTO dbo.MedicalSessions (Name, Description, StartDate, EndDate, ImageUrl, Category)
VALUES
(
    'Hip Replacement Surgery — Patient Batch A',
    'A scheduled batch of elective total hip replacement procedures for pre-assessed patients in the orthopaedic programme. Procedures include pre-operative preparation, surgical intervention under general anaesthesia and post-operative recovery handover to ward nursing staff.',
    '2026-05-20 07:00:00',
    '2026-05-20 15:00:00',
    '/images/placeholder-session.jpg',
    1
),
(
    'Cardiac Catheterisation — Batch B',
    'A series of diagnostic and interventional cardiac catheterisation procedures for patients referred by the cardiology department. Includes coronary angiography, stent placements and balloon angioplasty under fluoroscopic guidance by a senior interventional cardiologist.',
    '2026-06-10 08:00:00',
    '2026-06-10 14:00:00',
    '/images/placeholder-session.jpg',
    3
),
(
    'Paediatric Vaccination Drive — Winter Campaign',
    'A community outreach vaccination session targeting children aged 0 to 12 years as part of the national winter immunisation campaign. Covers MMR, polio, influenza and meningococcal vaccinations administered by a paediatric nursing team with parental consent documentation.',
    '2026-07-01 08:30:00',
    '2026-07-01 13:00:00',
    '/images/placeholder-session.jpg',
    7
),
(
    'Full-Body MRI Screening — Executive Health Package',
    'Comprehensive full-body MRI screening sessions for executive health package clients. Each session includes neurological, abdominal, musculoskeletal and cardiovascular imaging sequences with a written radiologist report delivered within 48 hours of the scan.',
    '2026-08-05 07:30:00',
    '2026-08-05 17:00:00',
    '/images/placeholder-session.jpg',
    7
),
(
    'Post-Operative Physiotherapy — Knee Rehab Group',
    'A structured group physiotherapy rehabilitation session for patients recovering from total knee replacement surgery. The programme includes gait re-education, range-of-motion exercises, hydrotherapy and progressive resistance training supervised by a senior physiotherapist.',
    '2026-09-12 09:00:00',
    '2026-09-12 12:00:00',
    '/images/placeholder-session.jpg',
    5
),
(
    'Outpatient Specialist Clinic — Endocrinology',
    'A scheduled outpatient specialist clinic for endocrinology referrals covering diabetes management, thyroid disorders, adrenal conditions and metabolic disease follow-ups. Each consultation slot is 30 minutes with electronic prescription and laboratory request integration.',
    '2026-10-15 08:00:00',
    '2026-10-15 16:00:00',
    '/images/placeholder-session.jpg',
    2
);
GO

-- =============================================
-- 7. INSERT RESERVATIONS (6 Records)
-- =============================================
INSERT INTO dbo.Reservations (FacilityId, SessionId, StartDate, EndDate)
VALUES
(
    1, 1,
    '2026-05-20 06:30:00',
    '2026-05-20 15:30:00'
),
(
    2, 2,
    '2026-06-10 07:30:00',
    '2026-06-10 15:00:00'
),
(
    5, 3,
    '2026-07-01 08:00:00',
    '2026-07-01 13:30:00'
),
(
    4, 4,
    '2026-08-05 07:00:00',
    '2026-08-05 17:30:00'
),
(
    6, 5,
    '2026-09-12 08:30:00',
    '2026-09-12 12:30:00'
),
(
    3, 6,
    '2026-10-15 07:30:00',
    '2026-10-15 16:30:00'
);
GO

-- =============================================
-- 8. VERIFY INSERTS
-- =============================================
SELECT 'Facilities' AS TableName, COUNT(*) AS RecordCount FROM dbo.Facilities
UNION ALL
SELECT 'MedicalSessions' AS TableName, COUNT(*) AS RecordCount FROM dbo.MedicalSessions
UNION ALL
SELECT 'Reservations' AS TableName, COUNT(*) AS RecordCount FROM dbo.Reservations;
GO

-- =============================================
-- 9. PREVIEW JOINED RESERVATIONS VIEW
-- =============================================
SELECT
    r.ReservationId,
    f.Name AS FacilityName,
    f.Location AS FacilityLocation,
    f.Capacity,
    s.Name AS SessionName,
    s.Category AS SessionCategory,
    s.Description AS SessionDescription,
    r.StartDate AS ReservationStart,
    r.EndDate AS ReservationEnd
FROM dbo.Reservations r
    INNER JOIN dbo.Facilities f ON r.FacilityId = f.FacilityId
    INNER JOIN dbo.MedicalSessions s ON r.SessionId = s.SessionId
ORDER BY r.StartDate;
GO