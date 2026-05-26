# Wild Nature Explorer

Платформа для изучения дикой природы: поиск видов, отметки на карте, загрузка находок и модерация. Включает интеграцию с ИИ для распознавания животных по фото и ответов на вопросы.

## Архитектура проекта

- `code/WildNatureExplorer.API/` — Backend (ASP.NET Core 8 REST API, PostgreSQL + PostGIS, Entity Framework Core).
- `frontend/` — Frontend (React 19, Vite, Leaflet для карт).
- `android-alert-app/` — Сопутствующее Android-приложение.
- `documentation/` — Документация (API, Docker, CI/CD).
- `WildNatureExplorer.Tests/` — Unit / Integration тесты.

## Запуск через Docker Compose (Рекомендуется)

Требования: Docker Desktop или Docker Engine + Compose v2.

1. Создайте `.env` файл из шаблона:
   ```bash
   cp .env.example .env
   ```
2. Обязательно укажите в `.env` ключ `GROQ_API_KEY`, а также настройте `DB_*` и `JWT_*` ключи.
3. Запустите контейнеры:
   ```bash
   docker compose up --build
   ```

**Эндпоинты:**
- **Web UI**: `http://localhost:5174` (по умолчанию)
- **API**: `http://localhost:5000`
- **Swagger**: `http://localhost:5000/swagger`

## Локальный запуск (Без Docker)

1. Поднимите и настройте базу **PostgreSQL 15 + PostGIS**.
2. Задайте те же переменные окружения, что и в `.env` (особенно `DB_HOST=localhost`).
3. Запуск Backend:
   ```bash
   cd code/WildNatureExplorer.API
   dotnet run
   ```
4. Запуск Frontend (запросы к `/api` будут проксироваться на `localhost:5000`):
   ```bash
   cd frontend
   npm ci
   npm run dev
   ```

## Тестирование
```bash
dotnet test WildNatureExplorer.Tests/WildNatureExplorer.Tests.csproj
```
Скрипты получения покрытия: `scripts/run-coverage.ps1` или `scripts/run-coverage-integration.ps1`.
