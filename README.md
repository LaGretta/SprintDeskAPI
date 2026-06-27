# SprintDeskAPI

SprintDeskAPI is a team project and task management system built with ASP.NET Core Web API and a modern React frontend. It supports JWT authentication, role-based project and task operations, paginated project/task lists, task status updates, assignment by user id, and task comments.

## How To Run The Full App

This repository contains two apps:

- `SprintDeskAPI/` - ASP.NET Core backend API
- `frontend/` - React frontend

Running the project from Rider starts the backend only. The frontend must be started separately.

Terminal 1 - backend:

```powershell
cd D:\GaGu\SprintDeskAPI
dotnet run --project SprintDeskAPI\SprintDeskAPI.csproj --urls http://localhost:5100
```

Backend URLs:

- API status page: `http://localhost:5100/`
- Swagger: `http://localhost:5100/swagger`

Terminal 2 - frontend:

```powershell
cd D:\GaGu\SprintDeskAPI\frontend
npm install
npm run dev
```

Frontend URL:

- `http://localhost:5173/`

## Tech Stack

- Backend: ASP.NET Core Web API, .NET 10, Entity Framework Core, SQL Server
- Auth: JWT Bearer tokens, BCrypt password hashing
- Mapping: AutoMapper
- API docs: Swagger / OpenAPI
- Tests: xUnit, Moq, FluentAssertions
- Frontend: Vite, React, TypeScript, CSS, lucide-react

## Features

- Register and login with JWT token storage
- Authorized API requests with `Authorization: Bearer <token>`
- Role-aware UI for Admin, Manager, and Developer users
- Projects list with pagination
- Project details, create, update, complete, archive, and delete actions where the API role allows them
- Tasks list, my tasks, and tasks by selected project
- Create, update, delete, assign, and status-change task flows
- Task comments list with add/delete actions
- Loading, error, empty, and success states throughout the frontend
- Configurable frontend API base URL

## Backend Setup

1. Open the solution:

   ```powershell
   cd D:\GaGu\SprintDeskAPI
   ```

2. Check the SQL Server connection string in `SprintDeskAPI/appsettings.json`:

   ```json
   "DefaultConnection": "Server=DESKTOP-EBQ9QOB;Database=SprintDeskPortfolioDb;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

3. Run the API:

   ```powershell
   dotnet run --project SprintDeskAPI\SprintDeskAPI.csproj --urls http://localhost:5100
   ```

In Development, the API applies EF Core migrations automatically with `MigrateAsync()`. Swagger is available at `http://localhost:5100/swagger/index.html`.

The initial migration is already included in the repository. For a fresh clone, usually only the second command is needed. The two EF Core commands used for migration workflow are:

```powershell
dotnet ef migrations add InitialCreate --project SprintDeskAPI\SprintDeskAPI.csproj --startup-project SprintDeskAPI\SprintDeskAPI.csproj --output-dir Migrations
dotnet ef database update --project SprintDeskAPI\SprintDeskAPI.csproj --startup-project SprintDeskAPI\SprintDeskAPI.csproj
```

If Rider says `address already in use` for port `5100`, another API instance is already running. Stop the old process or change the port in `SprintDeskAPI/Properties/launchSettings.json`.

## Frontend Setup

1. Install dependencies:

   ```powershell
   cd D:\GaGu\SprintDeskAPI\frontend
   npm install
   ```

2. Configure the API URL. Copy `.env.example` to `.env` if you want a local override:

   ```env
   VITE_API_BASE_URL=http://localhost:5100/api
   ```

3. Run the frontend:

   ```powershell
   npm run dev
   ```

The app runs at `http://127.0.0.1:5173/` or the URL printed by Vite.

## API And Auth Usage

Register:

```http
POST /api/Auth/register
Content-Type: application/json

{
  "fullName": "Jane Developer",
  "email": "jane@example.com",
  "password": "Password123!"
}
```

Login:

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "jane@example.com",
  "password": "Password123!"
}
```

Use the returned token for protected endpoints:

```http
Authorization: Bearer <token>
```

Roles are enforced by the API:

- Admin: project create/update/complete/archive/delete, task create/update/assign/delete
- Manager: project create/update/complete, task create/update/assign/delete
- Developer: read projects/tasks, view my tasks, update task status, add comments, delete own comments

The backend does not currently expose a users list endpoint. The frontend therefore accepts an assigned user id when creating or assigning tasks instead of showing a fake user picker.

## Testing

Run backend build and tests:

```powershell
dotnet build SprintDeskAPI.sln
dotnet test SprintDeskAPI.sln --no-build
```

Run frontend build:

```powershell
cd frontend
npm run build
```

Verified locally:

- Backend build: passed
- Backend tests: 17 passed
- Frontend install: passed
- Frontend production build: passed
- API smoke test: Swagger reachable, unauthenticated project request returns 401, register returns JWT, authorized project/task pages return paged responses
- Browser smoke test: frontend loads, registration reaches dashboard, no browser console errors

Note: `dotnet build` currently reports a NuGet advisory warning for AutoMapper 12.0.1.

## Screenshots

Portfolio screenshots to add later:

- Backend tests passing
- Login and registration screen
- Dashboard overview
- Projects page
- Tasks and comments page
