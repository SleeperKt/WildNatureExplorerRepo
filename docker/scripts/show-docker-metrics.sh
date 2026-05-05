#!/usr/bin/env sh
# Prints compose-related image metrics (sizes, creation time). Optional build timing.
# Usage: ./docker/scripts/show-docker-metrics.sh [--build] [--ps]
set -eu

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"
COMPOSE="docker compose -f docker-compose.yml"

# IEC binary units (matches typical "MiB" reporting)
human_iec() {
  _b="$1"
  awk -v b="$_b" 'BEGIN {
    split("B KiB MiB GiB TiB", u, " ")
    v = b + 0
    i = 1
    while (v >= 1024 && i < 5) { v /= 1024; i++ }
    if (i == 1) printf "%d %s\n", b, u[i]
    else printf "%.2f %s\n", v, u[i]
  }'
}

BUILD=false
PS=false
for arg in "$@"; do
  case "$arg" in
    --build) BUILD=true ;;
    --ps) PS=true ;;
  esac
done

echo ""
echo "=== Wild Nature Explorer - Docker metrics ==="
echo "Repository: $ROOT"
echo ""

if [ "$BUILD" = true ]; then
  echo "--- docker compose build ---"
  START=$(date +%s)
  $COMPOSE build
  END=$(date +%s)
  echo ""
  echo "Build elapsed: $((END - START))s"
fi

echo ""
echo "--- Compose images (service / repository / tag / size) ---"
echo ""
$COMPOSE images || true

echo ""
echo "--- Image inspect: created, size, architecture ---"
echo ""

TAGS="$($COMPOSE config --images 2>/dev/null || true)"
if [ -z "$TAGS" ]; then
  echo "(Could not resolve images from compose config.)"
else
  echo "$TAGS" | while IFS= read -r tag || [ -n "$tag" ]; do
    [ -z "$tag" ] && continue
    if docker image inspect "$tag" >/dev/null 2>&1; then
      SIZE=$(docker image inspect "$tag" --format '{{.Size}}')
      docker image inspect "$tag" --format 'Image: '"$tag"'
ID: {{.Id}}
RepoTags: {{json .RepoTags}}
Created: {{.Created}}
Architecture: {{.Architecture}}
Os: {{.Os}}'
      echo "Size (bytes): $SIZE"
      echo "Size (human): $(human_iec "$SIZE")"
    else
      echo "(Not pulled/built yet: $tag)"
    fi
    echo "---"
  done
fi

echo ""
echo "--- Reference: postgis/postgis ---"
docker images postgis/postgis --format 'table {{.Repository}}\t{{.Tag}}\t{{.ID}}\t{{.CreatedSince}}\t{{.Size}}' 2>/dev/null || true

if [ "$PS" = true ]; then
  echo ""
  echo "--- docker compose ps ---"
  $COMPOSE ps
fi

echo ""
echo "Tip: runtime stats - docker stats --no-stream (stack running)."
echo ""
