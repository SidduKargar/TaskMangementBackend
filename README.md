# TaskManagementAPI

A simple Task Management REST API built with .NET 8, Entity Framework Core (In-Memory), and JWT authentication. Designed to manage tasks with features like user authentication, task creation, and status updates.

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
cd TaskManagementAPI
2. Run with .NET CLI

cd TaskManagementAPI
dotnet build
dotnet run
The API will start on:

Http:- http://localhost:8080
IIS:- http://localhost:21562

3. Run with Docker (Optional)

docker build -t taskmanagementapi .
docker run -d -p 5000:80 taskmanagementapi

Http:- http://localhost:8080
IIS:- http://localhost:21562

4. Run Tests
cd TaskManagementAPI.Test
dotnet test
```
