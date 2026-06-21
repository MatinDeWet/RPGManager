# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in RPGManager, please report it
privately rather than opening a public issue.

- Use GitHub's [private vulnerability reporting](https://github.com/MatinDeWet/RPGManager/security/advisories/new)
  ("Report a vulnerability" under the **Security** tab), or
- Contact the maintainer directly.

Please include steps to reproduce, affected components, and any relevant logs.
We aim to acknowledge reports within a few days.

## Supported Versions

This project is under active development; security fixes are applied to the
`main` branch.

## Notes

- The PostgreSQL credentials in `appsettings.json` and `compose.yaml` are
  **local-development defaults only** and must never be reused in any shared or
  production environment. Supply real secrets via environment variables
  (`ConnectionStrings__CoreDB`) or a secrets manager.
- Commits are scanned for secrets by a [pre-commit](https://pre-commit.com)
  `gitleaks` hook. Install it with `pre-commit install` — see
  [Docs/PreCommit.md](Docs/PreCommit.md). This is a safety net, not a substitute
  for keeping real secrets out of the repo in the first place.
