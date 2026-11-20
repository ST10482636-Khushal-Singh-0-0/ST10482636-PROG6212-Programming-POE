# RaceDay — Event Management Platform

**Student Number:** ST10482636  
**Module:** PROG6212 — Programming 2B (Portfolio of Evidence)

RaceDay is an end-to-end sporting and endurance event management platform built across three progressive phases. It allows **Event Organisers** to create, manage, and publish marathons, cycling tours, trail runs, and triathlons — complete with race categories, capacity limits, banner artwork, and official race results — while enabling **Participants** to browse upcoming races, enrol into specific distance categories, receive race numbers, and track their personal race history.

---

## Video Walkthroughs (YouTube)

- **Part 1 — System Architecture, Database Design & Wireframes:** [Watch Part 1](https://youtu.be/04soipyka1s?si=BnSzY9LoytZW9uYI)
- **Part 2 — RESTful Web API, SQL Server & Role-Based Authorization:** [Watch Part 2](https://www.youtube.com/watch?v=2fqM0dSWzFI&t=41s)
- **Part 3 — ASP.NET Core MVC Front-End, Azure Blob Storage, Docker & CI/CD:** [Watch Part 3](https://youtu.be/dhFnPcmsRHU?si=g6_0QVrBixp-eFGM)

---

## How the System Works

RaceDay follows a decoupled **client-server microservice-style architecture**:

```text
┌─────────────────────────────┐         HTTP / JSON + Multipart         ┌─────────────────────────────┐
│   Part 3: RaceDay.Web       │ ──────────────────────────────────────► │   Part 2: RaceDay.Api       │
│   ASP.NET Core MVC (Docker) │ ◄────────────────────────────────────── │   ASP.NET Core Web API      │
│   http://localhost:8080     │   Session + X-User-Id / X-User-Role     │   http://localhost:5003     │
└─────────────────────────────┘                                         └──────────────┬──────────────┘
                                                                                       │
                                                                 ┌─────────────────────┴─────────────────────┐
                                                                 ▼                                           ▼
                                                  ┌─────────────────────────────┐             ┌─────────────────────────────┐
                                                  │   SQL Server (RaceDayDB)    │             │   Azure Blob Storage        │
                                                  │   Users, Venues, Events,    │             │   (Azurite / Azure Cloud)   │
                                                  │   Categories, Enrolments,   │             │   • event-banners           │
                                                  │   Results                   │             │   • profile-pictures        │
                                                  └─────────────────────────────┘             └─────────────────────────────┘
```

### 1. Authentication & Role-Based Access Control (RBAC)

- Users register and log in as either an **Organiser** or a **Participant**.
- **RaceDay.Web (Part 3)** stores the authenticated user's session (`UserId`, `UserRole`, `UserName`, `UserEmail`) and protects restricted routes using a custom `[SessionAuthorize]` filter.
- When `RaceDay.Web` communicates server-to-server with `RaceDay.Api`, a delegating `HttpClient` handler automatically forwards `X-User-Id` and `X-User-Role` headers alongside session support, allowing **RaceDay.Api (Part 2)** to enforce `[AuthorizeRole("Organiser")]` and `[AuthorizeRole("Participant")]` consistently across both Swagger and the MVC front-end.

### 2. Core User Workflows

#### Public Event Discovery (`HomeController`)

Any visitor can:
- Browse upcoming events.
- Filter by event name, sport type, or location.
- Inspect available race categories, distances, entry fees, and capacities.

#### Organiser Management (`OrganiserController`)

Organisers can:

- **Create, Edit & Delete Events:** Manage events through multipart form submissions:
  - `POST /api/events/with-banner`
  - `PUT /api/events/{id}/with-banner`
  - `DELETE /api/events/{id}`
- **Category Management:** Configure multiple distance categories per event (e.g., 10 km, 21.1 km, 42.2 km) with custom entry fees and participant caps.
- **Enrolment & Results Publishing:** View registered athletes per event and publish official finish times and positions using `POST /api/results`.

#### Participant Portal (`ParticipantController`)

Participants can:

- **Race Enrolment:** Enrol in open categories (`POST /api/enrolments`), with automatic duplicate-entry checks, capacity enforcement, and sequential race-number generation.
- **My Enrolments & My Results:** View active race entries (`GET /api/enrolments/my-entries`) and historical race results (`GET /api/results/my-history`).
- **Profile Management:** Update personal details and upload profile avatars stored in Azure Blob Storage.

### 3. Cloud Media Storage & Containerisation

- **Azure Blob Storage (`BlobStorageService`):** Uploaded event banners (`event-banners` container) and user avatars (`profile-pictures` container) are streamed directly to Azure Blob Storage (or the local **Azurite** emulator during development), storing only the resulting public blob URI in SQL Server.
- **Docker Containerisation:** `RaceDay.Web` is packaged using a multi-stage `Dockerfile`:
  - Build stage: `mcr.microsoft.com/dotnet/sdk:10.0`
  - Runtime stage: `mcr.microsoft.com/dotnet/aspnet:10.0`
  - Exposed port: `8080`
  - Host API communication: `http://host.docker.internal:5003/`

---

## Project Structure

```text
ST10482636-PROG6212-Programming POE/
│
├── Part 1/                         # Architecture documentation, ERD, UI wireframes & planning
│
├── Part 2/
│   └── RaceDay.Api/                # ASP.NET Core 10 REST API, EF Core, SQL Server & Blob Service
│       ├── Controllers/            # Auth, Events, Categories, Enrolments, Results, Users
│       ├── Data/                   # RaceDayDbContext & Database Seeder
│       ├── DTOs/                   # Request/Response Data Transfer Objects
│       ├── Filters/                # AuthorizeRoleAttribute (Session + Header RBAC)
│       ├── Models/                 # User, Venue, Event, EventCategory, Enrolment, Result
│       └── Services/               # Azure Blob Storage integration
│
├── Part 3/
│   └── RaceDay.Web/                # ASP.NET Core 10 MVC Front-End
│       ├── Controllers/            # Account, Home, Organiser, Participant, BaseApiController
│       ├── Filters/                # SessionAuthorizeAttribute
│       ├── Models/                 # ViewModels & Session DTOs
│       ├── Views/                  # Razor Views styled with Bootstrap 5 & custom sports theme
│       └── Dockerfile              # Multi-stage production container definition
│
└── .github/
    └── workflows/                  # CI/CD build & test automation pipeline
```

---

## Prerequisites & Local Setup

Before running RaceDay locally, ensure the following are installed:

| Requirement | Description |
|---|---|
| [.NET 10.0 SDK](https://dotnet.microsoft.com/download) | Required to build and run the application |
| SQL Server / SQL Server Express | Database server (`SQLEXPRESS`) |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Required to containerise and run the MVC application |
| [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) | Local Azure Blob Storage emulator |

### Step 1: Start the Part 2 Web API (`RaceDay.Api`)

Open a PowerShell terminal and start the API listening on all interfaces (`0.0.0.0:5003`) so it is reachable from both your host machine and the Docker container.

```powershell
cd "Part 2\RaceDay.Api"

dotnet run --urls "http://0.0.0.0:5003"
```

**Swagger UI:** http://localhost:5003/swagger

### Step 2: Build & Run the Part 3 MVC Web App in Docker (`RaceDay.Web`)

Open a second PowerShell terminal in the repository root and execute:

```powershell
docker build -t raceday-web:latest "Part 3/RaceDay.Web"

docker run -d -p 8080:8080 `
  -e ApiSettings__BaseUrl="http://host.docker.internal:5003/" `
  --name raceday-web `
  raceday-web:latest
```

**RaceDay Web Application:** http://localhost:8080

---

## AI Usage & Academic Integrity Disclosure

In accordance with institutional academic integrity guidelines, **Artificial Intelligence (AI) tools were utilized during the development of this project** as a productivity and debugging assistant.

AI assistance was used specifically for:

- Boilerplate scaffolding.
- Troubleshooting serialization and routing issues.
- Dockerfile optimization.
- Documentation structuring.

### Human Verification & Ownership

All AI-assisted suggestions and generated snippets were thoroughly reviewed, tested, modified, and verified line-by-line by the author (**ST10482636**) to ensure alignment with the PROG6212 rubric requirements, database schema integrity, security rules, and end-to-end runtime functionality.

---

**Developed as part of the PROG6212 Programming 2B Portfolio of Evidence.**
