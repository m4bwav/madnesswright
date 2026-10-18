"""Tests for skills/madnesswright/scripts/mpn.py. Run: python tests/test_mpn.py

Fixtures are synthetic: a minimal SceneTools-shaped project written to a temp folder,
so no file from the official (unlicensed) SceneTools repository is copied here.
"""
import contextlib
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "skills" / "madnesswright" / "scripts"))
import mpn  # noqa: E402

G_ROOM = "a" * 32
G_PS = "b" * 32
G_ZC = "c" * 32
G_SR = "d" * 32

ROOM_PREFAB = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Name: Room_Layout
--- !u!4 &101
Transform:
  m_GameObject: {fileID: 100}
  m_Father: {fileID: 0}
--- !u!1 &200
GameObject:
  m_Name: Characters
--- !u!4 &201
Transform:
  m_GameObject: {fileID: 200}
  m_Father: {fileID: 101}
--- !u!1 &300
GameObject:
  m_Name: Waypoints
--- !u!4 &301
Transform:
  m_GameObject: {fileID: 300}
  m_Father: {fileID: 101}
"""

SIMPLE_PREFAB = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &10
GameObject:
  m_Name: {name}
--- !u!4 &11
Transform:
  m_GameObject: {{fileID: 10}}
  m_Father: {{fileID: 0}}
"""


def instance(fid, guid, root_t, parent_fid, name=None, rot_y=0):
    mods = ""
    if name:
        mods += """    - target: {fileID: 10, guid: %s,
        type: 3}
      propertyPath: m_Name
      value: %s
      objectReference: {fileID: 0}
""" % (guid, name)
    mods += """    - target: {fileID: %s, guid: %s,
        type: 3}
      propertyPath: m_LocalRotation.y
      value: %s
      objectReference: {fileID: 0}
""" % (root_t, guid, rot_y)
    return """--- !u!1001 &%s
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    m_TransformParent: {fileID: %s}
    m_Modifications:
%s    m_RemovedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: %s, type: 3}
""" % (fid, parent_fid, mods, guid)


def stripped(fid, src_fid, guid, inst):
    return """--- !u!4 &%s stripped
Transform:
  m_CorrespondingSourceObject: {fileID: %s, guid: %s,
    type: 3}
  m_PrefabInstance: {fileID: %s}
  m_PrefabAsset: {fileID: 0}
""" % (fid, src_fid, guid, inst)


FLOOR = """--- !u!1 &900
GameObject:
  m_Name: floor
  m_TagString: Floor
  m_Layer: {layer}
  m_StaticEditorFlags: {static}
"""


def make_project(tmp, scene_name="Test_PG", label="mymod", room_rot=0, ps_parent="chars",
                 two_starts=False, floor_layer=8, floor_static=4294967295, zone_parent="wps"):
    root = Path(tmp)
    (root / "ProjectSettings").mkdir(parents=True)
    (root / "ProjectSettings" / "TagManager.asset").write_text(
        "%YAML 1.1\n--- !u!78 &1\nTagManager:\n  tags:\n  - Floor\n  layers:\n  - Default\n  - TransparentFX\n"
        "  - Ignore Raycast\n  - \n  - Water\n  - UI\n  - \n  - \n  - Terrain\n  m_SortingLayers: []\n", encoding="utf-8")
    pf = root / "Assets" / "Library" / "Prefabs"
    pf.mkdir(parents=True)
    for name, guid, body in (("Room_Layout", G_ROOM, ROOM_PREFAB),
                             ("Player_Start", G_PS, SIMPLE_PREFAB.format(name="Player_Start")),
                             ("Zone_Camera", G_ZC, SIMPLE_PREFAB.format(name="Zone_Camera")),
                             ("SceneRules", G_SR, SIMPLE_PREFAB.format(name="SceneRules"))):
        (pf / (name + ".prefab")).write_text(body, encoding="utf-8")
        (pf / (name + ".prefab.meta")).write_text("fileFormatVersion: 2\nguid: %s\nPrefabImporter:\n" % guid, encoding="utf-8")
    scenes = root / "Assets" / "Scenes"
    scenes.mkdir()
    parents = {"chars": "501", "wps": "502", "root": "0"}
    s = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"
    s += instance("500", G_ROOM, "101", "0", rot_y=room_rot)
    s += stripped("501", "201", G_ROOM, "500") + stripped("502", "301", G_ROOM, "500")
    s += instance("600", G_PS, "11", parents[ps_parent])
    if two_starts:
        s += instance("610", G_PS, "11", parents["chars"], name="Player_Start (1)")
    s += instance("700", G_ZC, "11", parents[zone_parent])
    s += instance("800", G_SR, "11", "0")
    s += FLOOR.format(layer=floor_layer, static=floor_static)
    scene = scenes / (scene_name + ".unity")
    scene.write_text(s, encoding="utf-8")
    (scenes / (scene_name + ".unity.meta")).write_text(
        "fileFormatVersion: 2\nguid: %s\nDefaultImporter:\n  assetBundleName: %s\n" % ("e" * 32, label), encoding="utf-8")
    return scene


def run(argv):
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        mpn.main(argv)
    return json.loads(buf.getvalue())


class SceneLint(unittest.TestCase):
    def lint(self, **kw):
        with tempfile.TemporaryDirectory() as t:
            return run(["scene-lint", str(make_project(t, **kw))])

    def test_clean_scene(self):
        r = self.lint()
        self.assertEqual(r["errors"], [], r)
        self.assertTrue(r["info"]["playground_listed"])
        self.assertEqual(r["info"]["prefab_counts"]["Player_Start"], 1)

    def test_rotated_room(self):
        self.assertTrue(any(e.startswith("S03") for e in self.lint(room_rot=0.38)["errors"]))

    def test_player_start_parent(self):
        self.assertTrue(any(e.startswith("S02") for e in self.lint(ps_parent="root")["errors"]))

    def test_two_player_starts(self):
        self.assertTrue(any(e.startswith("S01") for e in self.lint(two_starts=True)["errors"]))

    def test_zone_camera_parent(self):
        self.assertTrue(any(e.startswith("S05") for e in self.lint(zone_parent="chars")["errors"]))

    def test_floor_rules(self):
        errs = self.lint(floor_layer=0, floor_static=0)["errors"]
        self.assertTrue(any(e.startswith("S08") for e in errs))
        self.assertTrue(any(e.startswith("S09") for e in errs))

    def test_missing_label(self):
        self.assertTrue(any(e.startswith("S13") for e in self.lint(label="")["errors"]))

    def test_no_pg_suffix_is_info_only(self):
        r = self.lint(scene_name="Stage01")
        self.assertEqual(r["errors"], [])
        self.assertFalse(r["info"]["playground_listed"])


class CheckMod(unittest.TestCase):
    def mod(self, files):
        t = tempfile.TemporaryDirectory()
        self.addCleanup(t.cleanup)
        d = Path(t.name) / "MyMod"
        d.mkdir()
        for name, body in files.items():
            p = d / name
            p.parent.mkdir(parents=True, exist_ok=True)
            p.write_text(body, encoding="utf-8")
        return run(["check-mod", str(d)])

    def test_good_mod(self):
        r = self.mod({
            "info.txt": '{"Name": "Green Blood", "Author": "me", "Description": "x", "Tags": ["Gore"],}',
            "cards.txt": '{"Type": "Character", "Filename": "Nexus1_Agent", "Root": "Nexus1_Agent", "Blood": {"r": 0, "g": 1, "b": 0, "a": 1}}\n'
                         '{"Type": "Character", "Filename": "Hank", "Root": "Hank"}',
            "more.json": '{"Melee": [{"Filename": "M_ModSword", "Root": "M_SwordLong", "ObjRef": "Machete_Sword"}]}',
            "weapons.mobj": "bundle",
        })
        self.assertEqual(r["errors"], [], r)
        self.assertEqual(r["info"]["cards"], 3)
        self.assertEqual(r["info"]["bundles"], ["weapons.mobj"])

    def test_curly_quotes(self):
        r = self.mod({"info.txt": "{“Name”: “X”, “Description”: “d”}", "a.txt": '{"Type": "Melee", "Filename": "A"}'})
        self.assertTrue(any("curly quotes" in e for e in r["errors"]))

    def test_curly_quotes_in_txt_card_only_warn(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}', "a.txt": "{“Type”: “Melee”, “Filename”: “A”}"})
        self.assertEqual(r["errors"], [], r)
        self.assertTrue(any("straightens" in w for w in r["warnings"]))
        self.assertEqual(r["info"]["cards"], 1)

    def test_info_needs_description(self):
        r = self.mod({"info.txt": '{"Name": "X"}', "a.txt": '{"Type": "Melee", "Filename": "A"}'})
        self.assertTrue(any("no Description" in e for e in r["errors"]))

    def test_info_json_accepted(self):
        r = self.mod({"info.json": '{"Name": "X", "Description": "d"}', "a.txt": '{"Type": "Melee", "Filename": "A"}'})
        self.assertEqual(r["errors"], [], r)
        self.assertEqual(r["info"]["cards"], 1)

    def test_bare_card_in_json_is_ignored_by_game(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}',
                      "hank.json": '{"Type": "Character", "Filename": "Hank", "Root": "Hank"}'})
        self.assertTrue(any("silently ignores" in e for e in r["errors"]), r)

    def test_unknown_json_list_and_txt_type(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}',
                      "a.json": '{"Characters": [{"Filename": "Hank"}]}',
                      "b.txt": '{"Type": "character", "Filename": "Hank"}'})
        self.assertTrue(any("did you mean Character" in w for w in r["warnings"]), r)
        self.assertTrue(any("case-sensitive" in e for e in r["errors"]), r)

    def test_event_type_any_case(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}', "e.txt": '{"Type": "EVENT", "Filename": "Ev1"}'})
        self.assertEqual(r["errors"], [], r)

    def test_misspelt_root(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}',
                      "a.txt": '{"Type": "Character", "Filename": "Hank", "Root": "Hnak"}\n'
                               '{"Type": "Character", "Filename": "Nexus1_Agent", "Root": "Nexus2_Agent"}'})
        ws = [w for w in r["warnings"] if "misspelling" in w]
        self.assertEqual(len(ws), 1, r)
        self.assertIn("Hnak", ws[0])

    def test_shorthand_needs_type(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}', "a.txt": '{"Filename": "A"}'})
        self.assertTrue(any("no Type" in e for e in r["errors"]))

    def test_strict_json(self):
        r = self.mod({"info.txt": '{"Name": "X", "Description": "d"}', "a.json": '{"Melee": [{"Filename": "A",}]}'})
        self.assertTrue(any("not valid JSON" in e for e in r["errors"]))

    def test_assembly_path(self):
        r = self.mod({"info.txt": '{"Name": "X", "AssemblyPath": "Managed/My.dll", "EntryType": "My.Entry", "HarmonyID": "me.x"}'})
        self.assertTrue(any("AssemblyPath" in e for e in r["errors"]))

    def test_dependencies_numeric(self):
        r = self.mod({"info.txt": '{"Name": "X", "Dependencies": ["OtherLocalMod"]}', "a.txt": '{"Type": "Melee", "Filename": "A"}'})
        self.assertTrue(any("Dependencies" in e for e in r["errors"]))

    def test_empty_mod(self):
        r = self.mod({"info.txt": '{"Name": "X"}'})
        self.assertTrue(any("would do nothing" in e for e in r["errors"]))


class Deploy(unittest.TestCase):
    def test_deploy_copies_and_refuses_overwrite(self):
        with tempfile.TemporaryDirectory() as t:
            src = Path(t) / "src" / "MyMod"
            src.mkdir(parents=True)
            (src / "info.txt").write_text('{"Name": "X", "Author": "a", "Description": "d"}', encoding="utf-8")
            (src / "a.txt").write_text('{"Type": "Melee", "Filename": "A"}', encoding="utf-8")
            data = Path(t) / "data"
            r = run(["--data-dir", str(data), "deploy", str(src)])
            self.assertTrue(r["deployed"], r)
            self.assertTrue((data / "Mods" / "MyMod" / "a.txt").is_file())
            r2 = run(["--data-dir", str(data), "deploy", str(src)])
            self.assertFalse(r2["deployed"])

    def test_log_reads_mods_log(self):
        # Line shapes follow the game's own mod log writer (build 25615280).
        with tempfile.TemporaryDirectory() as t:
            data = Path(t)
            (data / "Mods" / "MyMod").mkdir(parents=True)
            (data / "Mods" / "Other").mkdir()
            (data / "Mods" / "mods.log").write_text(
                "\t=== MOD LOG ===\n\nScanning for Local Mods...\n  > FOUND: C:/x/Mods/MyMod\n\nMods Enabled:\n"
                "  > (1) [LOCAL] C:/x/Mods/MyMod\n  > APPLIED HARMONY PATCH: me.mymod\n\nScanning for MadCards...\n"
                " > OVERRIDE: Hank (Character)  >> Priority (10/0) \n"
                " > FAILED: Hank_Green (Character)\n\n\t=== END OF LOG ===", encoding="utf-8")
            (data / "Mods" / "enabled.txt").write_text('{"Enabled":["MyMod"]}', encoding="utf-8")
            # Shapes from a real beta 2.20.a Player.log (2026-10-18).
            (data / "Player.log").write_text(
                "Initialize engine version: 2019.2.6f1\nGAME INITIALIZED: Beta v2.20.a\n"
                "Fallback handler could not load library D:/G/Madness Project Nexus_Data/Mono/data-000001FF4DDC45D0.dll\n"
                " System.NullReferenceException: Object reference not set to an instance of an object\n"
                "  at Controller_Player.SwapControllersWithSquadmate (System.Boolean overrideChecks) [0x00052] in <x>:0 \n"
                "  at Combatant_Base.KillMe (Target_Object inKiller, KillSource inSource) [0x001ea] in <x>:0 \n", encoding="utf-8")
            r = run(["--data-dir", str(data), "log"])
            self.assertEqual(r["mods_log"]["problem_count"], 1, r)
            self.assertEqual([(c["kind"], c["card"]) for c in r["mods_log"]["cards"]], [("OVERRIDE", "Hank"), ("FAILED", "Hank_Green")])
            self.assertEqual(r["player_log"]["problem_count"], 1, r)
            self.assertTrue(r["player_log"]["problems"][0]["stack"][0].startswith("at Controller_Player.SwapControllersWithSquadmate"), r)
            self.assertEqual(r["mods_log"]["loaded"][0]["source"], "LOCAL")
            self.assertEqual(r["mods_log"]["applied_harmony"], ["me.mymod"])
            self.assertTrue(r["game_version"]["mods_allowed"])
            self.assertTrue(any("Other" in n for n in r["notes"]), r)

    def test_log_flags_release_build(self):
        with tempfile.TemporaryDirectory() as t:
            data = Path(t)
            (data / "Player.log").write_text("GAME INITIALIZED: Release v1.11.a\nERROR: Shader Shader is not supported on this GPU\n", encoding="utf-8")
            r = run(["--data-dir", str(data), "log"])
            self.assertFalse(r["game_version"]["mods_allowed"])
            self.assertEqual(r["player_log"]["problem_count"], 0, r)
            self.assertTrue(any("Release build" in n for n in r["notes"]), r)


class Backup(unittest.TestCase):
    def test_backup_zips_data(self):
        with tempfile.TemporaryDirectory() as t:
            data = Path(t) / "Gibbing Tree" / "Madness Project Nexus"
            (data / "Mods").mkdir(parents=True)
            (data / "career0.sav").write_text("save", encoding="utf-8")
            (data / "Player.log").write_text("log", encoding="utf-8")
            r = run(["--data-dir", str(data), "backup", "--out", str(Path(t) / "bk")])
            self.assertTrue(r["ok"], r)
            self.assertEqual(r["files"], 1)
            self.assertTrue(Path(r["zip"]).is_file())


if __name__ == "__main__":
    unittest.main(verbosity=1)
