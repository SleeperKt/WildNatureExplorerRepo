# Wild Nature Explorer

A web platform for exploring wildlife: species catalogues, maps, user sightings, roles and moderation, plus AI integrations for image analysis and Q&A. This repository contains an **ASP.NET Core** backend, a **React (Vite)** SPA, **Docker Compose** with **PostgreSQL + PostGIS**, and optionally an **Android companion app** under `android-alert-app`.

---

## Repository layout

| Area | Description |
| --- | --- |
| **API** (`code/WildNatureExplorer.API`) | REST API, JWT, Swagger, EF Core migrations applied on startup |
| **Domain & application** (`code/WildNatureExplorer.Domain`, `WildNatureExplorer.Application`) | Business logic and contracts |
| **Infrastructure** (`code/WildNatureExplorer.Infrastructure`) | EF Core, repositories, external AI HTTP clients |
| **Frontend** (`frontend`) | React 19, Leaflet; dev server proxies `/api` to the API |
| **Tests** (`WildNatureExplorer.Tests`) | Unit and integration tests, coverage tooling |
| **Documentation** (`documentation/`) | API guides, containerization, CI/CD, cloud, database, AI docs |

End users get registration and sign-in, profiles, species search and PostGIS-backed geo features, a personal sightings library, admin capabilities, and AI flows when API keys are configured.

---

## Prerequisites

- **Docker Desktop** (or Docker Engine + Compose v2) for the full stack with one command  
- Or locally: **.NET SDK 8**, **Node.js** (LTS compatible with Vite 7), **PostgreSQL 15 with PostGIS** (aligned with image `postgis/postgis:15-3.4`)

---

## Quick start (Docker Compose)

From the repository root (where `docker-compose.yml` lives):

```bash
cp .env.example .env
# Edit .env: set DB password, JWT_* keys, and GROQ_API_KEY (see Environment variables).

docker compose up --build
```

When containers are healthy:

| Endpoint | URL |
| --- | --- |
| Web UI (nginx + SPA) | `http://localhost:5174` (or `WEB_PORT` from `.env`) |
| API | `http://localhost:5000` (host port from `PORT`; container listens on **5000**) |
| Swagger UI | `http://localhost:5000/swagger` |
| Health | `GET http://localhost:5000/health` |

The database is published on host **`5432`**. Volume **`db-data-dev`** persists data across restarts.

### Environment variables

Use **[`.env.example`](.env.example)** as the template for Compose. Summary:

- **`DB_*`** — database name and credentials; in Compose **`DB_HOST` must be `db`** (the Compose service name on `wne-net`).
- **`PORT`** — host port mapped to the API (**set explicitly**, e.g. `5000`).
- **`JWT_KEY`**, **`JWT_ISSUER`**, **`JWT_AUDIENCE`** — required for the API to start.
- **`GROQ_API_KEY`** — must be **non-empty** for the process to boot (`GroqChatService` registers at startup and throws if the key is missing). Use a real key from the Groq console for live requests.
- **`HF_API_KEY`**, **`ANIMALDETECT_API_KEY`** — required for the matching AI paths; some checks occur only when those endpoints run.
- **`ADMIN_EMAIL`** — used by auth/bootstrap logic (see `AuthService` configuration usage).
- **`CORS_ORIGINS`** — optional; if unset, defaults are defined in `docker-compose.yml` for typical local SPA origins.

Full variable tables and container notes: [Containerization guide](documentation/Containerization/Wild Nature Explorer — Containerization.md).

---

## Local development without Docker

1. Run **PostgreSQL with PostGIS** and create a database and user matching your **`DB_*`** values.
2. Export the same environment variables the API expects (IDE launch profile, user secrets, or shell). On your machine **`DB_HOST` is usually `localhost`**, not `db`.
3. Run the API:

```bash
cd code/WildNatureExplorer.API
dotnet run
```

Migrations run when the API process starts.

4. Run the frontend (`/api` is proxied to **`http://localhost:5000`** by default — see `frontend/vite.config.js`):

```bash
cd frontend
npm ci
npm run dev
```

Vite binds **`http://localhost:5173`** (`strictPort: true`). Ensure **`CORS_ORIGINS`** or built-in defaults allow that origin.

For Docker SPA builds targeting a public API origin, set build arg **`WNE_API_PUBLIC_BASE_URL`** (see `frontend/Dockerfile`). Local Compose often passes **`""`** so nginx proxies `/api`; see `docker/nginx-frontend.compose.conf`.

Base URLs and Swagger workflow: [`documentation/API/guides/getting-started.md`](documentation/API/guides/getting-started.md).

---

## API documentation and schemas

- Hub for OpenAPI docs, guides, and Spectral references: **[`documentation/API/README.md`](documentation/API/README.md)**  
- Committed snapshot (when present): `documentation/API/schemas/openapi.json`  
- At runtime: **`/swagger/v1/swagger.json`**

---

## Tests and coverage

From the **repository root**:

```bash
dotnet test WildNatureExplorer.Tests/WildNatureExplorer.Tests.csproj
```

Coverage scripts (Windows PowerShell):

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run-coverage.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\run-coverage-integration.ps1
```

Pass **`-OpenReport`** to open the generated HTML report when your environment supports it. CI uploads coverage artifacts from **[`.github/workflows/Project.yml`](.github/workflows/Project.yml)**.

---

## CI/CD and cloud

Pipeline overview and related docs: [CI/CD documentation](documentation/CICD/Wild Nature Explorer — CI CD Documentation 3523f32b1f8b8053850eef151720950d.md). Cloud deployment notes: [documentation/Cloud/](documentation/Cloud/).

---

## Docker image metrics

Scripts report image sizes and creation times (useful for coursework write-ups). Run from the repo root with Docker running.

**PowerShell (Windows)**

```powershell
.\docker\scripts\show-docker-metrics.ps1          # inspect existing images
.\docker\scripts\show-docker-metrics.ps1 -Build   # rebuild, then inspect
.\docker\scripts\show-docker-metrics.ps1 -Ps      # include docker compose ps
```

**Bash**

```bash
./docker/scripts/show-docker-metrics.sh
./docker/scripts/show-docker-metrics.sh --build
./docker/scripts/show-docker-metrics.sh --ps
```

`docker-compose.yml` sets **`restart: unless-stopped`** on services; the frontend container defines an HTTP **`/`** healthcheck.

---

## Misc

- **Android**: see **`android-alert-app`** (Gradle companion project).  
- Clone example: `git clone https://github.com/aldrig/WildNatureExplorer.git` — the folder name matches your GitHub repo name.
