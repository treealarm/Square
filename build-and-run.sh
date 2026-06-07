#!/usr/bin/env bash
set -euo pipefail

export DOCKER_BUILDKIT=1

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Root directory: $ROOT_DIR ==="

echo "=== Building Square via docker-compose ==="
docker compose -f "$ROOT_DIR/docker-compose.yml" build

echo "=== Starting Square containers ==="
docker compose -f "$ROOT_DIR/docker-compose.yml" up
