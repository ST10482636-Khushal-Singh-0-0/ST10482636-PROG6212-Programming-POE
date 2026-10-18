# RaceDay - Event Management System (Part 2)
# Khushal Singh - ST10482636

## 1. System Overview
RaceDay is a full-stack web-based event management system built specifically for South African road running, walking, and cycling events (such as the Comrades Marathon, Soweto Marathon, and Cape Town Cycle Tour).
The platform replaces manual registrations and fragmented communication channels with a centralized system where organizers can host and manage events, and participants can enter races, select categories, and track their performance.

---

## 2. System User Roles
The system enforces two distinct roles across the entire platform:
- **Organiser**:
  - Creates, updates, and deletes road events.
  - Adds event distance categories, entry fees, and runner caps.
  - Views event enrolment rosters.
  - Captures official race results (finish times and positions) post-race.
- **Participant**:
  - Registers an account and manages profile information.
  - Browses upcoming road running, walking, and cycling events.
  - Enrols in specific categories and automatically receives an allocated race number.
  - Views active race entries and personal historical race results.

---

## 3. Project Structure
The repository is modularly organized across both submission phases:
- **/docs**: Contains all Part 1 planning documentation:
  - `docs/ERD.pdf`: Entity Relationship Diagram detailing all 6 relational entities, primary/foreign keys, and cardinalities.
  - `docs/RaceDay-API-Plan.md`: Structured RESTful endpoint specification covering HTTP methods, routes, role requirements, request bodies, and expected status codes.
  - `docs/RaceDay-DB.sql`: Schema creation script with constraints and South African seed data.
- **/Part 2**: Contains the implementation for the backend services:
  - `Part 2/RaceDay.Api/`: ASP.NET Core Web API with Entity Framework Core, role-based session state authorization, and Swagger UI.
  - `Part 2/RaceDay.Tests/`: Automated xUnit testing suite verifying authentication, role boundaries, and participant enrolments using an in-memory database.
  - `Part 2/RaceDay.sln`: Visual Studio solution grouping the API and testing projects.
- **.github/workflows/build-and-test.yml**: Automated CI/CD workflow executing restore, build, and test runners on every commit.

---

## 4. Part 2 Setup & Execution Instructions

### Database Setup
1. Open SQL Server Management Studio (SSMS) or Visual Studio SQL Server Data Tools.
2. Connect to your local SQL Server instance.
3. Open `docs/RaceDay-DB.sql` and click **Execute** (or press **F5**) to generate the schema and seed records.
4. Verify the connection string inside `Part 2/RaceDay.Api/appsettings.json` points to your instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }

---

## 5. Video Presentation Link
* **YouTube Link:** 
1. **Part 1 Video Walkthrough:** https://youtu.be/04soipyka1s?si=mCi02WOCRTkglHHg
- Explanation of the ERD structure, REST API endpoint plan, role enforcement, and live SSMS database execution.

2. **Part 2 Video Demonstration:** NEEDSSSSSSSS TO RECORD
- Demonstration of the compiled Web API, interactive testing in Swagger UI, session-based role authorization, and running passing unit tests.

---

## 6. AI Disclosure
AI tools were consulted during the initial brainstorming and formatting of the API route plans and relational constraints. All database designs, entity structures, and table mappings were analyzed, reviewed, and tested personally in SSMS.
