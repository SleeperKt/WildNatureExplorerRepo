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