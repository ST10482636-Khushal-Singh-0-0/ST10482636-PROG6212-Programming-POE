-- =============================================================
-- Script: RaceDay_Database_Setup.sql
-- Description: Creates the full relational schema & seed data for RaceDay
-- Target DBMS: Microsoft SQL Server (SSMS)
-- =============================================================

USE master;
GO

-- Drop database if it already exists to allow a clean run
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- =============================================================
-- TABLE 1: Users (Organisers & Participants)
-- =============================================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Role NVARCHAR(20) NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE()
);
GO

-- =============================================================
-- TABLE 2: Venues (Route locations & Coordinates)
-- =============================================================
CREATE TABLE Venues (
    VenueId INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(150) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    Latitude DECIMAL(9,6) NOT NULL,
    Longitude DECIMAL(9,6) NOT NULL
);
GO

-- =============================================================
-- TABLE 3: Events
-- =============================================================
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    VenueId INT NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    EventType NVARCHAR(30) NOT NULL CONSTRAINT CK_Events_Type CHECK (EventType IN ('Running Marathon', 'Trail Run', 'Cycle Tour', 'Fun Walk')),
    EventDate DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT 'Upcoming' CONSTRAINT CK_Events_Status CHECK (Status IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Events_Venue FOREIGN KEY (VenueId) REFERENCES Venues(VenueId)
);
GO

-- =============================================================
-- TABLE 4: EventCategories
-- =============================================================
CREATE TABLE EventCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(5,2) NOT NULL CONSTRAINT CK_Categories_Distance CHECK (DistanceKm > 0),
    EntryFee DECIMAL(10,2) NOT NULL CONSTRAINT CK_Categories_Fee CHECK (EntryFee >= 0),
    MaxParticipants INT NOT NULL CONSTRAINT CK_Categories_Max CHECK (MaxParticipants > 0),
    CONSTRAINT FK_Categories_Event FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE
);
GO

-- =============================================================
-- TABLE 5: Enrolments
-- =============================================================
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    ParticipantId INT NOT NULL,
    RaceNumber INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT GETDATE(),
    PaymentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Payment DEFAULT 'Confirmed' CONSTRAINT CK_Enrolments_Payment CHECK (PaymentStatus IN ('Pending', 'Confirmed', 'Refunded')),
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (CategoryId) REFERENCES EventCategories(CategoryId),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Enrolment_Participant_Category UNIQUE (CategoryId, ParticipantId),
    CONSTRAINT UQ_Enrolment_Category_RaceNumber UNIQUE (CategoryId, RaceNumber)
);
GO

-- =============================================================
-- TABLE 6: Results
-- =============================================================
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL CONSTRAINT UQ_Results_Enrolment UNIQUE,
    FinishTimeSeconds INT NULL, -- Total time taken in seconds
    OverallPosition INT NULL,
    CategoryPosition INT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Results_Status DEFAULT 'Finished' CONSTRAINT CK_Results_Status CHECK (Status IN ('Finished', 'DNF', 'DNS', 'Disqualified')),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId)
);
GO

-- =============================================================
-- SEED DATA (Minimum 2 Organisers, 2 Participants, 3 Events)
-- =============================================================

-- 1. Insert Users (2 Organisers, 2 Participants)
INSERT INTO Users (FullName, Email, PasswordHash, PhoneNumber, Role)
VALUES 
('Sipho Dlamini', 'sipho.organiser@raceday.co.za', 'AQAAAAEAACcQAAAAEJ3EXAMPLEHASH1==', '+27821112233', 'Organiser'),
('Elize Van Der Merwe', 'elize.events@raceday.co.za', 'AQAAAAEAACcQAAAAEJ3EXAMPLEHASH2==', '+27834445566', 'Organiser'),
('Kagiso Mokoena', 'kagiso.runner@gmail.com', 'AQAAAAEAACcQAAAAEJ3EXAMPLEHASH3==', '+27715557788', 'Participant'),
('Sarah Jenkins', 'sarah.j@outlook.com', 'AQAAAAEAACcQAAAAEJ3EXAMPLEHASH4==', '+27849990011', 'Participant');

-- 2. Insert Venues (Iconic South African locations)
INSERT INTO Venues (VenueName, City, Province, Latitude, Longitude)
VALUES 
('Moses Mabhida Stadium', 'Durban', 'KwaZulu-Natal', -29.828944, 31.030444),
('FNB Stadium', 'Johannesburg', 'Gauteng', -26.234794, 27.982436),
('Cape Town Stadium Precinct', 'Cape Town', 'Western Cape', -33.903611, 18.411111);

-- 3. Insert 3 Realistic South African Events
INSERT INTO Events (OrganiserId, VenueId, Title, Description, EventType, EventDate, Status)
VALUES 
(1, 1, 'Comrades Coastal Warmup', 'Preparation ultra-marathon and half-marathon along the coast.', 'Running Marathon', '2026-11-15 05:30:00', 'Upcoming'),
(1, 2, 'Soweto Heritage Road Race', 'Annual road run celebrating South African heritage and landmarks.', 'Running Marathon', '2026-12-05 06:00:00', 'Upcoming'),
(2, 3, 'Cape Peninsula Ocean Ride', 'Coastal road cycle tour looping through scenic passes.', 'Cycle Tour', '2027-02-20 06:15:00', 'Upcoming');

-- 4. Insert Categories for each Event
INSERT INTO EventCategories (EventId, CategoryName, DistanceKm, EntryFee, MaxParticipants)
VALUES 
-- Event 1: Comrades Coastal
(1, '42.2km Full Marathon', 42.20, 380.00, 2000),
(1, '21.1km Half Marathon', 21.10, 240.00, 3500),

-- Event 2: Soweto Heritage
(2, '21.1km Half Marathon', 21.10, 250.00, 4000),
(2, '10km Classic Run/Walk', 10.00, 160.00, 5000),

-- Event 3: Cape Ocean Ride
(3, '109km Full Loop', 109.00, 650.00, 8000),
(3, '42km Short Coastal', 42.00, 350.00, 3000);

-- 5. Insert Enrolments (Kagiso enters Marathon & Ride; Sarah enters Soweto 10km)
INSERT INTO Enrolments (CategoryId, ParticipantId, RaceNumber, PaymentStatus)
VALUES 
(1, 3, 1001, 'Confirmed'), -- Kagiso in Comrades 42.2km
(5, 3, 5001, 'Confirmed'), -- Kagiso in Cape Loop 109km
(4, 4, 3001, 'Confirmed'); -- Sarah in Soweto 10km

-- 6. Insert Completed/Sample Results
INSERT INTO Results (EnrolmentId, FinishTimeSeconds, OverallPosition, CategoryPosition, Status)
VALUES 
(1, 11730, 42, 12, 'Finished'); -- Kagiso: 3h 15m 30s
GO

-- Verification check query
SELECT 
    e.Title AS EventTitle,
    ec.CategoryName,
    u.FullName AS Participant,
    en.RaceNumber,
    r.FinishTimeSeconds,
    r.OverallPosition
FROM Enrolments en
JOIN EventCategories ec ON en.CategoryId = ec.CategoryId
JOIN Events e ON ec.EventId = e.EventId
JOIN Users u ON en.ParticipantId = u.UserId
LEFT JOIN Results r ON en.EnrolmentId = r.EnrolmentId;
GO