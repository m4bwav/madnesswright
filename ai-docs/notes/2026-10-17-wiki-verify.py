"""Wiki verification for madnesswright: every command on the wiki, run against a published release.

    python ai-docs/notes/2026-10-17-wiki-verify.py [VERSION] > ai-docs/notes/2026-10-17-wiki-verify.out.txt

Downloads madnesswright-VERSION.zip from the GitHub release into a fresh scratch folder (never the working tree)
and runs mpn.py from it. The zip ships only skills/, README, LICENSE and SECURITY.md, so the test fixtures
(tests/fixtures) and the development commands come from a depth-1 clone of the tag vVERSION in the same scratch
folder. Everything the game would own is synthetic and built here: a Steam root with an app manifest
(steam/), the game's LocalLow data folder (game-data/) with Player.log, Mods/, mods.log and enabled.txt, and a
SceneTools build output. The log lines follow the shapes in the repository's tests/test_mpn.py and
skills/madnesswright/references/troubleshooting.md. Nothing reads the real Steam install: every command that
looks for the game gets --steam and --data-dir.

Output sections are '## label' lines, as wikiwright's diffout and outputs expect. Each case prints the page's
command after '$ ', its stdout, its stderr, then '[exit N]'. The scratch folder is masked as <scratch>, the home
folder as ~, the backup time stamp as <stamp>, unittest's timing as <s>. Commands on the pages read
`python mpn.py ...`; here mpn.py is the release's skills/madnesswright/scripts/mpn.py, run from <scratch>.
The program refuses to print its output if the user name or home folder survives the masking.
"""
import getpass
import hashlib
import io
import json
import os
import re
import shlex
import shutil
import subprocess
import sys
import tempfile
import time
import urllib.request
import zipfile

BS = chr(92)
VERSION = sys.argv[1] if len(sys.argv) > 1 else "0.1.1"
WORK = os.path.realpath(tempfile.mkdtemp(prefix="mwwiki_"))
PLUGIN = os.path.join(WORK, "plugin")
REPO = os.path.join(WORK, "repo")
RUN = os.path.join(WORK, "run")
URL = "https://github.com/m4bwav/madnesswright/releases/download/v%s/madnesswright-%s.zip" % (VERSION, VERSION)
blob = urllib.request.urlopen(URL, timeout=120).read()
zipfile.ZipFile(io.BytesIO(blob)).extractall(PLUGIN)
MPN = os.path.join(PLUGIN, "skills", "madnesswright", "scripts", "mpn.py")
subprocess.run(["git", "-c", "advice.detachedHead=false", "clone", "-q", "--depth", "1", "--branch", "v" + VERSION,
                "https://github.com/m4bwav/madnesswright", REPO], check=True)
os.makedirs(RUN)
OUT = []
HOME = os.path.expanduser("~")


def forms(path):
    return [path.replace(BS, BS * 2), path, path.replace(BS, "/")]


def mask(text):
    for f in forms(RUN):
        text = text.replace(f, "<scratch>")
    for f in forms(WORK):
        text = text.replace(f, "<work>")
    for f in forms(HOME):
        text = text.replace(f, "~")
    text = re.sub(r"mpn-data-[0-9]{8}-[0-9]{6}[.]zip", "mpn-data-<stamp>.zip", text)
    text = re.sub(r"Ran ([0-9]+) tests? in [0-9.]+s", lambda m: "Ran %s tests in <s>s" % m.group(1), text)
    return text.rstrip()


def show(label, text):
    OUT.append("## " + label)
    OUT.append(text)
    OUT.append("")


def case(label, command, cwd=None, env=None):
    """Run one page command as the page shows it, from <scratch> (or cwd)."""
    args = shlex.split(command)
    if args[:2] == ["python", "mpn.py"]:
        args = [sys.executable, MPN] + args[2:]
    elif args[0] == "python":
        args = [sys.executable] + args[1:]
    e = dict(os.environ)
    e.pop("MPN_DATA_DIR", None)
    e["PYTHONIOENCODING"] = "utf-8"
    e["PYTHONDONTWRITEBYTECODE"] = "1"
    if env:
        e.update(env)
    p = subprocess.run(args, cwd=cwd or RUN, capture_output=True, text=True, encoding="utf-8", errors="replace", env=e)
    lines = ["$ " + command]
    if p.stdout.strip():
        lines.append(mask(p.stdout))
    if p.stderr.strip():
        lines.append(mask(p.stderr))
    lines.append("[exit %d]" % p.returncode)
    show(label, "\n".join(lines))


def write(rel, text, mtime=None):
    path = os.path.join(RUN, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    if mtime:
        os.utime(path, (mtime, mtime))


FIXED = time.mktime((2026, 10, 17, 18, 30, 0, 0, 0, -1))  # local time, so the printed 'modified' never varies

show("installed", "\n".join([
    "madnesswright %s from %s" % (VERSION, URL),
    "zip sha256 %s" % hashlib.sha256(blob).hexdigest(),
    "fixtures and development commands from a depth-1 clone of tag v%s" % VERSION,
    "Python %s on %s" % (sys.version.split()[0], "Windows" if os.name == "nt" else sys.platform),
]))

# ---- fixtures from the tag, as the repository's tests use them
for name in ("subtle-mod", "broken-mod", "scene-project", "arena-project"):
    shutil.copytree(os.path.join(REPO, "tests", "fixtures", name), os.path.join(RUN, name))

# ---- The helper: help, and where --strict goes
case("help", "python mpn.py --help")
case("strict-after-command", "python mpn.py check-mod broken-mod --strict")

# ---- Mods and MadCards: the two fixture mods from the repository
case("check-mod-subtle", "python mpn.py check-mod subtle-mod")
case("check-mod-broken", "python mpn.py check-mod broken-mod")
case("check-mod-broken-strict", "python mpn.py --strict check-mod broken-mod")

# subtle-mod with both faults fixed: the Root spelt right, the .json card wrapped in its container
write("green-hank/info.txt", """{
  "Name": "Tough Agents, Green Hank",
  "Author": "fixture",
  "Description": "Agents get 400 health; Hank bleeds green.",
  "Tags": ["Characters"]
}
""")
write("green-hank/agents.txt", '{ "Type": "Character", "Filename": "Nexus1_Agent", "Root": "Nexus1_Agent", "Health": 400 }\n')
write("green-hank/hank.json", '{ "Character": [ { "Filename": "Hank", "Root": "Hank", "Blood": {"r": 0, "g": 1, "b": 0, "a": 1} } ] }\n')
show("green-hank/hank.json (file)", open(os.path.join(RUN, "green-hank", "hank.json"), encoding="utf-8").read().rstrip())
case("check-mod-fixed", "python mpn.py check-mod green-hank")

# Type spelling, list names, curly quotes in a .txt card, a notes file read as cards
write("type-mod/info.json", '{"Name": "Type checks", "Author": "fixture", "Description": "Cards with near-miss types."}\n')
write("type-mod/chars.json", '{"Characters": [{"Filename": "Hank", "Root": "Hank", "Health": 300}]}\n')
write("type-mod/hank.txt", '{"Type": "character", "Filename": "Hank", "Root": "Hank", "Health": 300}\n')
write("type-mod/event.txt", '{"Type": "EVENT", "Filename": "TypeMod_Heal_01", "Conditions": [], "Actions": []}\n')
L, R = chr(0x201c), chr(0x201d)  # curly double quotes, as pasted from the official Google Docs
write("type-mod/quotes.txt", "{%sType%s: %sMelee%s, %sFilename%s: %sM_ModSword%s, %sRoot%s: %sM_SwordLong%s}\n" % ((L, R) * 6))
write("type-mod/notes.txt", "Remember to test Hank in the Playground.\n")
case("check-mod-types", "python mpn.py check-mod type-mod")

# A misspelt Root is caught only when a Filename in the same mod is close to it
write("root-mod/info.txt", '{"Name": "Tough agent", "Author": "fixture", "Description": "A new agent based on Nexus1_Agent."}\n')
write("root-mod/agent.txt", '{ "Type": "Character", "Filename": "Tough_Agent", "Root": "Nexsu1_Agent", "Health": 400 }\n')
case("check-mod-root-unseen", "python mpn.py check-mod root-mod")

# ---- Assets and Harmony: the assembly keys
write("harmony-mod/info.txt", """{
  "Name": "Faster Reload",
  "Author": "fixture",
  "Description": "Reloads faster.",
  "AssemblyPath": "Managed/FasterReload.dll",
  "EntryType": "ModEntry"
}
""")
case("check-mod-harmony-missing-dll", "python mpn.py check-mod harmony-mod")
write("harmony-mod/info.txt", """{
  "Name": "Faster Reload",
  "Author": "fixture",
  "Description": "Reloads faster.",
  "AssemblyPath": "Managed/FasterReload.dll",
  "HarmonyID": "com.example.fasterreload",
  "EntryType": "FasterReload.ModEntry"
}
""")
os.makedirs(os.path.join(RUN, "harmony-mod", "Managed"), exist_ok=True)
with open(os.path.join(RUN, "harmony-mod", "Managed", "FasterReload.dll"), "wb") as f:
    f.write(b"MZ placeholder, not a real assembly")
case("check-mod-harmony", "python mpn.py check-mod harmony-mod")

# ---- Levels with SceneTools: the two fixture scenes
case("scene-lint-arena", "python mpn.py scene-lint arena-project/Assets/Scenes/Arena_PG.unity")
case("scene-lint-warehouse", "python mpn.py scene-lint scene-project/Assets/Scenes/Warehouse_PG.unity")
case("scene-lint-warehouse-strict", "python mpn.py --strict scene-lint scene-project/Assets/Scenes/Warehouse_PG.unity")
write("Binary.unity", "UnityFS binary scene stand-in\n")
case("scene-lint-binary", "python mpn.py scene-lint Binary.unity")

# ---- A synthetic game: Steam root on the release branch, data folder, SceneTools output
ACF = """"AppState"
{
	"appid"		"488860"
	"installdir"		"Madness Project Nexus 2"
	"StateFlags"		"4"
	"LastUpdated"		"1792195200"
	"buildid"		"25615280"
	"UserConfig"
	{
		"BetaKey"		"%s"
	}
}
"""
write("steam/steamapps/appmanifest_488860.acf", ACF % "")
write("steam/steamapps/common/Madness Project Nexus 2/Madness Project Nexus_Data/README.txt", "stand-in for the game files\n")
write("game-data/Player.log", """Initialize engine version: 2019.2.6f1 (fe82a0e88406)
Launch Parameters (0 found)
ERROR: Shader Shader is not supported on this GPU
GAME INITIALIZED: Release v1.11.a
TRANSITION SQUAD FAIL: NO ONE TRANSITIONING
""", FIXED)
os.makedirs(os.path.join(RUN, "game-data", "Mods"))
write("game-data/career0.sav", "stand-in save\n")
write("Madness Project Nexus SceneTools/SceneObjs/arena_pg.sobj", "stand-in bundle\n", FIXED)

case("where-release", "python mpn.py --steam steam --data-dir game-data where")
case("log-release", "python mpn.py --data-dir game-data log")
case("backup", "python mpn.py --data-dir game-data backup --out backups")

# ---- Switched to releasebeta and launched once: deploy, then the logs of a Beta launch
write("steam/steamapps/appmanifest_488860.acf", ACF % "releasebeta")
case("deploy", "python mpn.py --data-dir game-data deploy green-hank")
case("deploy-again", "python mpn.py --data-dir game-data deploy green-hank")
case("deploy-force", "python mpn.py --data-dir game-data deploy green-hank --force")
case("deploy-broken", "python mpn.py --data-dir game-data deploy broken-mod")

shutil.copytree(os.path.join(RUN, "harmony-mod"), os.path.join(RUN, "game-data", "Mods", "faster-reload"))
os.makedirs(os.path.join(RUN, "game-data", "Mods", "old-mod"))
write("game-data/Mods/old-mod/info.txt", '{"Name": "Old mod"}\n')
write("game-data/Mods/enabled.txt", '{"Enabled":["green-hank","faster-reload"]}\n', FIXED)
write("game-data/Mods/mods.log", """	=== MOD LOG ===

Scanning for Local Mods...
  > FOUND: C:/Users/<you>/AppData/LocalLow/Gibbing Tree/Madness Project Nexus/Mods/faster-reload
  > FOUND: C:/Users/<you>/AppData/LocalLow/Gibbing Tree/Madness Project Nexus/Mods/green-hank

Mods Enabled:
  > (1) [LOCAL] C:/Users/<you>/AppData/LocalLow/Gibbing Tree/Madness Project Nexus/Mods/faster-reload
  > (2) [LOCAL] C:/Users/<you>/AppData/LocalLow/Gibbing Tree/Madness Project Nexus/Mods/green-hank
  > APPLIED HARMONY PATCH: com.example.fasterreload

Scanning for MadCards...
 > FAILED: Hank (Character)

	=== END OF LOG ===
""", FIXED)
write("game-data/Player.log", """Initialize engine version: 2019.2.6f1 (fe82a0e88406)
Launch Parameters (0 found)
ERROR: Shader Shader is not supported on this GPU
GAME INITIALIZED: Beta v2.20.a
info.txt contains no data
TRANSITION SQUAD FAIL: NO ONE TRANSITIONING
""", FIXED)
case("where-beta", "python mpn.py --steam steam --data-dir game-data where")
case("log-beta", "python mpn.py --data-dir game-data log --lines 3")
case("log-nothing", "python mpn.py --data-dir empty-folder log")

# ---- Development: the repository's own checks, from the tag
case("dev-check-skills", "python tests/check_skills.py", cwd=REPO)
case("dev-test-mpn", "python tests/test_mpn.py", cwd=REPO)
case("dev-budget", "python scripts/budget.py", cwd=REPO)

text = "\n".join(OUT).rstrip() + "\n"
user = getpass.getuser()
# the user name as a path component (the GitHub owner m4bwav merely contains it)
leaks = [s + user.lower() + e for s in (BS, "/") for e in (BS, "/", '"')]
if HOME in text or HOME.replace(BS, BS * 2) in text or any(l in text.lower() for l in leaks):
    sys.stderr.write("refusing to print: the user name or home folder is still in the output\n")
    for line in text.splitlines():
        if any(l in line.lower() for l in leaks):
            sys.stderr.write("  " + line.replace(user, "<user>")[:200] + "\n")
    sys.exit(1)
sys.stdout.reconfigure(encoding="utf-8", newline="\n")
sys.stdout.write(text)
shutil.rmtree(WORK, ignore_errors=True)
