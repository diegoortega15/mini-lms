
# Mini-LMS - Course Management System

A simple course management system with batch upload via CSV.

## Technologies

- **Backend:** .NET 8 Web API, Entity Framework, SQL Server
- **Frontend:** Angular 17, PrimeNG
- **Infrastructure:** Docker, RabbitMQ
- **Database:** SQL Server 2022

## Features

- Course CRUD
- CSV upload to enroll users
- Background processing with RabbitMQ
- Prevents duplicate emails
- Responsive interface

## How to Run

1. **Install Docker Desktop**

2. **Clone and run:**
   ```bash
   git clone <repo-url>
   cd project-folder
   docker-compose up --build
   ```

3. **Access:**
   - App: http://localhost:4200
   - API: http://localhost:5000

## CSV Upload

CSV file format:
```csv
email,name
joao@empresa.com,João Silva
maria@empresa.com,Maria Santos
```

- The first line must have the header
- Duplicate emails are automatically ignored

## Database

**Connection:**
- Server: `localhost,1433`
- User: `sa`
- Password: `MiniLMS123!`

You can use SQL Server Management Studio or Azure Data Studio.

## Project Structure

```
backend/         # .NET 8 API
frontend/        # Angular
docker-compose.yml
```

## Tests

To run unit tests:
```bash
docker run --rm -v .\backend:/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet test tests/MiniLMS.Tests/MiniLMS.Tests.csproj
```
