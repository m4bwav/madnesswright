"""Tests for skills/madnesswright-modtools/scripts/decomp.py. Run: python tests/test_decomp.py

The decompile folder is synthetic: a few made-up classes shaped like ILSpy output,
so no decompiled game code is stored in this repository.
"""
import contextlib
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "skills" / "madnesswright-modtools" / "scripts"))
import decomp  # noqa: E402

FILES = {
    "Options.cs": """public class Options
{
\tpublic static KeyCode key_Walk = KeyCode.CapsLock;
\tpublic static KeyCode key_Stats = KeyCode.P;
}
""",
    "DevConsole.cs": """public class DevConsole : MonoBehaviour
{
\tprivate void Start()
\t{
\t\tenabled = Game.IsDebugMode() && Debug.isDebugBuild;
\t}
\tprivate void Update()
\t{
\t\tif (Input.GetKeyDown(KeyCode.F1)) { }
\t\tif (Input.GetKeyDown(KeyCode.Delete)) { }
\t}
}
""",
    "TestTools.cs": """public class TestTools : MonoBehaviour
{
\tpublic static void Create()
\t{
\t\tif (LaunchParameters_Manager.HaveParameter(LaunchParameters_Manager.Params.devtools)) { }
\t}
\tprivate void Update()
\t{
\t\tif (Input.GetKeyDown(KeyCode.Insert)) { }
\t}
}
""",
    "Player.cs": """public class Player : Controller
{
\tpublic static Player PlayerController;
\tprotected override void RunAttacks()
\t{
\t\tif (Input.GetKeyDown(KeyCode.K)) { }
\t\tif (Input.GetButtonDown("Menu Cancel")) { }
\t\tHelpers.Unclaim(this);
\t}
\tpublic static bool SetTarget(Controller C, Target inTarget)
\t{
\t\treturn true;
\t}
}
""",
    "Sub/Helpers.cs": """public static class Helpers
{
\tpublic static void Unclaim(Controller c)
\t{
\t}
\tprivate static void Other()
\t{
\t\tUnclaim(null);
\t}
}
""",
}


def run(*argv):
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        decomp.main(list(argv))
    return json.loads(buf.getvalue())


class DecompTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.tmp = tempfile.TemporaryDirectory()
        cls.dir = Path(cls.tmp.name)
        for name, text in FILES.items():
            p = cls.dir / name
            p.parent.mkdir(parents=True, exist_ok=True)
            p.write_text(text, encoding="utf-8")

    @classmethod
    def tearDownClass(cls):
        cls.tmp.cleanup()

    def test_keys_marks_dev_only_and_lists_clashes(self):
        r = run("keys", str(self.dir), "--keys", "F1,Delete,Insert,K,CapsLock,P,U")
        self.assertTrue(all(h["dev_only"] for h in r["keys"]["F1"]))
        self.assertTrue(all(h["dev_only"] for h in r["keys"]["Insert"]), "the -devtools launch gate counts as dev-only")
        self.assertEqual(r["clashes"]["K"]["read_in"], ["Player.cs"])
        self.assertEqual(r["clashes"]["CapsLock"]["rebindable_default"], ["key_Walk"])
        self.assertIn("P", r["clashes"])
        for k in ("f1", "delete", "insert", "u"):
            self.assertIn(k, r["free"])
        self.assertNotIn("k", r["free"])
        self.assertIn("Menu Cancel", r["buttons"])

    def test_find_declarations_not_calls(self):
        r = run("find", str(self.dir), "Unclaim")
        self.assertEqual([(h["file"], h["line"]) for h in r["declarations"]], [("Sub/Helpers.cs", 3)])
        r = run("find", str(self.dir), "Player")
        self.assertEqual(r["declarations"][0]["file"], "Player.cs")
        r = run("find", str(self.dir), "SetTarget")
        self.assertIn("Controller C, Target inTarget", r["declarations"][0]["text"])

    def test_callers_skip_declarations(self):
        r = run("callers", str(self.dir), "Unclaim")
        self.assertEqual(sorted(h["file"] for h in r["callers"]), ["Player.cs", "Sub/Helpers.cs"])
        self.assertFalse(any("public static void" in h["text"] for h in r["callers"]))

    def test_max_truncates(self):
        r = run("callers", str(self.dir), "Unclaim", "--max", "1")
        self.assertTrue(r["truncated"])
        self.assertEqual(len(r["callers"]), 1)

    def test_missing_folder_fails_soft(self):
        r = run("keys", str(self.dir / "nope"))
        self.assertTrue(r["errors"])
        r = run("decompile", str(self.dir / "nope.dll"), "--out", str(self.dir / "out"))
        self.assertTrue(r["errors"])
        self.assertFalse((self.dir / "out").exists())


if __name__ == "__main__":
    unittest.main(verbosity=1)
