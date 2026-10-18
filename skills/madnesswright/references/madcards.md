# MadCards

Source: the official "MADNESS: Project Nexus Modding Overview" Google Doc (Part 1, Part 2d, Part 3), read 2026-10-17. The doc's own examples use curly quotes and an invalid `["r":0, ...]` colour syntax; the examples below are corrected and straight-quoted. Field names other than those shown here come from the MadCard Maker (MCM) glossary.

## What a card is

A MadCard is a set of values for one game entity: a character, weapon, armor piece, voice set, chatter colour and font, animation set, corpus block, explosion, damage-over-time effect, stage loadout, wave, stage, campaign, cutscene and more. The full list of Types and every vanilla Filename is in MCM.

- `Filename` (required): the entity's id. Taken by the game or another mod: your card overrides it. Unique: your card adds a new entity.
- `Type` (required): what the entity is (`Character`, `Melee`, `Loadout`, ...). In a standard `.json` file the array name gives the Type. Case-sensitive, except `Event`, `Mission`, `Cutscene` and `Driver`. The types the game's loader knows (build 25615280): Addon, AddonGroup, Ammo, AnimationProfile, Animations, Armor, Campaign, Character, Chatter, Corpus, DoT, Dungeon, Execution, Explosion, Gang, Keycard, Loadout, Melee, Origin, Ranged, Skill, SkillGroup, Skin, SoundPack, Stage_Arena, Stage_Custom, Stage_Dungeon, Stage_Story, Swatch, Thrown, VisualStockpile, VoiceSet, Wardrobe, plus Event, Mission, Cutscene, Driver. The loader also accepts `Stage_CAMPAIGN` in `.txt` and an `Item` list in `.json`; neither is in the official docs, so prefer the named stage types.
- `Root`: the Filename of another card of the same Type to inherit every value you do not set. Root and Filename need not match: Filename `Nexus1_Agent` with Root `Hank` turns every agent into Hank.
- `Priority`: lower converts first, for cards of the same Type that depend on each other.
- Values have no order inside a card.

## Two file formats

Standard JSON, saved as `.json`, strict (no trailing commas, no comments, straight quotes), always the container shape below. The game reads `.json` only as this container: a bare `{ "Type": ..., "Filename": ... }` card in a `.json` file, or a list name it does not know, is silently ignored.

```json
{
  "Character": [
    { "Filename": "Nexus1_Agent", "Root": "Nexus1_Agent", "Health": 400 },
    { "Filename": "Hank", "Root": "Hank" }
  ],
  "Melee": [
    { "Filename": "M_ModSword", "Root": "M_SwordLong", "ObjRef": "Machete_Sword",
      "Name": "The Mod Sword", "NameShort": "M-Sword", "Description": "A modded sword." }
  ]
}
```

Shorthand, saved as `.txt`: cards sit at the top level one after another, each with its own `Type`. The game splits the file on balanced braces, straightens curly double quotes, and parses each card with Unity's JsonUtility, which forgives some rule-breaking (the docs show trailing commas); keep to plain JSON objects anyway so other tools can read them. Every `.txt` and `.json` in the mod folder and its subfolders is read as cards, except `info.txt` and `info.json`, so keep notes as `.md`.

```
{ "Type": "Character", "Filename": "Nexus1_Agent", "Root": "Nexus1_Agent", "Health": 400 }
{ "Type": "Character", "Filename": "Hank", "Root": "Hank" }
```

Colours are Unity `Color` values, written as an object with floats from 0 to 1: `"Blood": {"r": 0, "g": 1, "b": 0, "a": 1}`. The docs' `["r":0, ...]` is not JSON. `Blood` is a `Color` field on the Character card (read from the game's code, build 25615280); other colour fields are named in MCM.

## Arrays: borrow, blank, empty

For lists of MadCards, MadObjects and enumerators only:

- `"*"` at an index keeps the Root's element there (only within the Root's array length).
- `null`, `"none"` or `""` at an index leaves it empty.
- A one-element array `["empty"]` empties the whole list regardless of the Root.
- A name the game cannot find falls back to the Root's element, as if `"*"` were there.

```json
{ "Character": [ { "Filename": "Abomination", "Root": "Abomination",
  "DefaultArmor": ["*", "My_Custom_Outfit", "*", null] } ] }
```

## Pointing at assets

- `ObjRef` names a MadObject prefab inside any enabled mod's `.mobj` bundle, by the prefab's name (not the bundle's name).
- `"ObjRef": "*M_Pipewrench"` borrows a built-in entity's object: the longsword then looks like the pipe wrench.
- Other fields take other asset kinds (for example `MenuIcon` takes an image); MCM says which.

## Stage, campaign and cutscene cards

These belong to madnesswright-scenes (its `references/stages-and-campaigns.md`).

## info.txt

One JSON object, required for a local mod, saved as `info.txt` or `info.json`. The game parses it with JsonUtility (curly quotes break it) and skips the mod unless `Name` and `Description` are both non-empty. Workshop items take their title and description from the Workshop, but still read `info.txt` for the assembly keys.

```json
{
  "Name": "Remastered Longsword",
  "Author": "Mystery Modder",
  "Description": "Swaps the default longsword for a new model.",
  "Tags": ["Weapon", "Remaster"],
  "Dependencies": [3536852525]
}
```

`Dependencies` are Workshop ids that auto-enable with this mod if the user has them installed. Harmony keys (`AssemblyPath`, `HarmonyID`, `EntryType`, `PatchOrder`, `ManualPatch`) are covered by madnesswright-modtools.
