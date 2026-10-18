# Security policy

## Reporting a problem

Open an issue or a pull request and I'll take a look. A pull request with the fix is the quickest route.

You can also report privately: open the repository's **Security** tab and choose **Report a vulnerability**.

This is a one-person project with no bug bounty and no response deadline. I'll reply when I can and say what I plan to do. Good-faith testing on your own copy is welcome.

## Supported versions

Only the latest release on the default branch gets fixes.

## Scope

The skills' instructions and the `mpn.py` helper run on your machine with your agent's permissions. In scope: anything that leads the agent to run commands, write files outside a backup zip and the game's Mods folder, fetch URLs, or enable code from a mod beyond what the skill says it does. That includes instructions planted in a mod, a MadCard, a scene file or a web page the skill reads (prompt injection). Mods with Harmony assemblies run code in the game; the skills say so before one is enabled. Out of scope: bugs in the agent tool itself, in MADNESS: Project Nexus, its SceneTools or ModTools projects, Unity or Steam; report those to their makers.
