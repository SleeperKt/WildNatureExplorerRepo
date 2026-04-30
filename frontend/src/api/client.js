import axios from "axios";

// Empty baseURL + paths like `/api/...`:
// - Local Vite dev: same-origin `/api` is proxied to the API (vite.config.js).
// - Docker Compose: nginx swaps in nginx-frontend.compose.conf and proxies `/api` to the API container.
// - Azure production: build with VITE_API_URL = public HTTPS origin of the API App Service (no trailing slash).
//
// Optional `VITE_DEV_TUNNEL_API_URL`: temporary tunnel/ngrok-style base URL for manual debugging only — do not set in prod CI images.
const baseURL =
  (import.meta.env.VITE_API_URL || "").trim() ||
  (import.meta.env.VITE_DEV_TUNNEL_API_URL || "").trim() ||
  "";

export const api = axios.create({
  baseURL,
});

let globalErrorHandler = null;

export const setGlobalErrorHandler = (handler) => {
  globalErrorHandler = handler;
};

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (!error.response && globalErrorHandler) {
      globalErrorHandler(
        "Cannot connect to server. Please check your internet connection and try again."
      );
    }
    throw error;
  }
);
