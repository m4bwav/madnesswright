// Nexus Prominence (Hank Raid) Playground template builder for MPN SceneTools (Unity 2019.2.6f1).
// Copy this file into <SceneTools>/Assets/Editor/ and run: menu Madness > Templates > Build Nexus Prominence _PG.
// It copies SamplePlayground_PG into NexusProminence_PG, clones the sample room into ten rooms laid out
// after the "NEXUS: HANK RAID" collab (street checkpoint to control tower), chains their doors, paints the
// gray-and-red Nexus look, places Nexus enemies and sets the asset label. Build the .sobj afterwards from
// Madness > AssetBundle as usual. Re-running overwrites the generated scene (asks first).
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NexusProminenceBuilder
{
    const string SrcScene = "Assets/Scenes/SamplePlayground_PG.unity";
    const string DstScene = "Assets/Scenes/NexusProminence_PG.unity";
    const string MatDir = "Assets/Templates/NexusProminence";
    const string CharStart = "Assets/Library/Prefabs/Markers/Char_Start.prefab";
    const string BundleLabel = "nexusprominence";
    const float RoomSpacing = 300f;

    struct Foe { public CharacterTypes Type; public int Count; public Foe(CharacterTypes t, int n) { Type = t; Count = n; } }

    class RoomSpec
    {
        public string Name;
        public AlertStatus Alert;
        public ProxyObj_Cover.ProxyType[] Cover;
        public Foe[] Foes;
        public bool KeepLocker, KeepSnipers;
    }

    static readonly RoomSpec[] Rooms =
    {
        new RoomSpec { Name = "01_StreetCheckpoint", Alert = AlertStatus.Unaware,
            Cover = new[] { ProxyObj_Cover.ProxyType.Barrel, ProxyObj_Cover.ProxyType.Crate1 },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 2) } },
        new RoomSpec { Name = "02_WantedHall", Alert = AlertStatus.Unaware,
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall1 },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 2), new Foe(CharacterTypes.Nexus2_Scout, 1) } },
        new RoomSpec { Name = "03_ControlsOffice", Alert = AlertStatus.Unaware,
            Cover = new[] { ProxyObj_Cover.ProxyType.Desk1, ProxyObj_Cover.ProxyType.Desk2 },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 2), new Foe(CharacterTypes.Nexus3_Support, 1) } },
        new RoomSpec { Name = "04_CargoBay", Alert = AlertStatus.Hunting,
            Cover = new[] { ProxyObj_Cover.ProxyType.Crate2, ProxyObj_Cover.ProxyType.Crate3, ProxyObj_Cover.ProxyType.Pallet },
            Foes = new[] { new Foe(CharacterTypes.Nexus4_Riot__Guard, 2), new Foe(CharacterTypes.Nexus6_Soldat, 1) } },
        new RoomSpec { Name = "05_SniperLoft", Alert = AlertStatus.Hunting, KeepSnipers = true,
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall2 },
            Foes = new[] { new Foe(CharacterTypes.Nexus2_Scout, 2), new Foe(CharacterTypes.Nexus1_Agent, 1) } },
        new RoomSpec { Name = "06_ShutterGarage", Alert = AlertStatus.Hunting,
            Cover = new[] { ProxyObj_Cover.ProxyType.Crate4, ProxyObj_Cover.ProxyType.Barrel },
            Foes = new[] { new Foe(CharacterTypes.Nexus5_Engineer, 2), new Foe(CharacterTypes.Nexus1_Agent, 1) } },
        new RoomSpec { Name = "07_SpotlightCorridor", Alert = AlertStatus.Combat,
            Cover = new ProxyObj_Cover.ProxyType[0],
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 3) } },
        new RoomSpec { Name = "08_LockerRoom", Alert = AlertStatus.Combat, KeepLocker = true,
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall3 },
            Foes = new[] { new Foe(CharacterTypes.Nexus4_Riot__Guard, 1), new Foe(CharacterTypes.Nexus1_Agent, 2) } },
        new RoomSpec { Name = "09_BreakRoom", Alert = AlertStatus.Combat,
            Cover = new[] { ProxyObj_Cover.ProxyType.Table, ProxyObj_Cover.ProxyType.Counter },
            Foes = new[] { new Foe(CharacterTypes.Nexus3_Support, 1), new Foe(CharacterTypes.Nexus1_Agent, 2), new Foe(CharacterTypes.Nexus_Scientist, 1) } },
        new RoomSpec { Name = "10_ControlTower", Alert = AlertStatus.Combat,
            Cover = new[] { ProxyObj_Cover.ProxyType.Desk1, ProxyObj_Cover.ProxyType.Counter, ProxyObj_Cover.ProxyType.Desk2 },
            Foes = new[] { new Foe(CharacterTypes.Nexus6_Soldat, 2), new Foe(CharacterTypes.Nexus4_Riot__Guard, 1), new Foe(CharacterTypes.Nexus5_Engineer, 1) } },
    };

    [MenuItem("Madness/Templates/Build Nexus Prominence _PG")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SrcScene) == null) { Debug.LogError("NexusProminence: " + SrcScene + " not found; pull SceneTools."); return; }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DstScene) != null &&
            !EditorUtility.DisplayDialog("Nexus Prominence", DstScene + " exists. Overwrite it?", "Overwrite", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        AssetDatabase.DeleteAsset(DstScene);
        AssetDatabase.CopyAsset(SrcScene, DstScene);
        var scene = EditorSceneManager.OpenScene(DstScene, OpenSceneMode.Single);

        var mats = MakeMaterials();
        var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharStart);
        var template = Object.FindObjectsOfType<RoomObj>().OrderBy(r => r.transform.position.x).First();
        foreach (var extra in Object.FindObjectsOfType<RoomObj>()) if (extra != template) Object.DestroyImmediate(extra.gameObject);

        var built = new List<RoomObj>();
        for (int i = 0; i < Rooms.Length; i++)
        {
            RoomObj room = i == 0 ? template
                : Object.Instantiate(template.gameObject, template.transform.position + Vector3.right * RoomSpacing * i, template.transform.rotation).GetComponent<RoomObj>();
            room.gameObject.name = "Room_" + Rooms[i].Name;
            room.ForceStartingRoom = i == 0;
            if (i > 0) StripStartOnly(room);
            Dress(room, Rooms[i], i, mats, charPrefab);
            built.Add(room);
        }
        ChainDoors(built);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var importer = AssetImporter.GetAtPath(DstScene);
        importer.SetAssetBundleNameAndVariant(BundleLabel, "");
        AssetDatabase.SaveAssets();
        Debug.Log("NexusProminence: built " + built.Count + " rooms into " + DstScene + " (label '" + BundleLabel + "'). Select Room_01_StreetCheckpoint, press SYNCH VISUALS, check it, then run scene-lint and the AssetBundle build.");
    }

    // Player_Start and the Playground terminal live only in the first room.
    static void StripStartOnly(RoomObj room)
    {
        foreach (var p in room.GetComponentsInChildren<Marker_Player>(true)) Object.DestroyImmediate(p.gameObject);
        foreach (var p in room.GetComponentsInChildren<ProxyObj_Playground>(true)) Object.DestroyImmediate(p.gameObject);
    }

    static Dictionary<string, Material> MakeMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Templates")) AssetDatabase.CreateFolder("Assets", "Templates");
        if (!AssetDatabase.IsValidFolder(MatDir)) AssetDatabase.CreateFolder("Assets/Templates", "NexusProminence");
        var d = new Dictionary<string, Material>
        {
            { "wall", Mat("NX_Wall", new Color(0.36f, 0.33f, 0.34f), Color.black) },
            { "floor", Mat("NX_Floor", new Color(0.24f, 0.22f, 0.23f), Color.black) },
            { "red", Mat("NX_RedStripe", new Color(0.85f, 0.05f, 0.05f), new Color(0.6f, 0f, 0f)) },
            { "dark", Mat("NX_DarkTrim", new Color(0.12f, 0.11f, 0.12f), Color.black) },
        };
        return d;
    }

    static Material Mat(string name, Color c, Color emission)
    {
        string path = MatDir + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, path); }
        m.color = c;
        m.SetFloat("_Glossiness", 0.1f);
        if (emission != Color.black) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission); }
        EditorUtility.SetDirty(m);
        return m;
    }

    static Bounds FloorBounds(RoomObj room)
    {
        var floors = room.GetComponentsInChildren<Collider>(true).Where(c => c.CompareTag("Floor")).ToArray();
        if (floors.Length == 0) floors = room.GetComponentsInChildren<Collider>(true);
        var b = floors[0].bounds;
        foreach (var c in floors) b.Encapsulate(c.bounds);
        return b;
    }

    static void Dress(RoomObj room, RoomSpec spec, int index, Dictionary<string, Material> mats, GameObject charPrefab)
    {
        // Look: gray walls, darker floor, red light and ambient (the collab's palette).
        foreach (var r in room.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (r.GetComponentInParent<SceneObj>() != null && r.GetComponentInParent<SceneObj>() != room) continue; // leave proxies and markers
            var col = r.GetComponent<Collider>();
            bool isFloor = col != null && col.CompareTag("Floor");
            r.sharedMaterial = isFloor ? mats["floor"] : mats["wall"];
        }
        room.AmbientColor = new Color(0.42f, 0.22f, 0.22f);
        room.FogColor = new Color(0.25f, 0.02f, 0.02f, 1f);
        foreach (var l in room.GetComponentsInChildren<Light>(true))
        {
            l.color = index == Rooms.Length - 1 ? new Color(1f, 0.35f, 0.35f) : new Color(1f, 0.62f, 0.6f);
        }

        var fb = FloorBounds(room);
        float floorTop = fb.max.y;
        var layout = room.transform.Find("Layout") ?? room.transform;
        // Signature waist-high red band on the back wall and a red kick strip at its foot.
        Strip(layout, "NX_WallBand", new Vector3(fb.center.x, floorTop + 0.75f, fb.max.z - 0.02f), new Vector3(fb.size.x, 0.22f, 0.04f), mats["red"]);
        Strip(layout, "NX_KickStrip", new Vector3(fb.center.x, floorTop + 0.04f, fb.max.z - 0.02f), new Vector3(fb.size.x, 0.08f, 0.04f), mats["red"]);
        Strip(layout, "NX_FloorEdge", new Vector3(fb.center.x, floorTop + 0.005f, fb.min.z + 0.15f), new Vector3(fb.size.x, 0.01f, 0.12f), mats["dark"]);

        // Cover: reuse the room's PX_Cover objects, add or remove to match the spec.
        var covers = room.GetComponentsInChildren<ProxyObj_Cover>(true).ToList();
        if (covers.Count > 0)
        {
            while (covers.Count < spec.Cover.Length)
                covers.Add(Object.Instantiate(covers[0].gameObject, covers[0].transform.parent).GetComponent<ProxyObj_Cover>());
            for (int c = 0; c < covers.Count; c++)
            {
                if (c >= spec.Cover.Length) { Object.DestroyImmediate(covers[c].gameObject); continue; }
                covers[c].Type = spec.Cover[c];
                var p = covers[c].transform.position;
                covers[c].transform.position = new Vector3(Mathf.Lerp(fb.min.x, fb.max.x, (c + 1f) / (spec.Cover.Length + 1f)), p.y, p.z);
                covers[c].gameObject.name = "PX_Cover_" + spec.Cover[c];
                EditorUtility.SetDirty(covers[c]);
            }
        }
        if (!spec.KeepLocker) foreach (var l in room.GetComponentsInChildren<ProxyObj_Locker>(true)) Object.DestroyImmediate(l.gameObject);
        if (!spec.KeepSnipers) foreach (var s in room.GetComponentsInChildren<Marker_Waypoint>(true).Where(w => w.gameObject.name.StartsWith("Sniperpoint"))) Object.DestroyImmediate(s.gameObject);

        // Enemies: Nexus faction, spread across the floor right of centre (the player enters from the left).
        if (charPrefab == null) { Debug.LogWarning("NexusProminence: " + CharStart + " missing; no enemies placed."); return; }
        var folder = room.CharacterFolder != null ? room.CharacterFolder.transform : room.transform;
        int total = spec.Foes.Sum(f => f.Count), n = 0;
        foreach (var foe in spec.Foes)
            for (int k = 0; k < foe.Count; k++, n++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab, room.gameObject.scene);
                go.transform.SetParent(folder, false);
                float t = total == 1 ? 0.7f : Mathf.Lerp(0.4f, 0.9f, n / (float)(total - 1));
                go.transform.position = new Vector3(Mathf.Lerp(fb.min.x, fb.max.x, t), floorTop, fb.center.z);
                var m = go.GetComponent<Marker_Character>();
                m.Character = foe.Type;
                m.Faction = Factions.Nexus;
                m.Alertness = spec.Alert;
                m.SerialNumber = "NXP_" + spec.Name + "_" + n;
                go.name = "Char_" + foe.Type + "_" + n;
                EditorUtility.SetDirty(m);
            }
        EditorUtility.SetDirty(room);
    }

    static void Strip(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(s.GetComponent<Collider>());
        s.name = name;
        s.transform.SetParent(parent, true);
        s.transform.position = pos;
        s.transform.localScale = size;
        s.GetComponent<MeshRenderer>().sharedMaterial = mat;
        s.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        s.isStatic = true;
    }

    // Rightmost door of room k leads to the leftmost door of room k+1; the other doors stay spawn doors.
    static void ChainDoors(List<RoomObj> rooms)
    {
        for (int k = 0; k + 1 < rooms.Count; k++)
        {
            var a = rooms[k].GetComponentsInChildren<ProxyObj_Door>(true).OrderBy(d => d.transform.position.x).LastOrDefault();
            var b = rooms[k + 1].GetComponentsInChildren<ProxyObj_Door>(true).OrderBy(d => d.transform.position.x).FirstOrDefault();
            if (a == null || b == null) { Debug.LogWarning("NexusProminence: no door to link between " + rooms[k].name + " and " + rooms[k + 1].name); continue; }
            a.LinkedDoor = b; b.LinkedDoor = a;
            a.DisableSpawn = b.DisableSpawn = true;
            a.SerialNumber = "NXP_door_" + k + "_out"; b.SerialNumber = "NXP_door_" + (k + 1) + "_in";
            a.gameObject.name = "PX_Door_to_" + rooms[k + 1].name; b.gameObject.name = "PX_Door_from_" + rooms[k].name;
            EditorUtility.SetDirty(a); EditorUtility.SetDirty(b);
        }
    }
}
