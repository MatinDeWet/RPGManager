# Pre-commit Hooks

RPGManager uses [pre-commit](https://pre-commit.com) to run automated checks
before every commit. Its primary job here is **secret scanning** — stopping
credentials, API keys, and private keys from ever being committed.

The hook config lives in [`.pre-commit-config.yaml`](../.pre-commit-config.yaml).

## One-time setup (per clone)

`pre-commit` is a Python tool. Install it once on your machine, then install the
git hook in this repo:

```bash
# macOS (Homebrew)
brew install pre-commit
# ...or via pip
pip install pre-commit

# from the repo root — wires up .git/hooks/pre-commit
pre-commit install
```

After `pre-commit install`, the hooks run automatically on `git commit` against
the files you staged. The first run downloads and caches each hook's
environment, so it may take a moment.

> **Local hooks are opt-in and can be bypassed** (`--no-verify`, or simply not
> running `pre-commit install`). As a backstop, the CI pipeline runs a gitleaks
> scan server-side on every push and pull request — see the `secret-scan` job in
> [`.github/workflows/ci.yml`](../.github/workflows/ci.yml). It uses
> [`gitleaks/gitleaks-action`](https://github.com/gitleaks/gitleaks-action),
> which is free for **personal-account** repositories and needs no license key.
>
> ⚠️ **If this repo is ever transferred to a GitHub organization**, the action
> requires a (free) `GITLEAKS_LICENSE` — obtain one at
> [gitleaks.io](https://gitleaks.io) and add it as a repo/org secret named
> `GITLEAKS_LICENSE`, then pass it through the job's `env`. The local
> pre-commit `gitleaks` hook has no such requirement.

## What runs

| Hook | Source | Purpose |
|------|--------|---------|
| `gitleaks` | [gitleaks/gitleaks](https://github.com/gitleaks/gitleaks) | Scans staged changes for secrets (keys, tokens, credentials) and blocks the commit if any are found. |
| `detect-private-key` | pre-commit-hooks | Extra guard for PEM/SSH private keys. |
| `check-added-large-files` | pre-commit-hooks | Blocks accidentally committing large/binary blobs. |
| `check-merge-conflict` | pre-commit-hooks | Catches leftover merge-conflict markers. |

> These hooks are intentionally **non-mutating** — they report and block, they
> do not rewrite your source files. Code formatting is already enforced by
> `.editorconfig` plus the analyzers (`TreatWarningsAsErrors`), so pre-commit
> stays out of that lane.

## Useful commands

```bash
pre-commit run --all-files     # scan the whole repo, not just staged files
pre-commit run gitleaks        # run a single hook
pre-commit autoupdate          # bump hook versions in .pre-commit-config.yaml
```

## Handling findings

If a commit is blocked:

1. **Real secret** — remove it from the code, rotate the credential, and supply
   it at runtime instead (environment variables such as
   `ConnectionStrings__CoreDB`, or a secrets manager). See [SECURITY.md](../SECURITY.md).
2. **False positive** — prefer a narrowly-scoped fix. You can add an inline
   `# gitleaks:allow` comment on the offending line, or maintain a
   `.gitleaks.toml` allowlist. Avoid `--no-verify`, which skips *all* hooks.

## Bypassing (discouraged)

`git commit --no-verify` skips the hooks entirely. Only use it when you are
certain there is no secret involved — it defeats the purpose of the scan.
