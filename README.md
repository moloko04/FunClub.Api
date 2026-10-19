# Fun Club Management System API

This is an ASP.NET Core Web API built for managing student Fun Club activities.

## Features
- Entity Framework Core Code-First with SQL Server (LocalDB)
- Authentication using Session Cookies and BCrypt password hashing
- Role-based access (Lecturer vs Participant)
- Swagger UI for testing endpoints

## Setup Instructions
1. Clone the repo.
2. Run `Update-Database` in the Package Manager Console to create the local database.
3. Press **F5** to start the API.
4. Open Swagger at `/swagger` to test the endpoints.

## Test Credentials
- **Lecturer Secret:** `FunClub-Lecturer-Secret-2026`

## Testing
Run `dotnet test` or open **Test ? Test Explorer** in Visual Studio.

## CI/CD
A GitHub Actions workflow (`.github/workflows/ci.yml`) automatically builds and tests the project on every push to `main`.