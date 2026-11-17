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
import geometry  # noqa: E402

G_ROOM = "a" * 32
G_PS = "b" * 32
G_ZC = "c" * 32
G_SR = "d" * 32
G_DOOR = "f" * 32
G_PG = "c" * 31 + "1"

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


def instance(fid, guid, root_t, parent_fid, name=None, rot_y=0, extra=None):
    mods = ""
    if extra:
        mods += """    - target: {fileID: 10, guid: %s,
        type: 3}
      propertyPath: %s
      value: %s
      objectReference: {fileID: 0}
""" % (guid, extra[0], extra[1])
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
                 two_starts=False, floor_layer=8, floor_static=4294967295, zone_parent="wps",
                 door_parent=None, booth=None):
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
                             ("SceneRules", G_SR, SIMPLE_PREFAB.format(name="SceneRules")),
                             ("PX_Door", G_DOOR, SIMPLE_PREFAB.format(name="PX_Door")),
                             ("PX_Playground", G_PG, SIMPLE_PREFAB.format(name="PX_Playground"))):
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
    if door_parent:
        s += instance("850", G_DOOR, "11", {"room": "500", "root": "0"}.get(door_parent, door_parent))
    if booth is not None:  # booth=True: ApplyWindowMask on, as the sample room ships it
        s += instance("860", G_PG, "11", "500", extra=("ApplyWindowMask", "1") if booth else None)
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

    def test_door_in_room(self):
        self.assertFalse(any(w.startswith("S14") for w in self.lint(door_parent="501")["warnings"]))

    def test_door_at_scene_root(self):
        self.assertTrue(any(w.startswith("S14") for w in self.lint(door_parent="root")["warnings"]))

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

    G_DOOR_CS, G_SQUAD_CS = "d" * 32, "f" * 32

    def door_lint(self, squad_refs, linked=False, disable=0):
        # plain (cloned) doors and a plain squad, found by their scripts' guids
        with tempfile.TemporaryDirectory() as t:
            scene = make_project(t)
            code = Path(t) / "Assets" / "Code"
            code.mkdir()
            for name, g in (("ProxyObj_Door", self.G_DOOR_CS), ("Marker_Squad", self.G_SQUAD_CS)):
                (code / (name + ".cs.meta")).write_text("fileFormatVersion: 2\nguid: %s\n" % g, encoding="utf-8")
            s = scene.read_text(encoding="utf-8")
            for fid, go, name in (("900", "901", "Door_Spare"), ("910", "911", "Door_Out")):
                s += "--- !u!1 &%s\nGameObject:\n  m_Name: %s\n" % (go, name)
                s += ("--- !u!114 &%s\nMonoBehaviour:\n  m_GameObject: {fileID: %s}\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
                      "  DisableSpawn: %d\n  LinkedDoor: {fileID: %s}\n  LinkedScene: \n") % (
                    fid, go, self.G_DOOR_CS, disable if name == "Door_Spare" else 0, "910" if linked or name == "Door_Out" else "0")
            lst = "\n" + "".join("  - {fileID: %s}\n" % r for r in squad_refs) if squad_refs else " []\n"
            s += ("--- !u!114 &950\nMonoBehaviour:\n  m_GameObject: {fileID: 0}\n  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
                  "  SpawnFromDoors:%s  SpawnRequirement: 0\n") % (self.G_SQUAD_CS, lst)
            scene.write_text(s, encoding="utf-8")
            return run(["scene-lint", str(scene)])

    def test_playground_booth_warns(self):
        r = self.lint(booth=True)
        w = [x for x in r["warnings"] if x.startswith("S16")]
        self.assertEqual(len(w), 1, r)
        self.assertIn("masked", w[0])
        self.assertEqual(r["errors"], [])
        self.assertNotIn("masked", [x for x in self.lint(booth=False)["warnings"] if x.startswith("S16")][0])
        self.assertFalse(any(x.startswith("S16") for x in self.lint()["warnings"]))

    def test_unused_spawn_door(self):
        r = self.door_lint(["999"])
        self.assertTrue(any(w.startswith("S15 door Door_Spare") for w in r["warnings"]), r)
        self.assertFalse(any("Door_Out" in w for w in r["warnings"]))  # linked doors are ways through, not spawns

    def test_used_disabled_or_linked_door_passes(self):
        for kw in ({"squad_refs": ["900"]}, {"squad_refs": ["999"], "disable": 1}, {"squad_refs": ["999"], "linked": True}):
            r = self.door_lint(**kw)
            self.assertFalse(any(w.startswith("S15") for w in r["warnings"]), (kw, r))

    def test_roomwide_squad_skips_s15(self):
        r = self.door_lint([])
        self.assertFalse(any(w.startswith("S15") for w in r["warnings"]))
        self.assertEqual(r["info"]["doors"]["roomwide_squads"], 1)


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


class BoxShareCheck(unittest.TestCase):
    CUBE = "  m_Mesh: {fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}\n"
    OBJ = "  m_Mesh: {fileID: 4300000, guid: 0123456789abcdef0123456789abcdef, type: 3}\n"

    def docs(self, cubes, others):
        bodies = [self.CUBE] * cubes + [self.OBJ] * others
        return {str(i): {"cls": 33, "stripped": False, "body": b} for i, b in enumerate(bodies)}

    def test_all_boxes_warns(self):
        w, info = [], {}
        mpn.box_share_check(self.docs(60, 2), w, info)
        self.assertTrue(w and w[0].startswith("S18"))
        self.assertEqual(info["mesh_counts"], {"builtin_cube": 60, "other": 2})

    def test_modelled_props_quiet(self):
        w, info = [], {}
        mpn.box_share_check(self.docs(60, 20), w, info)
        self.assertEqual(w, [])
        self.assertEqual(info["box_share"], 0.75)


class SampleShellCheck(unittest.TestCase):
    def docs(self, enabled):
        d = {}
        for i, (name, on) in enumerate(enabled):
            d["g%d" % i] = {"cls": 1, "stripped": False, "body": "  m_Name: %s\n" % name}
            d["r%d" % i] = {"cls": 23, "stripped": False, "body": "  m_GameObject: {fileID: g%d}\n  m_Enabled: %d\n" % (i, on)}
        return d

    def run_check(self, enabled):
        w, info = [], {}
        ref = lambda b, n: (b.split(n + ": {fileID: ")[1].split("}")[0], None)
        mpn.sample_shell_check(self.docs(enabled), w, info, mpn.field, ref)
        return w, info

    def test_drawn_walls_warn(self):
        w, info = self.run_check([("room_pg_room2_walls", 1), ("room_pg_room2_floor", 1)])
        self.assertTrue(w and w[0].startswith("S21"))

    def test_hidden_walls_quiet(self):
        w, info = self.run_check([("room_pg_room2_walls", 0), ("room_pg_room2_floor", 1)])
        self.assertEqual(w, [])
        self.assertEqual(info["sample_shell"]["hidden"], {"room_pg_room2_walls": 1})


class StagingCheck(unittest.TestCase):
    def inst(self, i, mods):
        body = "".join("  - target: {fileID: 1, guid: 0123456789abcdef0123456789abcdef, type: 3}\n"
                       "    propertyPath: %s\n    value: %s\n    objectReference: {fileID: %s}\n" % (p, v, r) for p, v, r in mods)
        return str(i), {"cls": 1001, "stripped": False, "body": body}

    def run_check(self, modsets):
        docs, inst = {}, []
        for i, mods in enumerate(modsets):
            fid, d = self.inst(i, mods)
            docs[fid] = d
            inst.append({"kind": "Char_Start", "name": "Char_%d" % i, "fid": fid})
        w, info = [], {}
        mpn.staging_check(docs, inst, w, info, mpn.mod_refs)
        return w, info

    def test_same_fight_warns(self):
        w, info = self.run_check([[("Alertness", "2", "0")]] * 6)
        self.assertTrue(w and w[0].startswith("S19"))
        self.assertEqual(info["enemy_staging"]["facings"], 1)

    def test_staged_quiet_and_counts(self):
        w, info = self.run_check([[("Alertness", "0", "0"), ("m_LocalRotation.y", "-0.7071", "0"), ("m_LocalRotation.w", "0.7071", "0")],
                                  [("AddBehaviors.Array.data[0]", "27", "0")], [("SpawnFromDoor", "", "55")]] + [[]] * 3)
        self.assertEqual(w, [])
        st = info["enemy_staging"]
        self.assertEqual((st["behaviors"], st["spawn_from_door"], st["facings"]), ({"Camper": 1}, 1, 2))

    def test_squad_units_counted(self):
        fid, d = self.inst(9, [("Wave.WaveUnits.Array.data[0].Total", "2", "0"), ("Wave.WaveUnits.Array.data[1].Total", "1", "0")])
        docs = {fid: d, "0": self.inst(0, [])[1]}
        inst = [{"kind": "Char_Start", "name": "Char_0", "fid": "0"}, {"kind": "Squad_Spawner", "name": "Squad_DoorWave", "fid": fid}]
        w, info = [], {}
        mpn.staging_check(docs, inst, w, info, mpn.mod_refs)
        st = info["enemy_staging"]
        self.assertEqual((st["squad_units"], st["total_with_backup"]), (3, 4))

    def test_fixed_in_place_warns(self):
        w, _ = self.run_check([[("FixedInPlace", "1", "0")]])
        self.assertTrue(w and w[0].startswith("S20"))


class ReliefCheck(unittest.TestCase):
    def docs(self, ys):
        d = {}
        for i, y in enumerate(ys):
            d["g%d" % i] = {"cls": 1, "stripped": False, "body": "  m_Layer: 13\n"}
            d["t%d" % i] = {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: %d}\n  m_LocalScale: {x: 1, y: %s, z: 1}\n" % (i, y)}
            d["c%d" % i] = {"cls": 65, "stripped": False, "body": "  m_GameObject: {fileID: %d}\n" % i}
        return {k.replace("g", ""): v for k, v in d.items()}

    def test_flat_map_warns(self):
        w, info = [], {}
        mpn.relief_check(self.docs(["1.2", "0.9"]), w, info, mpn.field, mpn.ref)
        self.assertTrue(w and w[0].startswith("S23"))
        self.assertEqual(info["solid_structures"]["count"], 2)

    def test_by_room(self):
        d = self.docs(["1.2", "4.1", "3.0"])
        d["90"] = {"cls": 1, "stripped": False, "body": "  m_Name: Room_05\n  m_Layer: 0\n"}
        d["91"] = {"cls": 1, "stripped": False, "body": "  m_Name: Room_06\n  m_Layer: 0\n"}
        d["800"] = {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: 90}\n  m_Father: {fileID: 0}\n"}
        d["801"] = {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: 91}\n  m_Father: {fileID: 0}\n"}
        for i, father in enumerate(["800", "800", "801"]):
            d["t%d" % i]["body"] += "  m_Father: {fileID: %s}\n" % father
        w, info = [], {}
        mpn.relief_check(d, w, info, mpn.field, mpn.ref)
        rooms = info["solid_structures"]["by_room"]
        self.assertEqual(rooms["Room_05"], {"count": 2, "tallest": 4.1, "taller_than_a_character": 1})
        self.assertEqual(rooms["Room_06"]["count"], 1)

    def test_by_room_prefab_instance(self):
        # room 1 kept as a prefab instance: its Transform is stripped and its name lives in the modifications
        d = self.docs(["1.2", "4.1"])
        d["700"] = {"cls": 4, "stripped": True, "body": "  m_PrefabInstance: {fileID: 600}\n"}
        d["600"] = {"cls": 1001, "stripped": False, "body": "  m_Modification:\n    m_TransformParent: {fileID: 0}\n"
                    "    - target: {fileID: 5, guid: abc, type: 3}\n      propertyPath: m_Name\n      value: Room_01\n"}
        for i in range(2):
            d["t%d" % i]["body"] += "  m_Father: {fileID: 700}\n"
        w, info = [], {}
        mpn.relief_check(d, w, info, mpn.field, mpn.ref)
        self.assertEqual(info["solid_structures"]["by_room"], {"Room_01": {"count": 2, "tallest": 4.1, "taller_than_a_character": 1}})

    def test_tall_structure_quiet(self):
        w, info = [], {}
        mpn.relief_check(self.docs(["1.2", "4.1"]), w, info, mpn.field, mpn.ref)
        self.assertEqual(w, [])
        self.assertEqual(info["solid_structures"]["taller_than_a_character"], 1)


class RaisedFloorCheck(unittest.TestCase):
    def docs(self, y, extra_layers=(), sy="1", room_y=None):
        # a ground floor at 0 and a Floor-tagged box at local y (scale y = sy), optionally both under a room at room_y
        d = {}
        for g, t, py, sc in (("1", "2", y, sy), ("3", "4", "0", "1")):
            d[g] = {"cls": 1, "stripped": False, "body": "  m_TagString: Floor\n  m_Layer: 8\n  m_Name: F%s\n" % g}
            d[t] = {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: %s}\n  m_LocalPosition: {x: 0, y: %s, z: 0}\n"
                    "  m_LocalScale: {x: 1, y: %s, z: 1}\n" % (g, py, sc)}
            if room_y is not None:
                d[t]["body"] += "  m_Father: {fileID: 90}\n"
        if room_y is not None:
            d["91"] = {"cls": 1, "stripped": False, "body": "  m_Name: Room_04\n"}
            d["90"] = {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: 91}\n  m_LocalPosition: {x: 900, y: %s, z: 0}\n" % room_y}
        for i, layer in enumerate(extra_layers):
            d["5%d" % i] = {"cls": 1, "stripped": False, "body": "  m_Layer: %s\n" % layer}
        return d

    def test_high_floor_needs_pathblock(self):
        info = {}
        mpn.raised_floor_check(self.docs("2.5", ("12",)), info, mpn.field, mpn.ref)
        self.assertIn("note", info["raised_floors"])

    def test_low_deck_with_ramp_quiet(self):
        # a 0.675 deck (centre 0.3375) under the 2-unit link gap needs a ramp, not PathBlock (Nexus Prominence zzc)
        info = {}
        mpn.raised_floor_check(self.docs("0.3375", ("12",), sy="0.675"), info, mpn.field, mpn.ref)
        self.assertEqual(info["raised_floors"]["count"], 1)
        self.assertNotIn("note", info["raised_floors"])

    def test_room_child_deck_found_by_world_height(self):
        # until 1.0.12 local Y > 1 missed a deck built as a child of its room
        info = {}
        mpn.raised_floor_check(self.docs("0.3375", ("12",), sy="0.675", room_y="6.44"), info, mpn.field, mpn.ref)
        self.assertEqual(info["raised_floors"]["floors"], ["Room_04 F1 rise 0.68"])

    def test_raised_floor_without_ramp_notes(self):
        info = {}
        mpn.raised_floor_check(self.docs("2.5"), info, mpn.field, mpn.ref)
        self.assertTrue(info["raised_floors"]["note"].startswith("S24"))

    def test_ramp_and_pathblock_quiet(self):
        info = {}
        mpn.raised_floor_check(self.docs("2.5", ("12", "25")), info, mpn.field, mpn.ref)
        self.assertNotIn("note", info["raised_floors"])

    def test_ground_floor_ignored(self):
        info = {}
        mpn.raised_floor_check(self.docs("0.09"), info, mpn.field, mpn.ref)
        self.assertEqual(info, {})


class SolidSlopeCheck(unittest.TestCase):
    def docs(self, layer="13", name="NX_DeckSlope", ramp=False):
        d = {"1": {"cls": 1, "stripped": False, "body": "  m_Layer: %s\n  m_Name: %s\n" % (layer, name)},
             "2": {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: 1}\n  m_LocalScale: {x: 3.6, y: 0.675, z: 3}\n"},
             "3": {"cls": 65, "stripped": False, "body": "  m_GameObject: {fileID: 1}\n"}}
        if ramp:
            d["4"] = {"cls": 1, "stripped": False, "body": "  m_Layer: 12\n  m_Name: NX_DeckRampCollider\n"}
        return d

    def test_solid_slope_without_ramp_warns(self):
        w, info = [], {}
        mpn.solid_slope_check(self.docs(), w, info, mpn.field, mpn.ref)
        self.assertEqual(info["solid_slopes"], ["NX_DeckSlope"])
        self.assertTrue(w and w[0].startswith("S25"))

    def test_scene_with_interactive_ramp_is_info_only(self):
        w, info = [], {}
        mpn.solid_slope_check(self.docs(ramp=True), w, info, mpn.field, mpn.ref)
        self.assertEqual(w, [])
        self.assertIn("solid_slopes", info)

    def test_other_names_and_layers_ignored(self):
        for kw in ({"name": "NX_FloorCrate"}, {"layer": "12"}):
            w, info = [], {}
            mpn.solid_slope_check(self.docs(**kw), w, info, mpn.field, mpn.ref)
            self.assertEqual((w, info), ([], {}))


class SquadTimingCheck(StagingCheck):
    def lint(self, squad_mods, placed=3):
        body = lambda m: self.inst(0, m)[1]["body"] + "  m_TransformParent: {fileID: 77}\n"
        docs, inst = {}, []
        for i in range(placed):
            docs[str(i)] = {"cls": 1001, "stripped": False, "body": body([])}
            inst.append({"kind": "Char_Start", "name": "Char_%d" % i, "fid": str(i)})
        docs["s"] = {"cls": 1001, "stripped": False, "body": body(squad_mods)}
        inst.append({"kind": "Squad_Spawner", "name": "Squad_DoorWave", "fid": "s"})
        e, w, info = [], [], {}
        mpn.squad_timing_check(docs, inst, e, w, info, mpn.mod_refs, mpn.ref)
        return e, w, info

    def test_build_zu_squad_errors(self):
        e, w, info = self.lint([("SecondsInRoomTilSpawn", "20", "0"), ("Wave.MaxSquadSize", "2", "0"),
                                ("Wave.MaxUnitCount", "4", "0"), ("Wave.WaveUnits.Array.data[0].Total", "2", "0")], placed=5)
        self.assertTrue(e and e[0].startswith("S22"))
        self.assertEqual(info["squad_timing"][0]["placed_with_it"], 5)

    def test_alarm_short_delay_quiet(self):
        e, w, _ = self.lint([("SecondsInRoomTilSpawn", "3", "0"), ("SpawnRequirement", "2", "0"),
                             ("Wave.MaxSquadSize", "2", "0"), ("Wave.MaxUnitCount", "5", "0"), ("Wave.WaveUnits.Array.data[0].Total", "2", "0")])
        self.assertEqual((e, w), ([], []))

    def test_capped_later_squads_warn(self):
        e, w, _ = self.lint([("Wave.MaxSquadSize", "2", "0"), ("Wave.MaxUnitCount", "4", "0"), ("Wave.WaveUnits.Array.data[0].Total", "4", "0")])
        self.assertEqual(e, [])
        self.assertTrue(w and "cap" in w[0])


class AmbientCheck(unittest.TestCase):
    def docs(self, cols):
        return {str(i): {"cls": 114, "stripped": False, "body": "  AmbientColor: {r: %s, g: %s, b: %s, a: 1}\n" % c} for i, c in enumerate(cols)}

    def test_same_ambient_warns(self):
        w, info = [], {}
        mpn.ambient_check(self.docs([(0.3, 0.2, 0.2)] * 4), w, info)
        self.assertTrue(w and w[0].startswith("S17"))
        self.assertEqual(len(info["room_ambient_luma"]), 4)

    def test_varied_ambient_quiet(self):
        w, info = [], {}
        mpn.ambient_check(self.docs([(0.3, 0.2, 0.2), (0.5, 0.1, 0.1), (0.2, 0.2, 0.2)]), w, info)
        self.assertEqual(w, [])


CUBE_MESH = "  m_Mesh: {fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}\n"


def box_docs(boxes, room="Room_01"):
    """Synthetic scene docs: a room root and one built-in cube per (name, (x, y, z), (sx, sy, sz)[, collider])."""
    d = {"1": {"cls": 1, "stripped": False, "body": "  m_Name: %s\n" % room},
         "2": {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: 1}\n  m_Father: {fileID: 0}\n"}}
    for i, b in enumerate(boxes):
        name, pos, scl = b[:3]
        g = str(100 + 10 * i)
        d[g] = {"cls": 1, "stripped": False, "body": "  m_Layer: 13\n  m_Name: %s\n  m_IsActive: 1\n" % name}
        d[str(101 + 10 * i)] = {"cls": 4, "stripped": False, "body": "  m_GameObject: {fileID: %s}\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n"
                                "  m_LocalPosition: {x: %s, y: %s, z: %s}\n  m_LocalScale: {x: %s, y: %s, z: %s}\n  m_Father: {fileID: 2}\n" % ((g,) + pos + scl)}
        d[str(102 + 10 * i)] = {"cls": 33, "stripped": False, "body": "  m_GameObject: {fileID: %s}\n" % g + CUBE_MESH}
        d[str(103 + 10 * i)] = {"cls": 23, "stripped": False, "body": "  m_GameObject: {fileID: %s}\n  m_Enabled: 1\n" % g}
        if len(b) > 3 and b[3]:
            d[str(104 + 10 * i)] = {"cls": 65, "stripped": False, "body": "  m_GameObject: {fileID: %s}\n  m_Enabled: 1\n"
                                    "  m_Size: {x: 1, y: 1, z: 1}\n  m_Center: {x: 0, y: 0, z: 0}\n" % g}
    return d


class CoplanarCheck(unittest.TestCase):
    def run_check(self, boxes):
        w, info = [], {}
        mpn.coplanar_check(box_docs(boxes), w, info, mpn.field, mpn.ref)
        return w, info

    def floor(self):
        return ("NX_Floor", (0, -0.5, 0), (20, 1, 20))

    def test_papers_at_one_height_warn(self):
        w, info = self.run_check([self.floor(), ("NX_Paper", (0, 0.012, 0), (0.5, 0.004, 0.6)), ("NX_Paper", (0.2, 0.012, 0.1), (0.5, 0.004, 0.6))])
        self.assertTrue(w and w[0].startswith("S26"))
        self.assertEqual(info["coplanar"]["decal_pairs"], 1)
        self.assertEqual(info["coplanar"]["by_room"], {"Room_01": 1})

    def test_lifted_paper_quiet(self):
        w, info = self.run_check([self.floor(), ("NX_Paper", (0, 0.012, 0), (0.5, 0.004, 0.6)), ("NX_Paper", (0.2, 0.016, 0.1), (0.5, 0.004, 0.6))])
        self.assertEqual(w, [])
        self.assertEqual(info["coplanar"]["pairs"], 0)

    def test_stripe_on_band_front_warns(self):
        # an upright stripe whose front face lies in the band's front face (Nexus Prominence zzg, room 5)
        w, info = self.run_check([self.floor(), ("NX_Band_Back", (0, 1.3, 9.94), (20, 0.55, 0.12)), ("NX_BandRise", (2, 2.5, 9.93), (0.75, 3, 0.1))])
        self.assertEqual([sorted((p["a"], p["b"])) for p in info["coplanar"]["worst"]], [["NX_BandRise", "NX_Band_Back"]])

    def test_touching_side_by_side_quiet(self):
        w, info = self.run_check([self.floor(), ("NX_A", (0, 0.5, 0), (1, 1, 1)), ("NX_B", (1, 0.5, 0), (1, 1, 1))])
        self.assertEqual(w, [])

    def test_overlap_area(self):
        sq = [(0, 0), (1, 0), (1, 1), (0, 1)]
        self.assertAlmostEqual(geometry.overlap_area(sq, [(0.5, 0.5), (1.5, 0.5), (1.5, 1.5), (0.5, 1.5)]), 0.25)
        self.assertAlmostEqual(geometry.overlap_area(sq, [(2, 2), (3, 2), (3, 3), (2, 3)]), 0.0)


class NarrowGapCheck(unittest.TestCase):
    def run_check(self, boxes):
        w, info = [], {}
        mpn.narrow_gap_check(box_docs(boxes), w, info, mpn.field, mpn.ref)
        return w, info

    def test_narrow_gap_warns(self):
        w, info = self.run_check([("NX_PylonA", (0, 1.5, 0), (3, 3, 3), True), ("NX_PylonB", (0, 1.5, 4.0), (3, 3, 3), True)])
        self.assertTrue(w and w[0].startswith("S27"))
        self.assertEqual(info["narrow_gaps"]["worst"][0]["gap"], 1.0)

    def test_wide_gap_quiet(self):
        w, info = self.run_check([("NX_PylonA", (0, 1.5, 0), (3, 3, 3), True), ("NX_PylonB", (0, 1.5, 6.0), (3, 3, 3), True)])
        self.assertEqual(w, [])

    def test_gap_filled_by_third_solid_quiet(self):
        w, info = self.run_check([("NX_BarricadeA", (-1.6, 0.6, 0), (2, 1.2, 0.8), True), ("NX_BarricadeB", (1.6, 0.6, 0), (2, 1.2, 0.8), True),
                                  ("NX_PillarPlinth", (0, 0.5, 0), (1.15, 1.0, 1.15), True)])
        self.assertEqual(w, [])


import hazards  # noqa: E402


def mods_body(mods):
    return "  m_Modification:\n    m_Modifications:\n" + "".join(
        "    - target: {fileID: 1, guid: %s, type: 3}\n      propertyPath: %s\n      value: %s\n      objectReference: {fileID: %s}\n"
        % ("a" * 32, p, v, o) for p, v, o in mods)


class SpikeHazardCheck(unittest.TestCase):
    def docs(self, with_hazard):
        d = {"10": {"cls": 1001, "stripped": False, "body": mods_body([("m_Name", "PX_Spikes_A", "0")])},
             "11": {"cls": 1, "stripped": True, "body": "  m_PrefabInstance: {fileID: 10}\n"},
             "20": {"cls": 1, "stripped": False, "body": "  m_Name: CO_Timer\n"},
             "21": {"cls": 114, "stripped": False, "body": "  m_GameObject: {fileID: 20}\n  SerialNumber: T_A\n  UpObjs:\n  - {fileID: 11}\n"
                    + ("  - {fileID: 30}\n" if with_hazard else "") + "  DownObjs: []\n"}}
        if with_hazard:
            d["30"] = {"cls": 1, "stripped": False, "body": "  m_Name: Blades\n"}
            d["31"] = {"cls": 114, "stripped": False, "body": "  m_GameObject: {fileID: 30}\n  contactDmg: 35\n"}
        return d, [{"kind": "PX_Togglable", "name": "PX_Spikes_A", "fid": "10"}]

    def test_shown_hidden_strip_errors(self):
        docs, inst = self.docs(False)
        e, info = [], {}
        hazards.spike_hazard_check(docs, inst, e, info, mpn.mod_refs, mpn.ref)
        self.assertTrue(e and e[0].startswith("S28"))

    def test_strip_with_hazard_quiet(self):
        docs, inst = self.docs(True)
        e, info = [], {}
        hazards.spike_hazard_check(docs, inst, e, info, mpn.mod_refs, mpn.ref)
        self.assertEqual(e, [])
        self.assertTrue(info["spike_drivers"][0]["hazard"])


class SpawnExpectCheck(unittest.TestCase):
    def docs(self, rappel, listed):
        d = {"40": {"cls": 1001, "stripped": False, "body": mods_body([("Type", "211" if rappel else "208", "0")])},
             "41": {"cls": 114, "stripped": True, "body": "  m_PrefabInstance: {fileID: 40}\n"},
             "50": {"cls": 1001, "stripped": False, "body": mods_body([("SpawnFromDoors.Array.data[0]", "", "41" if listed else "0")])}}
        return d, [{"kind": "PX_Door", "name": "PX_Spawn_Rappel", "fid": "40"}, {"kind": "Squad_Spawner", "name": "Squad_R", "fid": "50"}]

    def run_check(self, rappel, listed, expect=("Rapel",)):
        docs, inst = self.docs(rappel, listed)
        e, info = [], {}
        hazards.spawn_expect_check(docs, inst, e, info, mpn.mod_refs, list(expect))
        return e, info

    def test_missing_rappel_errors(self):
        e, _ = self.run_check(False, True)
        self.assertTrue(e and e[0].startswith("S29") and "none" in e[0])

    def test_unused_rappel_errors(self):
        e, _ = self.run_check(True, False)
        self.assertTrue(e and "no Squad_Spawner" in e[0])

    def test_used_rappel_quiet(self):
        e, info = self.run_check(True, True)
        self.assertEqual(e, [])
        self.assertEqual(info["expected_spawns"]["Rapel"], {"doors": 1, "used_by_squad": True})

    def test_no_expectation_quiet(self):
        e, info = self.run_check(False, False, expect=())
        self.assertEqual((e, info), ([], {}))


class NarrowGapTrigger(unittest.TestCase):
    def test_trigger_not_a_wall(self):
        docs = box_docs([("NX_PylonA", (0, 1.5, 0), (3, 3, 3), True), ("NX_Blades", (0, 1.5, 4.0), (3, 3, 3), True)])
        docs["114"]["body"] += "  m_IsTrigger: 1\n"
        w, info = [], {}
        mpn.narrow_gap_check(docs, w, info, mpn.field, mpn.ref)
        self.assertEqual(w, [])


if __name__ == "__main__":
    unittest.main(verbosity=1)
