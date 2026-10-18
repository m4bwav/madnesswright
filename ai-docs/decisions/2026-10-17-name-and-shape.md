# Name and shape of the plugin

Date: 2026-10-17. Status: decided (small rulings, flagged to the owner).

## Name: madnesswright

Fits the owner's "-wright" plugin family. "nexuswright" was rejected because Nexus Mods is a large modding site and the name would read as a Nexus Mods tool. "madness-agent" was rejected as vague.

## Three skills, not one

- `madnesswright`: the hub, for anything that is only MadCards, plus setup, deploy, log and sharing. Most requests land here.
- `madnesswright-scenes`: level editing is the user's second named goal and needs its own long rule set.
- `madnesswright-modtools`: assets and code share the ModTools and Unity setup and the "verify against the real game" guard.

## One script in the hub skill

`mpn.py` sits in `skills/madnesswright/scripts/`. The siblings call it by relative path. That avoids three copies and keeps the always-loaded cost to the three descriptions.

## Tier moderate (30 days), with one volatile claim at use

The game ships betas every few weeks, but the modding format has been stable since 2024. The releasebeta-only claim is the one most likely to flip, so it is a volatile claim re-checked every 14 days and at use.

## Budget

Scripts get their own budget row (green up to 9,000 tokens) because agents run `mpn.py` rather than read it. References stay at 3,500 green.

## No files from the official repositories

MPN_SceneTools and MPN_ModTools have no license file, so test fixtures are synthetic and nothing from them is committed.
