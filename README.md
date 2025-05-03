# TaskManagementAPI

A simple Task Management REST API built with .NET 8, Entity Framework Core (In-Memory), and JWT authentication. Designed to manage tasks with features like user authentication, and task creation

## Features

- JWT-based authentication (Login/Register)
- In-memory database with Entity Framework Core
- CRUD operations for tasks
- Role-based access control
- Unit tests using xUnit
- Docker support

---

## Getting Started

### Prerequisites

Make sure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Git](https://git-scm.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional, for Docker)

---

## Running the Project Locally

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/TaskManagementAPI.git
```
### 2. Run with .NET CLI
```bash
cd TaskManagementAPI
dotnet build
dotnet run
```
The API will start on:
```bash
Http:- http://localhost:8080
IIS:- http://localhost:21562
```

### 3. Run Tests
```bash
cd ..
cd TaskManagementAPI.Test
dotnet test
```
### 4 Folder Structure
```bash
TaskManagementAPI/ # Root solution directory
│
├── TaskManagementAPI/ # Main .NET API project
│ ├── Controllers/ 
│ ├── Models/
  ├── DTOs/ 
│ ├── DataAccess/ # EF Core DB context
│ ├── Services/ 
│ ├── appsettings.json
│ └── Program.cs 
│
├── TaskManagementAPI.Test/ # Test project (xUnit)
│ ├── Controllers/
| ├── Services/
| ├── Mocks/ 
|
├── .dockerignore # Docker ignore rules
├── .editorconfig # Code style settings
├── Dockerfile # Docker build instructions
├── README.md # Project documentation
└── TaskManagementAPI.sln # Solution file

```
