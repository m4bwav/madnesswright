#!/usr/bin/env python3
"""decomp.py - look things up in a decompiled Assembly-CSharp without reading whole files.

Standard library only, Python 3.9+. Every command prints exactly one JSON object and exits 0.
It reads the decompile folder and writes nothing, except `decompile`, which writes only to --out.
Never commit the decompile folder or paste its files anywhere public.

Commands
  decompile <Assembly-CSharp.dll> --out DIR   run ilspycmd -p (found on PATH or in ~/.dotnet/tools)
  keys <dir> [--keys F1,U,...]                every key the game reads: KeyCode literals, rebindable
                                              defaults (static KeyCode key_X = ...) and Input.GetButton names;
                                              files that only run in debug builds are marked dev_only
  find <dir> <Name> [--max N]                 declarations of a type or member: file, line, signature line
  callers <dir> <Name> [--max N]              call sites of a member: file, line, the calling line
"""
import argparse
import json
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path

ACCESS = r"(?:public|private|protected|internal)"
DEV_MARKERS = ("Game_Manager.IsDebugMode()", "Debug.isDebugBuild")
LAUNCH_GATE = "LaunchParameters_Manager.Params.devtools"
KEY_LITERAL = re.compile(r"KeyCode\.(\w+)")
KEY_DEFAULT = re.compile(r"static\s+KeyCode\s+(\w+)\s*=\s*KeyCode\.(\w+)")
BUTTON = re.compile(r"Input\.GetButton(?:Down|Up)?\(\"([^\"]+)\"\)")


def emit(obj):
    print(json.dumps(obj, indent=2, ensure_ascii=False))


def sources(root):
    for p in sorted(Path(root).rglob("*.cs")):
        try:
            yield p, p.read_text(encoding="utf-8", errors="replace")
        except OSError:
            continue


def rel(p, root):
    return str(Path(p).relative_to(root)).replace(os.sep, "/")


def dev_only(text):
    # A class whose Start/Awake switches itself on only in debug builds (Dev_Control) or that checks
    # the debug flag before reading keys, or one created only by the -devtools launch option (ModTestTools). A hint, not proof: open the file before trusting it.
    if LAUNCH_GATE in text:
        return True
    return any(m in text for m in DEV_MARKERS) and re.search(r"enabled\s*=\s*[^;]*(IsDebugMode|isDebugBuild)", text) is not None


def cmd_keys(args):
    root = Path(args.dir)
    if not root.is_dir():
        return {"errors": ["no such folder: %s" % root]}
    want = {k.strip().lower() for k in args.keys.split(",")} if args.keys else None
    keys, defaults, buttons = {}, {}, {}
    for p, text in sources(root):
        f, dev = rel(p, root), dev_only(text)
        for n, line in enumerate(text.splitlines(), 1):
            for m in KEY_DEFAULT.finditer(line):
                defaults.setdefault(m.group(2), []).append({"setting": m.group(1), "file": f, "line": n})
            for m in KEY_LITERAL.finditer(line):
                if KEY_DEFAULT.search(line):
                    continue
                keys.setdefault(m.group(1), []).append({"file": f, "line": n, "dev_only": dev})
            for m in BUTTON.finditer(line):
                buttons.setdefault(m.group(1), set()).add(f)
    if want is not None:
        keys = {k: v for k, v in keys.items() if k.lower() in want}
        defaults = {k: v for k, v in defaults.items() if k.lower() in want}
    clashes = {}
    for k in sorted(set(keys) | set(defaults)):
        live = [h for h in keys.get(k, []) if not h["dev_only"]]
        if defaults.get(k) or live:
            clashes[k] = {"rebindable_default": sorted({d["setting"] for d in defaults.get(k, [])}),
                          "read_in": sorted({h["file"] for h in live})}
    free = sorted(k for k in (want or []) if k not in {c.lower() for c in clashes})
    return {"folder": str(root), "keys": keys, "rebindable_defaults": defaults,
            "buttons": {b: sorted(v) for b, v in sorted(buttons.items())},
            "clashes": clashes, "free": free,
            "note": "clashes lists keys read outside debug-only classes or bound by default; check each file's "
                    "context (a menu that only reads keys while open is not a clash in play). Buttons are "
                    "Unity Input Manager axes whose keys live in the game's InputManager asset, not in code."}


def cmd_find(args):
    root = Path(args.dir)
    if not root.is_dir():
        return {"errors": ["no such folder: %s" % root]}
    name = re.escape(args.name)
    decl = re.compile(r"^\s*(?:\[[^\]]*\]\s*)*" + ACCESS + r"?[\w\s<>\[\],.?]*?\b(?:class|struct|enum|interface)\s+" + name + r"\b"
                      r"|^\s*" + ACCESS + r"[\w\s<>\[\],.?]*\s" + name + r"\s*(?:\(|=|;|\{|$)")
    hits = []
    for p, text in sources(root):
        for n, line in enumerate(text.splitlines(), 1):
            if decl.search(line):
                hits.append({"file": rel(p, root), "line": n, "text": line.strip()[:240]})
                if len(hits) >= args.max:
                    return {"name": args.name, "declarations": hits, "truncated": True}
    return {"name": args.name, "declarations": hits, "truncated": False}


def cmd_callers(args):
    root = Path(args.dir)
    if not root.is_dir():
        return {"errors": ["no such folder: %s" % root]}
    call = re.compile(r"(?<![\w])" + re.escape(args.name) + r"\s*\(")
    decl = re.compile(r"^\s*" + ACCESS + r"\b")
    hits = []
    for p, text in sources(root):
        for n, line in enumerate(text.splitlines(), 1):
            if call.search(line) and not decl.search(line):
                hits.append({"file": rel(p, root), "line": n, "text": line.strip()[:240]})
                if len(hits) >= args.max:
                    return {"name": args.name, "callers": hits, "truncated": True}
    return {"name": args.name, "callers": hits, "truncated": False}


def find_ilspy():
    exe = shutil.which("ilspycmd")
    if exe:
        return exe
    for name in ("ilspycmd.exe", "ilspycmd"):
        p = Path.home() / ".dotnet" / "tools" / name
        if p.is_file():
            return str(p)
    return None


def cmd_decompile(args):
    dll, out = Path(args.dll), Path(args.out)
    if not dll.is_file():
        return {"errors": ["no such file: %s" % dll]}
    exe = find_ilspy()
    if not exe:
        return {"errors": ["ilspycmd not found: dotnet tool install -g ilspycmd (the user installs it)"]}
    out.mkdir(parents=True, exist_ok=True)
    try:
        r = subprocess.run([exe, "-p", "-o", str(out), str(dll)], capture_output=True, text=True, timeout=900)
    except (OSError, subprocess.TimeoutExpired) as e:
        return {"errors": [str(e)]}
    files = sum(1 for _ in out.rglob("*.cs"))
    res = {"dll": str(dll), "out": str(out), "cs_files": files, "returncode": r.returncode}
    if r.returncode != 0 or files == 0:
        res["errors"] = [(r.stderr or r.stdout or "no output")[-800:]]
    return res


def main(argv=None):
    ap = argparse.ArgumentParser(description="Search a decompiled Assembly-CSharp (one JSON object out)")
    sub = ap.add_subparsers(dest="cmd", required=True)
    d = sub.add_parser("decompile")
    d.add_argument("dll")
    d.add_argument("--out", required=True)
    k = sub.add_parser("keys")
    k.add_argument("dir")
    k.add_argument("--keys", help="comma-separated KeyCode names to check, e.g. F1,F2,U,Delete,CapsLock")
    for name in ("find", "callers"):
        s = sub.add_parser(name)
        s.add_argument("dir")
        s.add_argument("name")
        s.add_argument("--max", type=int, default=60)
    args = ap.parse_args(argv)
    try:
        res = {"decompile": cmd_decompile, "keys": cmd_keys, "find": cmd_find, "callers": cmd_callers}[args.cmd](args)
    except Exception as e:  # fail soft: one JSON object, never a traceback
        res = {"errors": ["%s: %s" % (type(e).__name__, e)]}
    emit(res)


if __name__ == "__main__":
    sys.exit(main())
