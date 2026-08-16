#!/usr/bin/env bash
# Verify that repository-internal Markdown links resolve to real paths.
#
# External links are deliberately NOT requested. SET-20260816-002 recorded the
# sandbox blocking outbound requests, which made an external link check report
# false failures. This script is Bash rather than a PowerShell pipeline because
# SET-20260816-001 recorded a PowerShell pipeline parse failure.
set -uo pipefail

root=$(git rev-parse --show-toplevel) || exit 1
cd "$root" || exit 1

broken=0
checked=0

while IFS= read -r file; do
  dir=$(dirname "$file")
  while IFS= read -r target; do
    [ -z "$target" ] && continue
    case "$target" in
      http://*|https://*|mailto:*|\#*) continue ;;
    esac
    path=${target%%#*}
    [ -z "$path" ] && continue
    checked=$((checked + 1))
    if [ ! -e "$dir/$path" ]; then
      printf 'BROKEN %s -> %s\n' "$file" "$target"
      broken=$((broken + 1))
    fi
  done < <(awk '/^```/{fence = !fence; next} !fence' "$file" \
           | grep -oE '\]\([^)]+\)' | sed -E 's/^\]\(//; s/\)$//')
done < <(git ls-files '*.md')

printf '%d internal links checked, %d broken\n' "$checked" "$broken"
[ "$broken" -eq 0 ]
