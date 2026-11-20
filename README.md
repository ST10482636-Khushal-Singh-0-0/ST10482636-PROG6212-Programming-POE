# RaceDay — Endurance Event Management Platform (PROG6212 POE Part 3)

RaceDay is a full-stack ASP.NET Core (.NET 10) web platform connecting race Organisers and endurance athletes (Participants) for marathons, trail runs, cycling tours, and triathlons.

## Architecture Overview
- **Part 2 (`Part 2/RaceDay.Api`)**: ASP.NET Core Web API with Entity Framework Core (SQL Server), role-aware endpoints, and **Azure Blob Storage** (`event-banners` and `profile-pictures` containers).
- **Part 3 (`Part 3/RaceDay.Web`)**: ASP.NET Core MVC front-end consuming the Part 2 REST API via typed `HttpClient`, featuring session-based authentication, role-specific navigation (`Organiser` vs `Participant`), and a sporting UI.
- **Containerisation (`Part 3/RaceDay.Web/Dockerfile`)**: Multi-stage Docker build running the MVC web application on port `8080`.
- **CI/CD (`.github/workflows/build-and-test.yml`)**: Automated GitHub Actions workflow building both the Part 2 API and Part 3 MVC projects on every push and pull request.

## Demo Video
- **Unlisted YouTube Demonstration**: `https://youtu.be/dhFnPcmsRHU?si=ynsu6quW4bWnNtyA`

## Running the Application Locally
1. Ensure SQL Server (`localhost\SQLEXPRESS`) is running and `Part 2/RaceDay.Api/appsettings.json` contains valid `DefaultConnection` and `AzureBlobStorage` connection strings.
2. Start the Part 2 API (`Part 2/RaceDay.Api`):
   ```bash
   dotnet run --project "Part 2/RaceDay.Api/RaceDay.Api.csproj"