# WildNatureExplorer

API for exploring wildlife and interacting with AI services.

---

## Features

- User registration and login (JWT authentication)
- AI image analysis and question answering
- User profile management
- PostgreSQL database support
- Dockerized for easy deployment

---

## Quick Start

```bash
git clone https://github.com/aldrig/WildNatureExplorer.git
cd WildNatureExplorer
cp .env.example .env
docker-compose up --build


API will be available at: http://localhost:5000/api
Swagger UI: http://localhost:5000/swagger

---

## Docker metrics (image size, created date, optional build time)

Reports useful values for coursework write-ups (sizes as bytes plus human-readable IEC units, creation timestamps). Run from the repository root after Docker Desktop / Engine is running.

**PowerShell (Windows)**

```powershell
.\docker\scripts\show-docker-metrics.ps1          # inspect existing images
.\docker\scripts\show-docker-metrics.ps1 -Build   # rebuild then inspect
.\docker\scripts\show-docker-metrics.ps1 -Ps       # include docker compose ps (stack should be up)
```

**Bash**

```bash
./docker/scripts/show-docker-metrics.sh            # inspect existing images
./docker/scripts/show-docker-metrics.sh --build    # rebuild then inspect
./docker/scripts/show-docker-metrics.sh --ps       # include docker compose ps
```

Cold start / runtime: use `Measure-Command { docker compose up -d }` (PowerShell) or `time docker compose up -d`, then `docker stats --no-stream` while containers run.

Compose uses **`restart: unless-stopped`** on `db`, `api`, and `frontend`. The SPA container has a **healthcheck** on HTTP `/`.