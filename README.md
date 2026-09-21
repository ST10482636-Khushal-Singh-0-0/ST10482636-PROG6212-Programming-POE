# RaceDay - Event Management System (Part 1)
# Khushal Singh - ST10482636

## 1. System Overview
RaceDay is a full-stack web-based event management system built specifically for South African road running, walking, and cycling events (such as the Comrades Marathon, Soweto Marathon, and Cape Town Cycle Tour). 

The platform replaces manual registrations and fragmented communication channels with a centralized system where organizers can host and manage events, and participants can enter races, select categories, and track their performance.

---

## 2. System User Roles
The system enforces two distinct roles:

* **Organiser**:
  * Creates, updates, and deletes road events.
  * Adds event distance categories, entry fees, and runner caps.
  * Views event enrolment rosters.
  * Captures official race results (finish times and positions) post-race.

* **Participant**:
  * Registers an account and manages profile information.
  * Browses upcoming road running, walking, and cycling events.
  * Enrols in specific categories and automatically receives an allocated race number.
  * Views active race entries and personal historical race results.

---

## 3. Part 1 Planning Deliverables
All required planning documents are located inside the `/docs` folder:
* **Entity Relationship Diagram (ERD)**: `docs/RaceDay_ERD.png` — Details the 6 relational database entities (`Users`, `Venues`, `Events`, `EventCategories`, `Enrolments`, `Results`), primary/foreign keys, and cardinalities.
* **RESTful API Endpoint Plan**: `docs/RaceDay_API_Endpoint_Plan.md` — Complete HTTP specification covering endpoints, role permissions, request bodies, and expected status codes.
* **Database Setup Script**: `docs/RaceDay_Database_Setup.sql` — Clean, tested T-SQL script containing schema creation, table constraints, and realistic South African seed data.

---

## 4. Database Setup Instructions
To run and test the SQL script:
1. Open **SQL Server Management Studio (SSMS)** or Visual Studio SQL Server Data Tools.
2. Connect to your local SQL Server instance.
3. Open `docs/RaceDay_Database_Setup.sql`.
4. Click **Execute** (or press `F5`).
5. The script automatically drops existing instances of `RaceDayDB`, creates the fresh database schema, applies all foreign keys and constraints, seeds initial data, and runs a verification query showing current event entries.

---

## 5. Video Presentation Link
Walkthrough and explanation of the ERD structure, REST API endpoint plan, role enforcement, and live SSMS database execution:
* **YouTube Link (Unlisted):** add this now now

---

## 6. AI Disclosure
AI tools were consulted during the initial brainstorming and formatting of the API route plans and relational constraints. All database designs, entity structures, and table mappings were analyzed, reviewed, and tested personally in SSMS.
