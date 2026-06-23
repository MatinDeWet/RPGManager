---
name: commit-push
description: Commit the current work to a GitFlow feature branch and push it to origin for RPGManager, without opening a pull request. Use when asked to commit and/or push a change (e.g. "commit this", "commit and push", "push my work"). Handles the protected-branch guardrails (never commit to main/develop), branches off the up-to-date origin/develop when needed, writes a conventional-commit message with no Claude attribution, and pushes. For opening a PR as well, use open-pr instead.
---

# Commit & push (GitFlow)

RPGManager follows GitFlow (see `CONTRIBUTING.md`). `main` and `develop` are **protected — never commit to them directly**; `develop` is the default integration target. This skill commits and pushes only — it does **not** open a PR (use `open-pr` for that).

## Steps

1. **Check state**: `git status -sb` and note the current branch and what's staged/unstaged.
2. **Get onto a feature branch** (skip if already on a non-protected `feature/*`/`release/*`/`hotfix/*` branch):
   - `git fetch origin` — local `develop` is often behind `origin/develop`; always base on the remote.
   - If currently on `main`/`develop` (or work sits on a stale base): stash work (`git stash push -u`), `git switch -c feature/<slug> origin/develop`, then `git stash pop` and resolve any conflicts.
   - Branch naming: `feature/*` for new work; `release/*` (off develop) and `hotfix/*` (off main) merge into **both** main and develop.
3. **Commit**: stage (`git add -A`, or selectively) and write a **conventional commit** (`feat(scope): ...`, `fix(...)`, `refactor(...)`, `chore(...)`, `docs(...)`, `test(...)`). Subject in imperative mood; body explains the *why*. Pre-commit hooks (gitleaks secret scan, private-key/large-file/merge-conflict checks) run automatically and are non-mutating — if one fails, fix the cause, don't bypass.
4. **Push**: `git push -u origin feature/<slug>` (or a plain `git push` if upstream is already set).

## Honor the user's commit preferences

- **No `Co-Authored-By` trailer** on commits.
- **No "Generated with Claude" / Claude attribution** anywhere in the commit message.

## Notes

- Only commit/push when the user asks.
- Match the existing commit style on the branch (`git log --oneline -10`) — scope names, tense, granularity.
- When the work is several logical changes, prefer multiple focused commits over one mixed commit.
- Before pushing, verify the diff is what you intend and isn't based on a stale/protected base: `git diff origin/develop...HEAD --stat`.
- If the branch already exists on origin and history was rewritten, push with `--force-with-lease` (never a bare `--force`).
- If a PR already exists for this branch, pushing updates it automatically — no further action needed here.
