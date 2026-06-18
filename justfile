# QuizzArena Backend — task runner
# Requires: just, docker, docker compose
# Run `just` to see available recipes.

set dotenv-load := true

compose := "docker compose -f QuizzArena.Backend/docker-compose.yml"

# List available recipes (default)
default:
    @just --list

# ── Docker env ─────────────────────────────────────────────────────────────────

# Start core stack (Postgres, RabbitMQ, Azurite, Ollama) + backend
up:
    {{ compose }} up -d --build

# Start core stack + Whisper (small model, 16GB+ RAM)
up-high:
    {{ compose }} --profile high up -d --build

# Start core stack + Whisper (tiny model, 8GB RAM)
up-low:
    {{ compose }} --profile low up -d --build

# Start only backing services (no backend rebuild)
up-services:
    {{ compose }} up -d postgres rabbitmq azurite ollama

# Stop all containers (keep volumes)
down:
    {{ compose }} down

# Stop all containers and remove volumes (clean slate)
down-clean:
    {{ compose }} down -v

# Show live logs for all services (Ctrl-C to exit)
logs:
    {{ compose }} logs -f

# Show live logs for the backend only
logs-backend:
    {{ compose }} logs -f backend

# Show running container status
ps:
    {{ compose }} ps

# Rebuild the backend image without starting
build:
    {{ compose }} build backend

# Restart only the backend container (re-reads env, no rebuild)
restart:
    {{ compose }} restart backend

# ── Dev helpers ────────────────────────────────────────────────────────────────

# Open Swagger UI in the default browser
swagger:
    @echo "Opening http://localhost:8080/swagger"
    xdg-open http://localhost:8080/swagger 2>/dev/null || open http://localhost:8080/swagger 2>/dev/null || true

# Open RabbitMQ management console
rabbitmq:
    @echo "Opening http://localhost:15672  (guest / guest)"
    xdg-open http://localhost:15672 2>/dev/null || open http://localhost:15672 2>/dev/null || true
