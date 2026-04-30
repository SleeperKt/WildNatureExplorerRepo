# syntax=docker/dockerfile:1
# Multi-stage: Vite build + nginx static hosting.
# For Azure, pass WNE_API_PUBLIC_BASE_URL at build time so the SPA calls the public API origin.
FROM node:20-alpine AS build
WORKDIR /app/frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
ARG WNE_API_PUBLIC_BASE_URL=
ENV VITE_API_URL=$WNE_API_PUBLIC_BASE_URL
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY docker/nginx-frontend.default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/frontend/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
