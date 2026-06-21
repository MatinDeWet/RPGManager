---
name: open-pr
description: Commit the current work to a GitFlow feature branch and open a pull request into develop for RPGManager. Use when asked to commit/push/PR a change. Handles the protected-branch guardrails (never commit to main/develop), branches off the up-to-date origin/develop, writes a conventional-commit message, pushes, and creates the PR with gh.
---

# Open a PR (GitFlow)

RPGManager follows GitFlow (see `CONTRIBUTING.md`). `main` and `develop` are **protected — never commit to them directly**; `develop` is the default integration target and PR base. The `build` CI check must pass before merge.

## Steps

1. **Check state**: `git status -sb` and note the current branch.
2. **Fetch**: `git fetch origin` — local `develop` is often behind `origin/develop`; always base on the remote.
3. **Get onto a feature branch off `origin/develop`:**
   - If currently on `main`/`develop` (or work is uncommitted on a stale base): stash untracked + tracked work (`git stash push -u`), `git switch -c feature/<slug> origin/develop`, then `git stash pop`.
   - Branch naming: `feature/*` for new work; `release/*` (off develop) and `hotfix/*` (off main) merge into **both** main and develop.
   - Resolve any pop conflicts before continuing.
4. **Commit**: stage (`git add -A`) and write a **conventional commit** (`feat(scope): ...`, `fix(...)`, `refactor(...)`, `chore(...)`). Subject in imperative mood; body explains the *why*. Pre-commit hooks (gitleaks secret scan, private-key/large-file/merge-conflict checks) run automatically and are non-mutating — if one fails, fix the cause, don't bypass.
5. **Push**: `git push -u origin feature/<slug>`.
6. **PR**: `gh pr create --base develop --head feature/<slug> --title "..." --body "..."`. Body: Summary + bullets of what changed, notable decisions, and a Verification section (build/tests/manual).

## Honor the user's commit/PR preferences

- **No `Co-Authored-By` trailer** on commits.
- **No "Generated with Claude" / Claude attribution** in commit messages or PR bodies.

## Notes

- Only commit/push when the user asks.
- If a PR already exists for the branch, just commit + push to update it (don't open a second); confirm with `gh pr view`.
- Watch for work accidentally based on a stale or protected branch — verify the diff is what you intend (`git diff origin/develop...HEAD --stat`) before pushing.
- Don't include changes already merged upstream; if you rebased onto `origin/develop`, drop edits that duplicate commits already there.
