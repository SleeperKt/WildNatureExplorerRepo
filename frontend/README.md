# Wild Nature Explorer – Frontend

SPA веб-платформы для изучения дикой природы.

## Стек технологий
- **Рендеринг и сборка**: React 19, Vite 7
- **Маршрутизация**: React Router v7
- **Карты**: Leaflet + react-leaflet
- **Сетевые запросы**: axios
- **Анимации**: GSAP
- **Мобильная обертка**: Capacitor (Поддержка Android)
- **Линтинг и форматирование**: ESLint + Prettier

## Запуск для разработки

Требуется Node.js (совместимый с Vite 7).

```bash
# Установка зависимостей
npm ci

# Запуск dev-сервера (локально на http://localhost:5173)
npm run dev
```

В режиме разработки все запросы по пути `/api` проксируются на бекенд. 
По умолчанию используется `http://localhost:5000`. Изменить можно через переменную окружения `.env`:
```env
VITE_DEV_PROXY_TARGET=http://api.myproject.local:5000
```

## Доступные скрипты

- `npm run dev` — Запуск dev-сервера с HMR.
- `npm run build` — Сборка продакшен-версии (в папку `dist/`).
- `npm run preview` — Локальный превью-сервер собранной production-версии.
- `npm run lint` — Проверка кода линтером.
- `npm run format` — Автоформатирование кода с помощью Prettier.
