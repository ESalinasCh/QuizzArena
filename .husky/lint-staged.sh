#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────────────
# Staged-only whitespace format check (pre-commit).
#
# Runs `dotnet format whitespace` scoped to ONLY the project(s) that own the
# staged C# files, instead of the whole solution. Loading the full .slnx
# workspace stalls on slow filesystems (notably WSL /mnt/c); loading a single
# project completes in seconds. CI and the pre-push gate still run the full
# solution-wide check.
#
# Invoked as `bash .husky/lint-staged.sh` (via `just lint-staged`) so it does
# not depend on the file's execute bit, which cannot be set on /mnt/c mounts.
# ──────────────────────────────────────────────────────────────────────────────
set -o pipefail

# Staged C# files (Added/Copied/Modified/Renamed), repo-root-relative.
files=$(git diff --cached --name-only --diff-filter=ACMR -- '*.cs')
if [ -z "$files" ]; then
    echo "No staged C# files — whitespace check skipped."
    exit 0
fi

# Group each staged file under the nearest ancestor directory holding a .csproj.
declare -A proj_files
while IFS= read -r f; do
    [ -z "$f" ] && continue
    dir=$(dirname "$f")
    proj=""
    while [ "$dir" != "." ] && [ "$dir" != "/" ]; do
        cand=$(find "$dir" -maxdepth 1 -name '*.csproj' 2>/dev/null | head -n1)
        if [ -n "$cand" ]; then proj="$cand"; break; fi
        dir=$(dirname "$dir")
    done
    if [ -z "$proj" ]; then
        echo "⚠  no owning .csproj found for $f — skipping"
        continue
    fi
    proj_files["$proj"]+="$f "
done <<< "$files"

if [ ${#proj_files[@]} -eq 0 ]; then
    echo "No staged files map to a project — skipped."
    exit 0
fi

rc=0
for proj in "${!proj_files[@]}"; do
    echo "▶ whitespace check: $proj"
    echo "    ${proj_files[$proj]}"
    # shellcheck disable=SC2086  # intentional word-splitting of the file list
    dotnet format whitespace "$proj" \
        --include ${proj_files[$proj]} \
        --verify-no-changes \
        --no-restore
    s=$?
    [ $s -ne 0 ] && rc=$s
done

[ $rc -eq 0 ] && echo "✔  staged C# files are whitespace-clean"
exit $rc
