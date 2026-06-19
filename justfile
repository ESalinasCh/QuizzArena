# QuizzArena Backend — task runner
# Requires: just, docker, docker compose, dotnet SDK 10
# Run `just` to see available recipes.

set dotenv-load := true

compose := "docker compose -f QuizzArena.Backend/docker-compose.yml"
backend := "QuizzArena.Backend"
sln     := "QuizzArena.Backend.slnx"

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

# ── Setup ──────────────────────────────────────────────────────────────────────

# Register git hooks — run once after cloning the repo
# Optionally also installs Husky.Net (dotnet husky) if dotnet is available.
install-hooks:
    git config core.hooksPath .husky
    chmod +x .husky/commit-msg .husky/pre-commit .husky/pre-push
    @echo "✔  git hooks registered (core.hooksPath = .husky)"
    dotnet tool restore \
        && dotnet husky install \
        && echo "✔  Husky.Net installed (husky.sh generated)" \
        || echo "⚠  dotnet not available — hooks work without it, Husky.Net integration skipped"

# ── Dev helpers ────────────────────────────────────────────────────────────────

# Open Swagger UI in the default browser
swagger:
    @echo "Opening http://localhost:8080/swagger"
    xdg-open http://localhost:8080/swagger 2>/dev/null || open http://localhost:8080/swagger 2>/dev/null || true

# Open RabbitMQ management console
rabbitmq:
    @echo "Opening http://localhost:15672  (guest / guest)"
    xdg-open http://localhost:15672 2>/dev/null || open http://localhost:15672 2>/dev/null || true

# ── Static analysis ────────────────────────────────────────────────────────────

# Verify formatting and code style — fails on any diff (mirrors CI check)
lint:
    cd {{ backend }} && dotnet format {{ sln }} \
        --severity warn \
        --verify-no-changes \
        --no-restore

# Auto-fix formatting and style issues in place
lint-fix:
    cd {{ backend }} && dotnet format {{ sln }} --severity warn --no-restore

# Full Roslyn static analysis: builds in Release treating ALL warnings as errors.
# Escalates beyond Directory.Build.props (which only errors on a subset) to catch
# every nullable, CA, and IDE code-style diagnostic in the solution.
analyze:
    cd {{ backend }} && dotnet restore {{ sln }}
    cd {{ backend }} && dotnet build {{ sln }} \
        -c Release \
        --no-restore \
        /p:TreatWarningsAsErrors=true \
        /p:EnforceCodeStyleInBuild=true

# ── Unit tests & coverage ──────────────────────────────────────────────────────

# Run unit tests and collect Cobertura coverage into coverage/raw/
test:
    cd {{ backend }} && dotnet test {{ sln }} \
        --verbosity normal \
        --collect:"XPlat Code Coverage" \
        --results-directory coverage/raw

# Generate an HTML + text summary from the last test run.
# Requires reportgenerator in the local tool manifest:
#   dotnet tool install dotnet-reportgenerator-globaltool
test-report:
    cd {{ backend }} && dotnet tool run reportgenerator \
        -- \
        -reports:"coverage/raw/**/coverage.cobertura.xml" \
        -targetdir:"coverage/report" \
        -reporttypes:"Html;TextSummary"
    @cat {{ backend }}/coverage/report/Summary.txt 2>/dev/null || true
    xdg-open {{ backend }}/coverage/report/index.html 2>/dev/null \
        || open {{ backend }}/coverage/report/index.html 2>/dev/null || true

# ── Secret scanning ────────────────────────────────────────────────────────────

# Scan the full git history for accidentally committed secrets (requires Docker)
scan-secrets:
    @echo "Scanning git history for secrets with gitleaks…"
    docker run --rm \
        -v "$(pwd):/repo" \
        zricethezav/gitleaks:latest detect \
            --source /repo \
            --verbose \
            --exit-code 1

# Scan only staged changes — run this before committing (pre-commit guard)
scan-secrets-staged:
    @echo "Scanning staged changes for secrets…"
    docker run --rm \
        -v "$(pwd):/repo" \
        zricethezav/gitleaks:latest protect \
            --source /repo \
            --staged \
            --verbose

# ── Security audit ─────────────────────────────────────────────────────────────

# Audit direct and transitive NuGet packages for known CVEs, and list outdated deps
audit:
    @echo "── Vulnerable packages ──────────────────────────────────────────────"
    cd {{ backend }} && dotnet list package --vulnerable --include-transitive
    @echo ""
    @echo "── Outdated packages ────────────────────────────────────────────────"
    cd {{ backend }} && dotnet list package --outdated

# ── All quality gates ──────────────────────────────────────────────────────────

# Run every quality gate in sequence: lint → analyze → test → scan-secrets → audit
check-all: lint analyze test scan-secrets audit
    @echo ""
    @echo "✅  All quality gates passed."
