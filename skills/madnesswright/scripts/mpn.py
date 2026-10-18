#!/usr/bin/env python3
"""mpn.py - helper for modding MADNESS: Project Nexus (Steam app 488860).

Standard library only, Python 3.9+, Windows, macOS and Linux (Steam Deck via Proton).
Every command prints exactly one JSON object and exits 0 unless --strict is given
(before the command name) and the result has errors.

Commands
  where                          install, branch (BetaKey), build id, data, Mods and save folders, tool build outputs
  backup [--out DIR]             zip the game's data folder (saves, Mods, settings) before a branch switch or mod removal
  check-mod <mod-dir>            lint a mod folder: info.txt, MadCards (.json strict, .txt shorthand), bundles, assembly
  deploy <mod-dir> [--force]     check, then copy the mod folder into the game's Mods folder
  log [--lines N]                game version, enabled list, problem lines and tail of Mods/mods.log and Player.log
  scene-lint <scene.unity> [--project DIR]
                                 check a SceneTools scene against the official stage rules

Overrides: --data-dir DIR (or MPN_DATA_DIR) for the game's LocalLow folder, --steam DIR for the Steam root.
"""
import argparse
import datetime
import json
import os
import re
import shutil
import sys
import zipfile
from pathlib import Path

APP_ID = "488860"
COMPANY = "Gibbing Tree"
PRODUCT = "Madness Project Nexus"
TOOL_OUTPUTS = {
    "scenetools": ("Madness Project Nexus SceneTools", "SceneObjs", ".sobj"),
    "modtools": ("Madness Project Nexus ModTools", "MadObjs", ".mobj"),
}
CURLY = "\u201c\u201d\u2018\u2019"
CURLY_DOUBLE = "\u201c\u201d"
# What the game's loader accepts, read from its Assembly-CSharp (build 25615280, loader of beta 2.2.a):
# info.txt or info.json; Name and Description both required; .json cards only as a container keyed by these names;
# a .txt card's Type names a MadCard_<Type> class (case-sensitive) or Event/Mission/Cutscene/Driver (any case).
INFO_NAMES = ("info.txt", "info.json")
INFO_KEYS = {"Name", "Author", "Description", "Tags", "Dependencies", "AssemblyPath", "HarmonyID", "EntryType", "PatchOrder", "ManualPatch"}
CARD_TYPES = {"Addon", "AddonGroup", "Ammo", "AnimationProfile", "Animations", "Armor", "Campaign", "Character", "Chatter",
              "Corpus", "DoT", "Dungeon", "Execution", "Explosion", "Gang", "Keycard", "Loadout", "Melee", "Origin", "Ranged",
              "Skill", "SkillGroup", "Skin", "SoundPack", "Stage_Arena", "Stage_CAMPAIGN", "Stage_Custom", "Stage_Dungeon",
              "Stage_Story", "Swatch", "Thrown", "VisualStockpile", "VoiceSet", "Wardrobe"}
SCRIPT_TYPES = {"event", "mission", "cutscene", "driver"}
JSON_KEYS = (CARD_TYPES - {"Stage_CAMPAIGN"}) | {"Item", "Event", "Mission", "Cutscene", "Driver"}
GAME_VERSION = re.compile(r"GAME INITIALIZED: (\w+) v([\w.]+)")


def emit(obj, strict=False):
    print(json.dumps(obj, indent=2, ensure_ascii=False))
    if strict and obj.get("errors"):
        sys.exit(1)


# ---------------------------------------------------------------- locations

def steam_roots(override=None):
    if override:
        return [Path(override).expanduser()]
    roots = []
    home = Path.home()
    if sys.platform.startswith("win"):
        try:
            import winreg
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Software\Valve\Steam") as k:
                roots.append(Path(winreg.QueryValueEx(k, "SteamPath")[0]))
        except OSError:
            pass
        roots.append(Path(os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")) / "Steam")
    elif sys.platform == "darwin":
        roots.append(home / "Library/Application Support/Steam")
    else:
        roots += [home / ".steam/steam", home / ".local/share/Steam",
                  home / ".var/app/com.valvesoftware.Steam/.local/share/Steam"]
    seen, out = set(), []
    for r in roots:
        key = str(r.resolve()).lower() if r.exists() else None
        if key and key not in seen:
            seen.add(key)
            out.append(r)
    return out


def libraries(roots):
    libs = []
    for root in roots:
        libs.append(root)
        vdf = root / "steamapps" / "libraryfolders.vdf"
        if vdf.is_file():
            for m in re.finditer(r'"path"\s+"([^"]+)"', vdf.read_text(encoding="utf-8", errors="replace")):
                libs.append(Path(m.group(1).replace("\\\\", "\\")))
    seen, out = set(), []
    for lib in libs:
        k = str(lib).lower().rstrip("\\/")
        if k not in seen and lib.exists():
            seen.add(k)
            out.append(lib)
    return out


def acf_value(text, key):
    m = re.search(r'"%s"\s+"([^"]*)"' % re.escape(key), text, re.IGNORECASE)
    return m.group(1) if m else None


def find_install(libs):
    for lib in libs:
        acf = lib / "steamapps" / ("appmanifest_%s.acf" % APP_ID)
        if acf.is_file():
            t = acf.read_text(encoding="utf-8", errors="replace")
            installdir = acf_value(t, "installdir") or "Madness Project Nexus 2"
            updated = acf_value(t, "LastUpdated")
            flags = acf_value(t, "StateFlags") or "0"
            todo, done = acf_value(t, "BytesToDownload"), acf_value(t, "BytesDownloaded")
            return {
                "fully_installed": flags.isdigit() and int(flags) & 4 == 4 and not (todo and done and int(done) < int(todo)),
                "download": "%s of %s bytes" % (done, todo) if todo and done and int(done) < int(todo) else None,
                "library": str(lib),
                "manifest": str(acf),
                "install_dir": str(lib / "steamapps" / "common" / installdir),
                "build_id": acf_value(t, "buildid"),
                "beta_key": acf_value(t, "BetaKey") or "",
                "last_updated": datetime.datetime.fromtimestamp(int(updated), datetime.timezone.utc).strftime("%Y-%m-%d") if updated and updated.isdigit() and int(updated) > 0 else None,
            }
    return None


def data_dir(args, install):
    if getattr(args, "data_dir", None):
        return Path(args.data_dir).expanduser()
    if os.environ.get("MPN_DATA_DIR"):
        return Path(os.environ["MPN_DATA_DIR"]).expanduser()
    cands = []
    if sys.platform.startswith("win"):
        cands.append(Path.home() / "AppData" / "LocalLow" / COMPANY / PRODUCT)
    if install:  # Proton prefix (Linux, Steam Deck)
        cands.append(Path(install["library"]) / "steamapps" / "compatdata" / APP_ID / "pfx" / "drive_c" /
                     "users" / "steamuser" / "AppData" / "LocalLow" / COMPANY / PRODUCT)
    for c in cands:
        if c.is_dir():
            return c
    return cands[0] if cands else None


def tool_outputs(ddir):
    out = {}
    if not ddir:
        return out
    base = ddir.parent  # .../LocalLow/Gibbing Tree
    for key, (product, sub, ext) in TOOL_OUTPUTS.items():
        d = base / product / sub
        files = sorted(d.glob("*" + ext), key=lambda p: p.stat().st_mtime, reverse=True) if d.is_dir() else []
        out[key] = {"folder": str(d), "exists": d.is_dir(),
                    "newest": [{"file": f.name, "modified": datetime.datetime.fromtimestamp(f.stat().st_mtime).isoformat(timespec="seconds")} for f in files[:5]]}
    return out


def mod_log_path(mods):
    """The loader writes Mods/mods.log; the official doc calls it mods.txt, so accept both."""
    if not mods:
        return None
    for name in ("mods.log", "mods.txt"):
        if (mods / name).is_file():
            return str(mods / name)
    return None


def read_enabled(mods):
    """Mods/enabled.txt is {"Enabled": [folder names]}; once it exists, a mod folder not listed starts disabled."""
    p = mods / "enabled.txt" if mods else None
    if not p or not p.is_file():
        return None
    try:
        data = json.loads(p.read_text(encoding="utf-8-sig", errors="replace"))
        return {"file": str(p), "enabled": list(data.get("Enabled") or [])}
    except (ValueError, AttributeError) as e:
        return {"file": str(p), "enabled": None, "error": "does not parse: %s" % e}


def game_version(ddir):
    p = ddir / "Player.log" if ddir else None
    if not p or not p.is_file():
        return None
    try:
        with open(p, encoding="utf-8", errors="replace") as fh:
            for i, line in enumerate(fh):
                m = GAME_VERSION.search(line)
                if m:
                    return {"status": m.group(1), "version": m.group(2), "mods_allowed": m.group(1) == "Beta"}
                if i > 2000:
                    break
    except OSError:
        pass
    return None


def cmd_where(args):
    roots = steam_roots(args.steam)
    install = find_install(libraries(roots))
    ddir = data_dir(args, install)
    notes, errors = [], []
    if not install:
        notes.append("Steam install of app 488860 not found; pass --steam or check the Steam library folders.")
    else:
        idir = Path(install["install_dir"])
        if not install["fully_installed"] or not idir.is_dir() or not any(idir.iterdir()):
            notes.append("Steam lists the game but its files are not (fully) downloaded%s; finish the install and launch it once before modding" % (" (" + install["download"] + ")" if install["download"] else ""))
        bk = install["beta_key"]
        install["branch"] = bk or "public (release)"
        if bk.lower() != "releasebeta":
            notes.append("Not on the releasebeta branch. As of 2026-10 official modding (Workshop, SceneTools, ModTools, Harmony) runs only on releasebeta; back up first (mpn.py backup), then Steam > Properties > Betas.")
    gv = game_version(ddir)
    if gv and gv["status"] != "Beta":
        notes.append("Player.log says the last launch ran a %s build (v%s): the game loads mods only when this reads Beta. Relaunch after switching branch." % (gv["status"], gv["version"]))
    mods = ddir / "Mods" if ddir else None
    res = {
        "steam_roots": [str(r) for r in roots],
        "install": install,
        "data_dir": str(ddir) if ddir else None,
        "data_dir_exists": bool(ddir and ddir.is_dir()),
        "mods_dir": str(mods) if mods else None,
        "mods": sorted(p.name for p in mods.iterdir() if p.is_dir()) if mods and mods.is_dir() else [],
        "mods_log": mod_log_path(mods),
        "enabled": read_enabled(mods),
        "player_log": str(ddir / "Player.log") if ddir and (ddir / "Player.log").is_file() else None,
        "game_version": game_version(ddir),
        "tool_outputs": tool_outputs(ddir),
        "notes": notes,
        "errors": errors,
    }
    emit(res, args.strict)


# ---------------------------------------------------------------- backup

def cmd_backup(args):
    install = find_install(libraries(steam_roots(args.steam)))
    ddir = data_dir(args, install)
    if not ddir or not ddir.is_dir():
        return emit({"ok": False, "errors": ["game data folder not found: %s" % ddir]}, args.strict)
    out_dir = Path(args.out).expanduser() if args.out else Path.home() / "mpn-backups"
    out_dir.mkdir(parents=True, exist_ok=True)
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    zpath = out_dir / ("mpn-data-%s.zip" % stamp)
    count = 0
    with zipfile.ZipFile(zpath, "w", zipfile.ZIP_DEFLATED) as z:
        for f in ddir.rglob("*"):
            if f.is_file() and f.name not in ("Player.log", "Player-prev.log"):
                z.write(f, f.relative_to(ddir.parent))
                count += 1
    emit({"ok": True, "zip": str(zpath), "files": count, "bytes": zpath.stat().st_size, "source": str(ddir), "errors": []}, args.strict)


# ---------------------------------------------------------------- mod checks

def split_objects(text):
    """Top-level {...} blocks of a shorthand MadCard file, respecting strings."""
    blocks, depth, start, in_str, esc = [], 0, None, False, False
    for i, ch in enumerate(text):
        if in_str:
            if esc:
                esc = False
            elif ch == "\\":
                esc = True
            elif ch == '"':
                in_str = False
            continue
        if ch == '"':
            in_str = True
        elif ch == "{":
            if depth == 0:
                start = i
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0 and start is not None:
                blocks.append(text[start:i + 1])
                start = None
            if depth < 0:
                return blocks, "unbalanced '}' at offset %d" % i
    if depth != 0 or in_str:
        return blocks, "unbalanced braces or an unclosed string"
    return blocks, None


def top_value(block, key):
    """Value of a top-level "Key": "value" in a block (depth 1 only)."""
    depth, i = 0, 0
    pat = re.compile(r'"%s"\s*:\s*"([^"]*)"' % re.escape(key))
    in_str = False
    while i < len(block):
        ch = block[i]
        if in_str:
            if ch == "\\":
                i += 1
            elif ch == '"':
                in_str = False
        elif ch == '"':
            if depth == 1:
                m = pat.match(block, i)
                if m:
                    return m.group(1)
            in_str = True
        elif ch in "{[":
            depth += 1
        elif ch in "}]":
            depth -= 1
        i += 1
    return None


def lenient_json(text):
    try:
        return json.loads(text), None
    except ValueError:
        pass
    t = re.sub(r",(\s*[}\]])", r"\1", text)
    try:
        return json.loads(t), "parsed only after removing trailing commas (shorthand style)"
    except ValueError as e:
        return None, str(e)


def near_miss(a, b):
    """True when a and b differ only by case or by at most two edits (a swap counts as one)."""
    if a.lower() == b.lower():
        return True
    a, b = a.lower(), b.lower()
    if re.sub(r"\d", "", a) == re.sub(r"\d", "", b):
        return False  # Nexus1_Agent and Nexus2_Agent are siblings, not typos
    if abs(len(a) - len(b)) > 2 or min(len(a), len(b)) < 4:
        return False
    d = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]
    for i in range(len(a) + 1):
        d[i][0] = i
    for j in range(len(b) + 1):
        d[0][j] = j
    for i in range(1, len(a) + 1):
        for j in range(1, len(b) + 1):
            cost = 0 if a[i - 1] == b[j - 1] else 1
            d[i][j] = min(d[i - 1][j] + 1, d[i][j - 1] + 1, d[i - 1][j - 1] + cost)
            if i > 1 and j > 1 and a[i - 1] == b[j - 2] and a[i - 2] == b[j - 1]:
                d[i][j] = min(d[i][j], d[i - 2][j - 2] + 1)
    return d[len(a)][len(b)] <= 2


def check_mod(mod):
    errors, warnings, info = [], [], {}
    if not mod.is_dir():
        return {"mod": str(mod), "errors": ["not a folder"], "warnings": [], "info": {}}
    files = [p for p in mod.rglob("*") if p.is_file()]
    top = {f.name.lower(): f for f in mod.iterdir() if f.is_file()}
    infos = [top[n] for n in INFO_NAMES if n in top]
    infop = infos[0] if infos else None
    for f in files:
        if f.suffix.lower() in (".txt", ".json"):
            t = f.read_text(encoding="utf-8-sig", errors="replace")
            if not any(c in t for c in CURLY):
                continue
            rel = f.relative_to(mod)
            if f == infop or f.suffix.lower() == ".json":
                errors.append("%s: curly quotes found; the game parses this file as strict JSON, so use straight quotes (examples pasted from the Google Docs carry curly ones)" % rel)
            elif any(c in t for c in "‘’"):
                warnings.append("%s: curly single quotes found; the game straightens only curly double quotes in .txt cards" % rel)
            else:
                warnings.append("%s: curly double quotes found; the game straightens them in .txt cards, but other tools will choke, so use straight quotes" % rel)
    meta = None
    if len(infos) > 1:
        warnings.append("both info.txt and info.json: the game uses whichever it finds first; keep one")
    if not infop:
        warnings.append("no info.txt or info.json: a local mod is not recognised without one (Workshop items do not need one)")
    else:
        meta, note = lenient_json(infop.read_text(encoding="utf-8-sig", errors="replace"))
        if meta is None:
            errors.append("%s does not parse: %s" % (infop.name, note))
        else:
            if note:
                info["info_txt_note"] = note
            if not isinstance(meta, dict):
                errors.append("%s must be one {...} object" % infop.name)
                meta = None
    if meta:
        info["name"] = meta.get("Name")
        for k in ("Name", "Description"):
            if not meta.get(k):
                errors.append("info has no %s: the game skips a mod whose Name or Description is empty (\"info.txt contains no data\")" % k)
        if not meta.get("Author"):
            warnings.append("info has no Author")
        deps = meta.get("Dependencies", [])
        if deps and not all(str(d).isdigit() for d in deps):
            errors.append("Dependencies must be Steam Workshop item ids (numbers); local mods cannot depend on local mods")
        extra = sorted(set(meta) - INFO_KEYS)
        if extra:
            warnings.append("info keys the game ignores: %s (keys are case-sensitive)" % ", ".join(extra))
        ap = meta.get("AssemblyPath")
        if ap:
            if not (mod / ap).is_file():
                errors.append("AssemblyPath %s not found relative to the mod folder (the game logs 'HARMONY FAIL: Path ... not found.')" % ap)
            et = meta.get("EntryType")
            if et and "." not in et:
                warnings.append("EntryType %s has no namespace: fine only if the class sits in the global namespace; otherwise write Namespace.Class" % et)
            if not meta.get("HarmonyID"):
                warnings.append("no HarmonyID: the game makes up local.<random guid> each launch, so other mods cannot order against yours")
        elif any(k in meta for k in ("EntryType", "HarmonyID", "ManualPatch")):
            errors.append("Harmony keys set but no AssemblyPath")
    cards, card_files = [], 0
    for f in files:
        rel = str(f.relative_to(mod))
        low = f.name.lower()
        if low in INFO_NAMES or low in ("mods.txt", "mods.log", "enabled.txt"):
            continue
        if f.suffix.lower() == ".json":
            card_files += 1
            try:
                data = json.loads(f.read_text(encoding="utf-8-sig", errors="replace"))
            except ValueError as e:
                errors.append("%s: not valid JSON (%s). Standard .json must be strict; save shorthand as .txt" % (rel, e))
                continue
            if not isinstance(data, dict):
                errors.append("%s: top level must be an object of Type arrays" % rel)
                continue
            if "Filename" in data or "Type" in data:
                errors.append("%s: a bare card in a .json file; the game reads .json only as {\"<Type>\": [cards]} and silently ignores this card. Wrap it, or save it as .txt" % rel)
                continue
            for typ, arr in data.items():
                if typ not in JSON_KEYS:
                    near = sorted(k for k in JSON_KEYS if near_miss(typ, k))
                    warnings.append("%s: \"%s\" is not a card list the game reads; its cards are ignored%s" % (rel, typ, " (did you mean %s?)" % near[0] if near else ""))
                    continue
                if not isinstance(arr, list):
                    errors.append("%s: %s should be an array of cards" % (rel, typ))
                    continue
                for c in arr:
                    if not isinstance(c, dict) or not c.get("Filename"):
                        errors.append("%s: a %s card has no Filename" % (rel, typ))
                    else:
                        cards.append({"file": rel, "type": typ, "filename": c["Filename"], "root": c.get("Root")})
        elif f.suffix.lower() == ".txt":
            card_files += 1
            text = f.read_text(encoding="utf-8-sig", errors="replace")
            for a, b in (("“", '"'), ("”", '"')):
                text = text.replace(a, b)
            blocks, err = split_objects(text)
            if err:
                errors.append("%s: %s" % (rel, err))
            if not blocks:
                warnings.append("%s: no {...} cards found" % rel)
            for b in blocks:
                typ, fn = top_value(b, "Type"), top_value(b, "Filename")
                if not typ:
                    errors.append("%s: a shorthand card has no Type (required in .txt)" % rel)
                elif typ not in CARD_TYPES and typ.lower() not in SCRIPT_TYPES:
                    near = [k for k in CARD_TYPES if k.lower() == typ.lower()]
                    errors.append("%s: Type \"%s\" is not a card type the game knows (it logs 'TYPE ERROR! Type Doesn't Exist')%s" % (rel, typ, "; Types are case-sensitive, did you mean %s?" % near[0] if near else ""))
                if not fn and (typ or "").lower() not in SCRIPT_TYPES:
                    errors.append("%s: a card has no Filename" % rel)
                if fn:
                    cards.append({"file": rel, "type": typ, "filename": fn, "root": top_value(b, "Root")})
    names = {c["filename"] for c in cards}
    for c in cards:
        r = c["root"]
        if r and r not in names and r != c["filename"]:
            close = [n for n in names | {c["filename"]} if n != r and near_miss(r, n)]
            if close:
                warnings.append("%s: %s card %s has Root \"%s\", which looks like a misspelling of %s; a Root the game cannot find gives no inheritance and no error" % (c["file"], c["type"], c["filename"], r, close[0]))
    seen = {}
    for c in cards:
        k = (c["type"], c["filename"])
        if k in seen:
            warnings.append("duplicate %s card %s in %s and %s (the later one wins by Priority/load order)" % (c["type"], c["filename"], seen[k], c["file"]))
        seen[k] = c["file"]
    bundles = [str(f.relative_to(mod)) for f in files if f.suffix.lower() in (".mobj", ".sobj")]
    manif = [str(f.relative_to(mod)) for f in files if f.suffix.lower() == ".manifest"]
    if manif:
        warnings.append("Unity .manifest files are not needed in a mod: %s" % ", ".join(manif))
    if not cards and not bundles and not (meta and meta.get("AssemblyPath")):
        errors.append("the mod has no MadCards, no .mobj/.sobj bundles and no assembly; it would do nothing")
    info.update({
        "cards": len(cards), "card_files": card_files,
        "card_list": cards[:50],
        "bundles": bundles,
        "preview": any(f.stem.lower() == "preview" for f in mod.iterdir() if f.is_file()),
    })
    return {"mod": str(mod), "errors": errors, "warnings": warnings, "info": info}


def cmd_check_mod(args):
    emit(check_mod(Path(args.mod).expanduser()), args.strict)


def cmd_deploy(args):
    src = Path(args.mod).expanduser().resolve()
    res = check_mod(src)
    install = find_install(libraries(steam_roots(args.steam)))
    ddir = data_dir(args, install)
    if res["errors"] and not args.force:
        res["deployed"] = False
        res["errors"].append("not deployed: fix the errors or pass --force")
        return emit(res, args.strict)
    if not ddir:
        res["errors"].append("game data folder unknown; pass --data-dir")
        return emit(res, args.strict)
    dest = ddir / "Mods" / src.name
    if dest.resolve() == src:
        res.update({"deployed": True, "dest": str(dest), "note": "already in the Mods folder"})
        return emit(res, args.strict)
    if dest.exists():
        if not args.force:
            res["deployed"] = False
            res["errors"].append("%s exists; pass --force to replace it" % dest)
            return emit(res, args.strict)
        shutil.rmtree(dest)
    dest.parent.mkdir(parents=True, exist_ok=True)
    shutil.copytree(src, dest)
    res.update({"deployed": True, "dest": str(dest),
                "next": "In game: gear icon (main menu, bottom left) > select the mod > ENABLED > Apply. Then run mpn.py log."})
    emit(res, args.strict)


PROBLEM = re.compile(r"error|exception|fail|could not|couldn't|invalid|missing|not found|null ?reference|contains no data|already exists", re.IGNORECASE)
# Player.log noise seen on a clean launch of build 25615280 (no mods): not mod problems.
# The data-*.dll line is Mono loading a mod assembly from bytes (beta 2.20.a), not a failure.
NOISE = re.compile(r"Shader is not supported on this GPU|TRANSITION SQUAD FAIL|Launch Parameters|\[FAILED\].*\.exe$|Debug\.bindings\.h|Fallback handler could not load library .*data-[0-9A-F]+\.dll", re.IGNORECASE)
MOD_LINE = re.compile(r"^\s*> \((\d+)\) \[(LOCAL|WORKSHOP)\] (.+)$")
CARD_LINE = re.compile(r"^\s*> (?!FOUND:)([A-Z]+): (.+?) \((\w+)\)")


def scan_log(p, lines):
    if not p or not Path(p).is_file():
        return None
    t = Path(p).read_text(encoding="utf-8", errors="replace").splitlines()
    probs = []
    for i, l in enumerate(t):
        if PROBLEM.search(l) and not NOISE.search(l):
            pr = {"line": i + 1, "text": l[:300]}
            if "Exception" in l:  # the frames name the method that threw: a mod's class or the game code it upset
                pr["stack"] = [s.strip()[:200] for s in t[i + 1:i + 6] if s.lstrip().startswith("at ")][:4]
            probs.append(pr)
    return {"file": str(p), "modified": datetime.datetime.fromtimestamp(Path(p).stat().st_mtime).isoformat(timespec="seconds"),
            "lines": len(t), "problems": probs[-60:], "problem_count": len(probs), "tail": [l[:300] for l in t[-lines:]]}


def cmd_log(args):
    install = find_install(libraries(steam_roots(args.steam)))
    ddir = data_dir(args, install)
    mods = ddir / "Mods" if ddir else None
    res = {"data_dir": str(ddir) if ddir else None, "game_version": game_version(ddir), "notes": [], "errors": []}
    res["enabled"] = read_enabled(mods)
    res["mods_log"] = scan_log(mod_log_path(mods), args.lines)
    if res["mods_log"]:
        t = Path(res["mods_log"]["file"]).read_text(encoding="utf-8", errors="replace").splitlines()
        res["mods_log"]["loaded"] = [{"n": int(m.group(1)), "source": m.group(2), "dir": m.group(3)} for m in map(MOD_LINE.match, t) if m]
        res["mods_log"]["applied_harmony"] = [l.split("APPLIED HARMONY PATCH:", 1)[1].strip() for l in t if "APPLIED HARMONY PATCH:" in l]
        res["mods_log"]["cards"] = [{"kind": m.group(1), "card": m.group(2), "type": m.group(3)} for m in map(CARD_LINE.match, t) if m][:100]
    res["player_log"] = scan_log(ddir / "Player.log" if ddir else None, args.lines)
    gv = res["game_version"]
    if gv and not gv["mods_allowed"]:
        res["notes"].append("The last launch ran a %s build (v%s): the game loads no mods unless this reads Beta (the releasebeta branch). Nothing below is about your mod." % (gv["status"], gv["version"]))
    folders = sorted(p.name for p in mods.iterdir() if p.is_dir()) if mods and mods.is_dir() else []
    en = (res["enabled"] or {}).get("enabled")
    if en is not None:
        off = [f for f in folders if f not in en]
        if off:
            res["notes"].append("Not in enabled.txt, so off at launch: %s. Enable in the gear-icon menu and Apply." % ", ".join(off))
    if not res["mods_log"] and not res["player_log"]:
        res["errors"].append("no Mods/mods.log or Player.log found; launch the game once with the mod enabled")
    elif not res["mods_log"]:
        res["notes"].append("no Mods/mods.log yet: the game writes it when it applies mods, which happens only on a Beta build with -nomods absent")
    emit(res, args.strict)


# ---------------------------------------------------------------- scene lint

DOC = re.compile(r"^--- !u!(\d+) &(-?\d+)( stripped)?[ \t]*\r?$", re.MULTILINE)
REF = r"\{fileID: (-?\d+)(?:, guid: ([0-9a-f]{32}))?"


def parse_unity(text):
    docs = {}
    ms = list(DOC.finditer(text))
    for i, m in enumerate(ms):
        body = text[m.end(): ms[i + 1].start() if i + 1 < len(ms) else len(text)]
        docs[m.group(2)] = {"cls": int(m.group(1)), "stripped": bool(m.group(3)), "body": body}
    return docs


def field(body, name):
    m = re.search(r"^\s*%s: (.*)$" % re.escape(name), body, re.MULTILINE)
    return m.group(1).strip() if m else None


def ref(body, name):
    m = re.search(r"%s: %s" % (re.escape(name), REF), body.replace("\n        type", " type").replace("\n    type", " type"))
    return (m.group(1), m.group(2)) if m else (None, None)


def modifications(body):
    mods = []
    flat = re.sub(r",\s*\n\s+type: (\d)", r", type: \1", body)
    for m in re.finditer(r"- target: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: \d\}\s*\n\s*propertyPath: (.*)\n\s*value: (.*)", flat):
        mods.append((m.group(1), m.group(2), m.group(3).strip(), m.group(4).strip()))
    return mods


class Project:
    def __init__(self, root):
        self.root = root
        self.guid_path = {}
        self.cache = {}
        self.layers = {8: "Terrain"}
        if root:
            for meta in (root / "Assets").rglob("*.prefab.meta"):
                g = re.search(r"^guid: ([0-9a-f]{32})", meta.read_text(encoding="utf-8", errors="replace"), re.MULTILINE)
                if g:
                    self.guid_path[g.group(1)] = meta.with_suffix("")
            tm = root / "ProjectSettings" / "TagManager.asset"
            if tm.is_file():
                t = tm.read_text(encoding="utf-8", errors="replace")
                m = re.search(r"^  layers:\n((?:  - .*\n)+)", t.replace("\r\n", "\n"), re.MULTILINE)
                if m:
                    self.layers = {i: l[4:].strip() for i, l in enumerate(m.group(1).splitlines()) if l[4:].strip()}

    def prefab_name(self, guid):
        p = self.guid_path.get(guid)
        return p.stem if p else None

    def prefab_docs(self, guid):
        if guid not in self.cache:
            p = self.guid_path.get(guid)
            self.cache[guid] = parse_unity(p.read_text(encoding="utf-8", errors="replace")) if p else {}
        return self.cache[guid]

    def root_transform(self, guid):
        for fid, d in self.prefab_docs(guid).items():
            if d["cls"] == 4 and ref(d["body"], "m_Father")[0] == "0":
                return fid
        return None

    def object_name_in_prefab(self, guid, transform_fid):
        docs = self.prefab_docs(guid)
        d = docs.get(transform_fid)
        if not d:
            return None
        go = docs.get(ref(d["body"], "m_GameObject")[0])
        return field(go["body"], "m_Name") if go else None


def find_project(scene):
    for p in [scene.parent] + list(scene.parents):
        if (p / "Assets").is_dir() and (p / "ProjectSettings").is_dir():
            return p
    return None


def cmd_scene_lint(args):
    scene = Path(args.scene).expanduser()
    errors, warnings, info = [], [], {}
    if not scene.is_file():
        return emit({"scene": str(scene), "errors": ["scene file not found"], "warnings": [], "info": {}}, args.strict)
    text = scene.read_text(encoding="utf-8", errors="replace")
    if not text.startswith("%YAML"):
        return emit({"scene": str(scene), "errors": ["scene is binary; set Edit > Project Settings > Editor > Asset Serialization to Force Text"], "warnings": [], "info": {}}, args.strict)
    proj = Project(Path(args.project).expanduser() if args.project else find_project(scene))
    if not proj.root:
        warnings.append("SceneTools project not found above the scene; prefab names fall back to instance names (pass --project)")
    docs = parse_unity(text)

    def go_name(go_fid):
        d = docs.get(go_fid)
        return field(d["body"], "m_Name") if d else None

    def transform_name(t_fid):
        d = docs.get(t_fid)
        if not d:
            return None
        if d["stripped"]:
            src_fid, guid = ref(d["body"], "m_CorrespondingSourceObject")
            return proj.object_name_in_prefab(guid, src_fid) if guid else None
        return go_name(ref(d["body"], "m_GameObject")[0])

    instances = []
    for fid, d in docs.items():
        if d["cls"] != 1001:
            continue
        guid = ref(d["body"], "m_SourcePrefab")[1]
        mods = modifications(d["body"])
        name = next((v for (_, g, p, v) in mods if p == "m_Name" and g == guid), None)
        kind = proj.prefab_name(guid) or re.sub(r" \(\d+\)$", "", name or "?")
        root_t = proj.root_transform(guid) if guid else None
        rot = {p[-1]: v for (t, g, p, v) in mods if p.startswith("m_LocalRotation.") and (root_t is None or t == root_t)}
        rotated = any(abs(float(rot.get(a, 0) or 0)) > 1e-4 for a in "xyz")
        parent = transform_name(ref(d["body"], "m_TransformParent")[0])
        instances.append({"kind": kind, "name": name or kind, "parent": parent, "rotated": rotated})

    def of(kind):
        return [i for i in instances if i["kind"] == kind]

    counts = {}
    for i in instances:
        counts[i["kind"]] = counts.get(i["kind"], 0) + 1
    info["prefab_counts"] = dict(sorted(counts.items()))
    rooms = of("Room_Layout")
    info["rooms"] = len(rooms)

    ps = of("Player_Start")
    if len(ps) != 1:
        errors.append("S01 every scene needs exactly one Player_Start; found %d" % len(ps))
    for p in ps:
        if p["parent"] != "Characters":
            errors.append("S02 Player_Start must be parented to the room's Characters object; parent is %s" % p["parent"])
    for r in rooms:
        if r["rotated"]:
            errors.append("S03 room %s is rotated; rooms must never be rotated" % r["name"])
    if not rooms:
        warnings.append("no Room_Layout prefab instance found; every playable area needs a room (unpacked rooms are not detected)")
    for z in of("Zone_Camera"):
        if z["rotated"]:
            errors.append("S04 %s is rotated; Zone_Camera must not be rotated" % z["name"])
        if z["parent"] != "Waypoints":
            errors.append("S05 %s must be parented to its room's Waypoints object or it is not detected; parent is %s" % (z["name"], z["parent"]))
    sr = of("SceneRules")
    if len(sr) > 1:
        errors.append("S06 use at most one SceneRules object; found %d" % len(sr))
    for s in sr:
        if s["parent"]:
            warnings.append("S07 SceneRules belongs in the scene root; it is under %s" % s["parent"])

    terrain = next((i for i, n in proj.layers.items() if n == "Terrain"), 8)
    floors = 0
    for fid, d in docs.items():
        if d["cls"] == 1 and not d["stripped"]:
            b = d["body"]
            name = field(b, "m_Name")
            if field(b, "m_TagString") == "Floor":
                floors += 1
                if field(b, "m_Layer") != str(terrain):
                    errors.append("S08 floor %s has layer %s; floors need Layer Terrain" % (name, proj.layers.get(int(field(b, "m_Layer") or 0), field(b, "m_Layer"))))
                if (field(b, "m_StaticEditorFlags") or "0") == "0":
                    errors.append("S09 floor %s is not Static" % name)
    for fid, d in docs.items():
        if d["cls"] == 20 and not d["stripped"]:
            n = go_name(ref(d["body"], "m_GameObject")[0])
            if n and n != "Dev_Cam":
                warnings.append("S10 camera %s is not used by the game; delete the default Main Camera" % n)
        if d["cls"] == 108 and not d["stripped"]:
            n = go_name(ref(d["body"], "m_GameObject")[0])
            if n == "Directional Light":
                warnings.append("S11 the default Directional Light is ignored (lights outside rooms are not used); delete it")
    info["floors_tagged"] = floors
    if floors == 0:
        warnings.append("no object tagged Floor in the scene file (floors inside room prefabs are not checked); characters need a Floor to spawn and path on")
    lights = sum(1 for d in docs.values() if d["cls"] == 108)
    info["lights_in_scene_file"] = lights
    if lights > 4 * max(1, len(rooms)):
        warnings.append("S12 %d lights for %d room(s); only 4 are active at once (3 on low quality); use the Glow Effect instead" % (lights, len(rooms)))

    meta = scene.with_name(scene.name + ".meta")
    label = None
    if meta.is_file():
        label = field(meta.read_text(encoding="utf-8", errors="replace"), "assetBundleName")
    info["asset_label"] = label or None
    if not label:
        errors.append("S13 the scene has no Asset Label (AssetBundle name); >> Build will not export it")
    info["playground_listed"] = scene.stem.endswith("_PG")
    if not scene.stem.endswith("_PG"):
        info["playground_note"] = "no _PG suffix: it will not show in the Playground scene list (fine for stage-only scenes)"
    emit({"scene": str(scene), "project": str(proj.root) if proj.root else None,
          "errors": errors, "warnings": warnings, "info": info}, args.strict)


def main(argv=None):
    ap = argparse.ArgumentParser(description="MADNESS: Project Nexus modding helper (one JSON object out)")
    ap.add_argument("--strict", action="store_true", help="exit 1 when the result has errors")
    ap.add_argument("--data-dir", help="the game's LocalLow data folder (holds Mods/)")
    ap.add_argument("--steam", help="Steam root folder")
    sub = ap.add_subparsers(dest="cmd", required=True)
    sub.add_parser("where")
    b = sub.add_parser("backup")
    b.add_argument("--out")
    c = sub.add_parser("check-mod")
    c.add_argument("mod")
    d = sub.add_parser("deploy")
    d.add_argument("mod")
    d.add_argument("--force", action="store_true")
    lg = sub.add_parser("log")
    lg.add_argument("--lines", type=int, default=30)
    s = sub.add_parser("scene-lint")
    s.add_argument("scene")
    s.add_argument("--project")
    args = ap.parse_args(argv)
    try:
        {"where": cmd_where, "backup": cmd_backup, "check-mod": cmd_check_mod, "deploy": cmd_deploy,
         "log": cmd_log, "scene-lint": cmd_scene_lint}[args.cmd](args)
    except Exception as e:  # fail soft: one JSON object, always
        emit({"errors": ["%s: %s" % (type(e).__name__, e)]}, args.strict)


if __name__ == "__main__":
    main()
