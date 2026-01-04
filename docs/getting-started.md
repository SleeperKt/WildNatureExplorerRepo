# Getting Started

## Prerequisites
- .NET 8
- PostgreSQL
- Docker (optional)

## Running the API
1. Clone the repository
2. Configure environment variables (.env)
3. Run `dotnet build`
3. Run `docker compose build`
3. Run `docker compose up`
4. Open Swagger UI at http://localhost:5000/swagger/index.html
5. Check Database   docker exec -it wild-nature-explorer-db psql -U (user name) -d (database name)

## Authentication
This API uses JWT Bearer authentication.
