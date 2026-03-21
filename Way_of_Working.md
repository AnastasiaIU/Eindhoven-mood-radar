# Group Project Git Workflow

## Why we work this way

We have **one shared fork** where everyone is a collaborator. Instead of committing directly to shared branches, everyone works on their own **feature branch** and submits it via a **Pull Request (PR)**. This means:

- Your work stays under **your name** in the history (we use rebase merging)
- No one accidentally breaks the shared codebase
- Everyone gets a chance to **review each other's work**
- The history stays **clean and linear** — easy to read and debug

---

## Branch structure

```
Fontys's repo: main
                        ↑  (weekly PR for submission)
our fork:       main
                        ↑  (weekly PR after testing)
our fork:       dev     ←  everyone merges their work here daily/when ready
                        ↑
                MOON-123-short-description   ← your personal working branch (linked to Jira)
```

---

## One-time setup

```bash
# 1. Clone the group fork (NOT the Fontys's repo)
git clone <our-fork-url>
cd <repo-name>

# 2. Add the Fontys's repo as 'upstream' so you can sync later
git remote add upstream <fontys-repo-url>

# 3. Verify your remotes — you should see origin (our fork) and upstream (fontys)
git remote -v
```

---

## Step-by-step: working on a Jira task

### 1. Always start from a fresh dev

Before creating a new branch, make sure your local `dev` is up to date:

```bash
git checkout dev
git pull origin dev
```

### 2. Create your branch from Jira

The easiest way to create a properly linked branch is directly from the Jira ticket:

1. Open your assigned ticket in Jira (e.g., `MOON-123`)
2. In the right-hand panel, find **Development** and click **Create branch**
3. Jira will open GitHub and pre-fill the branch name (e.g., `MOON-123-login-page`)
4. Set the **source branch** to `dev` — this is important!
5. Click **Create branch**

Then pull it down locally:

```bash
git fetch origin
git checkout MOON-123-login-page
```

> **Why branch from Jira?** Jira automatically links the branch, commits, and PRs to the ticket. Your team can see the status of work directly on the board without anyone manually updating it.

### 3. Work and commit regularly

```bash
git add .
git commit -m "MOON-123: Short description of what you did"
```

Including the ticket number in your commit message keeps everything linked in Jira. Commit often — small, focused commits are better than one giant commit.

### 4. Keep your branch up to date with dev

If others have merged work into `dev` while you were working, sync it into your branch to avoid conflicts later:

```bash
git fetch origin
git rebase origin/dev
```

> If you hit conflicts during rebase, resolve them file by file, then run `git rebase --continue`.

### 5. Push your branch

```bash
git push origin MOON-123-login-page
```

If you rebased after already pushing, you may need:

```bash
git push origin MOON-123-login-page --force-with-lease
```

> `--force-with-lease` is safer than `--force` — it won't overwrite someone else's changes.

### 6. Open a Pull Request into dev

- Go to our fork on GitHub
- Open a PR from `MOON-123-login-page` → `dev`
- Write a short description of what your PR does
- Assign a teammate to review it

Jira will automatically update the ticket to show the open PR.

### 7. Review and merge

- At least **one other person** should review and approve the PR
- The author (or reviewer) merges using **Rebase and Merge** — this keeps your commits linear and under your name
- Jira will automatically mark the development item as merged

---

## Weekly: merging dev → main → Fontys

1. Everyone finishes and merges open feature PRs into `dev`
2. Someone opens a PR from `dev` → `main` in our fork
3. Everyone does a quick smoke test on `main`
4. If everything works, open a PR from our `main` → Fontys's repo

---

## Quick reference cheatsheet

| What | Command |
|---|---|
| Update local dev | `git checkout dev && git pull origin dev` |
| Fetch Jira-created branch | `git fetch origin && git checkout MOON-123-name` |
| Commit with ticket reference | `git commit -m "MOON-123: message"` |
| Sync with latest dev | `git fetch origin && git rebase origin/dev` |
| Push branch | `git push origin MOON-123-name` |
| Force push after rebase | `git push origin MOON-123-name --force-with-lease` |

---

## Golden rules

- ❌ Never commit directly to `dev` or `main`
- ❌ Never merge your own PR without a review
- ❌ Never create a branch from `main` — always branch from `dev`
- ✅ Always create your branch from Jira, with `dev` as the source
- ✅ Always include the Jira ticket number in your commit messages
- ✅ Keep commits small and descriptive
- ✅ Communicate in the PR if something is blocked or unclear
