# Contributing

RPGManager uses a **GitFlow** branching model. The two long-lived branches are
protected — **never commit directly to them**; all changes land via pull request.

## Long-lived branches

| Branch    | Purpose                                              | Receives merges from |
|-----------|------------------------------------------------------|----------------------|
| `main`    | Production. Tagged releases only.                    | `release/*`, `hotfix/*` |
| `develop` | Integration branch. Default branch; everyday target. | `feature/*`, `release/*`, `hotfix/*` |

## Working branches

| Prefix      | Branch off | Merge back into        | Use for                                |
|-------------|------------|------------------------|----------------------------------------|
| `feature/*` | `develop`  | `develop`              | New features and non-urgent fixes      |
| `release/*` | `develop`  | `main` **and** `develop` | Stabilising a release (version bump, final fixes) |
| `hotfix/*`  | `main`     | `main` **and** `develop` | Urgent production fixes                 |

Name branches descriptively, e.g. `feature/card-search`, `hotfix/null-user-id`.

## Everyday workflow (features)

```bash
git checkout develop
git pull
git checkout -b feature/my-change      # branch off develop
# ... commit work ...
git push -u origin feature/my-change
gh pr create --base develop            # PR into develop
```

CI (`build`) must pass before a PR can merge. Branch protection requires a PR
but **0 approvals**, so you may self-merge once the check is green. Merged
branches are deleted automatically.

## Releases

```bash
git checkout -b release/x.y.z develop  # branch off develop
# bump version, final fixes, then:
gh pr create --base main               # merge release -> main
# tag main after merge:
git tag -a vx.y.z -m "x.y.z" && git push origin vx.y.z
# then merge the same changes back into develop
gh pr create --base develop --head release/x.y.z
```

## Hotfixes

```bash
git checkout -b hotfix/x.y.z main      # branch off main
# fix, then PR into main, tag, and also PR back into develop
```

## Pre-commit hooks

Before your first commit, install [pre-commit](https://pre-commit.com) and wire
up the git hook. This scans every commit for accidentally-included secrets:

```bash
brew install pre-commit   # or: pip install pre-commit
pre-commit install        # run once per clone, from the repo root
```

See [Docs/PreCommit.md](Docs/PreCommit.md) for the full hook reference and how
to handle findings.

## Build & quality gates

Warnings are treated as errors (`Directory.Build.props`), so the CI build fails
on any analyzer or code-style violation. Run `dotnet build` locally before
pushing. See [CLAUDE.md](CLAUDE.md) for architecture and build details.
