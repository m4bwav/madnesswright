# Agent rules for madnesswright

Rules for any AI agent (Claude Code, Codex, Cursor, Copilot, Gemini CLI) working in this repository. `CLAUDE.md` imports this file and `.github/copilot-instructions.md` points here.

Read [README.md](README.md) first. The three skills live under [skills/](skills/). Each is an evergreen unit with its companions beside it: RESEARCH, CHANGELOG, LEARNINGS, TESTS, `evals/evals.json`, MAINTENANCE and `evergreen.json`.

## Rules

- Evergreen units in pointer mode: before editing a skill, read its `evergreen.json`. If `next_due` has passed or `contradiction` is set, say so and refresh after the task (the evergreen plugin's `evergreen-refresh`, otherwise the skill's `MAINTENANCE.md`). Every edit to a SKILL.md gets a CHANGELOG entry with its reason (`R-`, `L-` or `T-` ids, or "user request"). Re-run that skill's `evals/evals.json` (evergreen-test) when the change alters what the skill triggers on, decides or does.
- Research beats recall. The game ships betas often, SceneTools and ModTools change every few weeks, and the official docs are Google Docs that are edited in place. Never change a branch name, version, path, menu item, card field or rule from memory: cite the source in RESEARCH.md (an `R-` entry) and log the change in CHANGELOG.md (a `C-` entry). Primary sources: the Steam News API for app 488860, the official Google Docs (fetch with `/export?format=txt`), github.com/MPN-Modders.
- `skills/madnesswright/scripts/mpn.py` is stdlib Python 3.9+, prints exactly one JSON object per command and fails soft. Keep it true on Windows, macOS and Linux. It may write only to a backup zip, the game's `Mods/` folder (deploy) and nowhere else.
- Never copy files from the official SceneTools or ModTools repositories into this one: they carry no license. Test fixtures are synthetic (`tests/test_mpn.py` builds them).
- Skills stay under 200 lines, with a description of at most 1,024 characters (Agent Skills spec) that names the sibling owning nearby requests.
- The skills install nothing on their own: Unity, the tool projects, decompilers, MCP servers and Workshop uploaders are the user's to approve. The agent never handles Steam credentials.
- Every Steam Workshop update gets real change notes: the version and what changed for the player since the last update (features, fixes, anything they must do), drawn from the CHANGELOG. Never "Update" or blank. `examples/WorkshopUpload` takes them as a file and refuses an update without one; an update made another way gets its notes filled in on the item's Change Notes tab (Edit). The owner asked for this on 2026-10-25.
- Before committing, run `python tests/check_skills.py`, `python tests/test_mpn.py`, `python tests/test_decomp.py` and `python scripts/budget.py` from the repository root. CI runs all four.
- This is a public repository. Nothing in it names a person other than the author credit, a machine, an absolute path on someone's machine, a private project or a credential. Whoever asked for a mod is "the requester" or "the player", never a family word (`tests/check_skills.py` fails on them).
- Versions: when a packaged file changes, bump `.claude-plugin/plugin.json` and add a line to the root CHANGELOG.md; tag `vX.Y.Z`.
- No AI attribution anywhere: no Co-Authored-By trailers and no "generated with" lines in commits, pull requests or files.

## everlast (session knowledge, load on demand)

- `ai-docs/INDEX.md` lists what past sessions learned here. At the start of a task, scan it and open only the entries that match; read `ai-docs/HANDOFF.md` when continuing unfinished work.
- Before finishing a task that hit a dead end, verified a non-obvious command or made a design choice, record it in `ai-docs/` and rewrite `HANDOFF.md` when work is left unfinished.
- Link documents with relative markdown links. No wikilinks in the repository.
