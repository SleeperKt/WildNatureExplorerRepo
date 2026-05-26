# Wild Nature Explorer – Frontend

This directory contains the Single Page Application (SPA) for the Wild Nature Explorer platform. It provides an interactive UI for wildlife exploration, species cataloging, map tracking, and AI-driven features.

## Tech Stack

- **Framework:** React 19
- **Build Tool:** Vite 7
- **Routing:** React Router v7
- **Maps & Geolocation:** Leaflet + React-Leaflet
- **State/Data Fetching:** Axios
- **Animations:** GSAP
- **Mobile Integration:** Capacitor (for Android app generation)
- **Code Quality:** ESLint & Prettier

## Getting Started

### Prerequisites

- **Node.js**: Version 20 or LTS compatible with Vite 7.
- **Backend API**: The backend should be running locally (usually on port 5000) for full functionality.

### Installation

```bash
npm ci
```

### Local Development

To run the development server with Hot Module Replacement (HMR):

```bash
npm run dev
```

The server will start at `http://localhost:5173`. By default, Vite is configured to proxy `/api` requests to `http://localhost:5000`.

### Environment Variables

To override the default API proxy target, create a `.env` file based on `.env.example` (if present) or add:

```env
VITE_DEV_PROXY_TARGET=http://your-custom-backend-url:5000
```

For production Docker builds, the base URL is provided via the `WNE_API_PUBLIC_BASE_URL` build argument.

## Available Scripts

- `npm run dev`: Starts the local development server.
- `npm run build`: Builds the app for production to the `dist/` folder.
- `npm run preview`: Bootstraps a local static web server to preview the production build.
- `npm run lint`: Analyzes code for potential errors using ESLint.
- `npm run format`: Formats code automatically using Prettier.
- `npm run format:check`: Checks if the code matches Prettier formatting rules without making changes.

## Mobile Support (Capacitor)

This frontend is integrated with Capacitor to build the Android application located in `../android-alert-app`. Capacitor bundles the output from `dist/` into native mobile views.
