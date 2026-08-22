#!/usr/bin/env bash
# Verify that repository-internal Markdown links resolve to real paths.
#
# External links are deliberately NOT requested. SET-20260816-002 recorded the
# sandbox blocking outbound requests, which made an external link check report
# false failures. This script is Bash rather than a PowerShell pipeline because
# SET-20260816-001 recorded a PowerShell pipeline parse failure.
#
# The file list includes untracked-but-not-ignored files (--others
# --exclude-standard), not only tracked ones. SET-20260821-007 recorded this
# script reporting "0 broken" over documents it had never opened, because a
# newly created file is untracked until it is staged. The summary prints the
# file count for the same reason: a green result must state its denominator, or
# a run that checked nothing is indistinguishable from a run that checked
# everything.
set -uo pipefail

root=$(git rev-parse --show-toplevel) || exit 1
cd "$root" || exit 1

broken=0
checked=0
files=0

while IFS= read -r file; do
  files=$((files + 1))
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
done < <(git ls-files --cached --others --exclude-standard '*.md' | sort -u)

printf '%d files, %d internal links checked, %d broken\n' "$files" "$checked" "$broken"
[ "$broken" -eq 0 ]
