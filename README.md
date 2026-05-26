# Wild Nature Explorer

A web platform for exploring wildlife: species catalogues, maps, user sightings, roles and moderation, plus AI integrations for image analysis and Q&A. This repository contains an **ASP.NET Core** backend, a **React (Vite)** SPA, **Docker Compose** with **PostgreSQL + PostGIS**, and optionally an **Android companion app**.

## Project Structure

- **`code/`** — Backend (ASP.NET Core 8 REST API, Domain, Application, Infrastructure).
- **`frontend/`** — Frontend (React 19, Vite, Leaflet for maps).
- **`android-alert-app/`** — Companion Android application.
- **`documentation/`** — API guides, containerization, cloud, and CI/CD docs.
- **`WildNatureExplorer.Tests/`** — Unit and integration tests.

## Quick Start (Docker Compose)

Requires **Docker Desktop** (or Docker Engine + Compose v2).

1. Prepare your environment variables:
   ```bash
   cp .env.example .env
   ```
   *Important: Set `GROQ_API_KEY` (required for API to boot) and configure `DB_*` / `JWT_*` keys.*
2. Start the application:
   ```bash
   docker compose up --build
   ```

**Default Endpoints:**
- **Web UI:** `http://localhost:5174`
- **API:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger`

## Local Development (Without Docker)

To run locally, you need **.NET 8 SDK**, **Node.js**, and a local **PostgreSQL 15 + PostGIS** database. Ensure you set the environment variables exactly as outlined in `.env.example`.

**Run Backend:**
```bash
cd code/WildNatureExplorer.API
dotnet run
```

**Run Frontend:**
```bash
cd frontend
npm ci
npm run dev
```
*(The frontend will automatically proxy `/api` requests to `http://localhost:5000` during development).*

## Testing

Run the entire test suite from the repository root:
```bash
dotnet test WildNatureExplorer.Tests/WildNatureExplorer.Tests.csproj
```
