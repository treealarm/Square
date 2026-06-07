#!/usr/bin/env bash
set -euo pipefail

# ==============================
# CONFIG
# ==============================

NAMESPACE="treealarm"
REGISTRY="docker.io"
BASE_PATH="$NAMESPACE"
DOCKER_USER="treealarm"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOKEN_FILE="$ROOT_DIR/.docker-token"

if [[ ! -f "$TOKEN_FILE" ]]; then
    echo "Token file not found: $TOKEN_FILE" >&2
    echo "Create it with your Docker Hub access token (it's gitignored, won't be committed)." >&2
    exit 1
fi

TOKEN="$(<"$TOKEN_FILE")"

echo "Token length: ${#TOKEN}"

# ==============================
# LOGIN
# ==============================
echo "$TOKEN" | docker login "$REGISTRY" -u "$DOCKER_USER" --password-stdin

# ==============================
# IMAGES  local → remote
# ==============================
declare -a PAIRS=(
    "leafletalarms:latest      $BASE_PATH/leafletalarms:latest"
    "integrationhost:latest    $BASE_PATH/integrationhost:latest"
    "aasubservice:latest       $BASE_PATH/aasubservice:latest"
    "grpctracksclient:latest   $BASE_PATH/grpctracksclient:latest"
    "blinkservice:latest       $BASE_PATH/blinkservice:latest"
)

# ==============================
# TAG + PUSH
# ==============================
for pair in "${PAIRS[@]}"; do
    local_img=$(echo "$pair" | awk '{print $1}')
    remote_img=$(echo "$pair" | awk '{print $2}')

    echo "=== Processing $local_img ==="
    docker tag "$local_img" "$remote_img"
    docker push "$remote_img"
done

echo "=== Done ==="
