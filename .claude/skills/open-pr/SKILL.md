---
name: open-pr
description: Commit and push the current work to a GitFlow feature branch and open a pull request into develop for RPGManager. Use when asked to open/raise/create a PR (or "commit, push and PR this"). Handles the protected-branch guardrails (never commit to main/develop), branches off the up-to-date origin/develop, writes a conventional-commit message with no Claude attribution, pushes, and creates the PR with gh. To commit and push without opening a PR, use commit-push instead.
---

# Open a PR (GitFlow)

RPGManager follows GitFlow (see `CONTRIBUTING.md`). `main` and `develop` are **protected — never commit to them directly**; `develop` is the default integration target and PR base. The `build` CI check must pass before merge.

## Steps

1. **Commit & push** the work first. Follow the `commit-push` skill for the mechanics — check state, branch off the up-to-date `origin/develop` if on a protected/stale base, write a conventional commit (no `Co-Authored-By`, no Claude attribution), and push `feature/<slug>` to origin.
2. **PR**: `gh pr create --base develop --head feature/<slug> --title "..." --body "..."`.
   - Title: conventional-commit style, summarizing the whole branch.
   - Body: **Summary** + bullets of what changed, **Notable decisions**, and a **Verification** section (build/tests/manual). Use `gh pr create --body-file` (or a heredoc) for multi-paragraph bodies.

## Honor the user's PR preferences

- **No `Co-Authored-By` trailer** on commits.
- **No "Generated with Claude" / Claude attribution** in commit messages or the PR body.

## Notes

- Only commit/push/PR when the user asks.
- If a PR already exists for the branch, just commit + push to update it (don't open a second); confirm with `gh pr view`.
- Watch for work accidentally based on a stale or protected branch — verify the diff is what you intend (`git diff origin/develop...HEAD --stat`) before pushing.
- Don't include changes already merged upstream; if you rebased onto `origin/develop`, drop edits that duplicate commits already there.
- After creating, report the PR URL (`gh pr view --json url`).
