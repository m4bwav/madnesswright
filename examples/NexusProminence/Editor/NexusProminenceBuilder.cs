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
        public float Width = 1f, Depth = 1f; // stretch of the sample box: rooms are not all the same square
        public Vector2[] CoverAt;           // cover spots as (0..1 across, 0..1 from front to back); default spread
    }

    static readonly RoomSpec[] Rooms =
    {
        new RoomSpec { Name = "01_StreetCheckpoint", Width = 1.5f, Depth = 1f, Alert = AlertStatus.Unaware, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Barrel, ProxyObj_Cover.ProxyType.Crate1, ProxyObj_Cover.ProxyType.Barrel, ProxyObj_Cover.ProxyType.Crate2 },
            CoverAt = new[] { new Vector2(0.18f, 0.3f), new Vector2(0.45f, 0.55f), new Vector2(0.62f, 0.25f), new Vector2(0.8f, 0.6f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 3) } },
        new RoomSpec { Name = "02_WantedHall", Width = 1.3f, Depth = 1f, Alert = AlertStatus.Unaware, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall1, ProxyObj_Cover.ProxyType.Crate1, ProxyObj_Cover.ProxyType.Barrel },
            CoverAt = new[] { new Vector2(0.35f, 0.4f), new Vector2(0.6f, 0.65f), new Vector2(0.85f, 0.3f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 3), new Foe(CharacterTypes.Nexus2_Scout, 1) } },
        new RoomSpec { Name = "03_ControlsOffice", Width = 1.0f, Depth = 1f, Alert = AlertStatus.Unaware, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Desk1, ProxyObj_Cover.ProxyType.Desk2, ProxyObj_Cover.ProxyType.Desk1 },
            CoverAt = new[] { new Vector2(0.25f, 0.45f), new Vector2(0.55f, 0.3f), new Vector2(0.8f, 0.6f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 2), new Foe(CharacterTypes.Nexus3_Support, 1) } },
        new RoomSpec { Name = "04_CargoBay", Width = 1.6f, Depth = 1.15f, Alert = AlertStatus.Hunting, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Crate2, ProxyObj_Cover.ProxyType.Crate3, ProxyObj_Cover.ProxyType.Pallet, ProxyObj_Cover.ProxyType.Crate4, ProxyObj_Cover.ProxyType.Crate1, ProxyObj_Cover.ProxyType.Crate3 },
            CoverAt = new[] { new Vector2(0.15f, 0.6f), new Vector2(0.3f, 0.3f), new Vector2(0.45f, 0.55f), new Vector2(0.6f, 0.25f), new Vector2(0.72f, 0.7f), new Vector2(0.88f, 0.4f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus4_Riot__Guard, 2), new Foe(CharacterTypes.Nexus1_Agent, 2), new Foe(CharacterTypes.Nexus6_Soldat, 1) } },
        new RoomSpec { Name = "05_SniperLoft", Width = 1.2f, Depth = 1f, Alert = AlertStatus.Hunting, KeepSnipers = true, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall2, ProxyObj_Cover.ProxyType.Crate1, ProxyObj_Cover.ProxyType.Barrel },
            CoverAt = new[] { new Vector2(0.3f, 0.4f), new Vector2(0.6f, 0.6f), new Vector2(0.8f, 0.3f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus2_Scout, 2), new Foe(CharacterTypes.Nexus1_Agent, 2) } },
        new RoomSpec { Name = "06_ShutterGarage", Width = 1.35f, Depth = 1f, Alert = AlertStatus.Hunting, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Crate4, ProxyObj_Cover.ProxyType.Barrel, ProxyObj_Cover.ProxyType.Pallet, ProxyObj_Cover.ProxyType.Crate2 },
            CoverAt = new[] { new Vector2(0.2f, 0.4f), new Vector2(0.4f, 0.7f), new Vector2(0.6f, 0.35f), new Vector2(0.82f, 0.6f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus5_Engineer, 2), new Foe(CharacterTypes.Nexus1_Agent, 2) } },
        new RoomSpec { Name = "07_SpotlightCorridor", Width = 2.0f, Depth = 0.75f, Alert = AlertStatus.Combat, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall1, ProxyObj_Cover.ProxyType.Barrel, ProxyObj_Cover.ProxyType.Wall3 },
            CoverAt = new[] { new Vector2(0.3f, 0.5f), new Vector2(0.55f, 0.5f), new Vector2(0.78f, 0.5f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus1_Agent, 4), new Foe(CharacterTypes.Nexus4_Riot__Guard, 1) } },
        new RoomSpec { Name = "08_LockerRoom", Width = 0.85f, Depth = 0.85f, Alert = AlertStatus.Combat, KeepLocker = true, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Wall3, ProxyObj_Cover.ProxyType.Table },
            CoverAt = new[] { new Vector2(0.4f, 0.45f), new Vector2(0.7f, 0.6f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus4_Riot__Guard, 1), new Foe(CharacterTypes.Nexus1_Agent, 2) } },
        new RoomSpec { Name = "09_BreakRoom", Width = 1.15f, Depth = 1f, Alert = AlertStatus.Combat, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Table, ProxyObj_Cover.ProxyType.Table, ProxyObj_Cover.ProxyType.Counter, ProxyObj_Cover.ProxyType.Barrel },
            CoverAt = new[] { new Vector2(0.2f, 0.4f), new Vector2(0.45f, 0.55f), new Vector2(0.7f, 0.35f), new Vector2(0.9f, 0.65f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus3_Support, 1), new Foe(CharacterTypes.Nexus1_Agent, 3), new Foe(CharacterTypes.Nexus_Scientist, 1) } },
        new RoomSpec { Name = "10_ControlTower", Width = 1.5f, Depth = 1.1f, Alert = AlertStatus.Combat, 
            Cover = new[] { ProxyObj_Cover.ProxyType.Desk1, ProxyObj_Cover.ProxyType.Counter, ProxyObj_Cover.ProxyType.Desk2, ProxyObj_Cover.ProxyType.Desk1, ProxyObj_Cover.ProxyType.Crate1 },
            CoverAt = new[] { new Vector2(0.2f, 0.35f), new Vector2(0.4f, 0.6f), new Vector2(0.6f, 0.3f), new Vector2(0.78f, 0.55f), new Vector2(0.9f, 0.25f) },
            Foes = new[] { new Foe(CharacterTypes.Nexus6_Soldat, 2), new Foe(CharacterTypes.Nexus4_Riot__Guard, 2), new Foe(CharacterTypes.Nexus5_Engineer, 1), new Foe(CharacterTypes.Nexus1_Agent, 2) } },
    };

    [MenuItem("Madness/Templates/Build Nexus Prominence _PG")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SrcScene) == null) { Debug.LogError("NexusProminence: " + SrcScene + " not found; pull SceneTools."); return; }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DstScene) != null &&
            !Application.isBatchMode && !EditorUtility.DisplayDialog("Nexus Prominence", DstScene + " exists. Overwrite it?", "Overwrite", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        AssetDatabase.DeleteAsset(DstScene);
        AssetDatabase.CopyAsset(SrcScene, DstScene);
        var scene = EditorSceneManager.OpenScene(DstScene, OpenSceneMode.Single);

        var mats = MakeMaterials();
        var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharStart);
        var template = Object.FindObjectsOfType<RoomObj>().OrderBy(r => r.transform.position.x).First();
        foreach (var extra in Object.FindObjectsOfType<RoomObj>()) if (extra != template) Object.DestroyImmediate(extra.gameObject);

        var built = new List<RoomObj>();
        // Clone every room from the bare template first, then dress each one; cloning a dressed room
        // would copy its set, enemies and markers into the next one.
        for (int i = 0; i < Rooms.Length; i++)
            built.Add(i == 0 ? template
                : Object.Instantiate(template.gameObject, template.transform.position + Vector3.right * RoomSpacing * i, template.transform.rotation).GetComponent<RoomObj>());
        for (int i = 0; i < Rooms.Length; i++)
        {
            var room = built[i];
            Stretch(room, Rooms[i].Width, Rooms[i].Depth);
            room.gameObject.name = "Room_" + Rooms[i].Name;
            room.ForceStartingRoom = i == 0;
            if (i > 0) StripStartOnly(room);
            Dress(room, Rooms[i], i, mats, charPrefab);
        }
        ChainDoors(built);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        var importer = AssetImporter.GetAtPath(DstScene);
        importer.SetAssetBundleNameAndVariant(BundleLabel, "");
        AssetDatabase.SaveAssets();
        Debug.Log("NexusProminence: built " + built.Count + " rooms into " + DstScene + " (label '" + BundleLabel + "'). Select Room_01_StreetCheckpoint, press SYNCH VISUALS, check it, then run scene-lint and the AssetBundle build.");
    }

    // Stretch the cloned sample box: room meshes scale about the left wall (x) and the back wall (z);
    // everything else that sits in the room (doors, cover, markers, lights, camera borders, camera) moves
    // with the stretch but keeps its size. The camera keeps its distance (only x moves).
    static void Stretch(RoomObj room, float sx, float sz)
    {
        if (Mathf.Approximately(sx, 1f) && Mathf.Approximately(sz, 1f)) return;
        Physics.SyncTransforms();
        var fb = FloorBounds(room);
        float ax = fb.min.x, az = fb.max.z;
        int scaled = 0, moved = 0;
        StretchWalk(room.transform, room, ax, az, sx, sz, ref scaled, ref moved);
        if (room.CameraPoint != null)
        {
            var t = room.CameraPoint.transform; var p = t.position;
            t.position = new Vector3(ax + (p.x - ax) * sx, p.y, p.z);
        }
        Debug.Log("NexusProminence: stretched " + room.name + " x" + sx + " z" + sz + " (" + scaled + " meshes scaled, " + moved + " objects moved)");
    }

    static void StretchWalk(Transform parent, RoomObj room, float ax, float az, float sx, float sz, ref int scaled, ref int moved)
    {
        foreach (Transform c in parent)
        {
            if (room.CameraPoint != null && c == room.CameraPoint.transform) continue;
            var p = c.position;
            var np = new Vector3(ax + (p.x - ax) * sx, p.y, az + (p.z - az) * sz);
            bool geometry = (c.GetComponent<MeshRenderer>() != null || c.GetComponent<Collider>() != null) && c.GetComponent<SceneObj>() == null && c.GetComponent<Camera>() == null && c.childCount == 0;
            if (geometry)
            {
                c.position = np;
                var s = c.localScale;
                // imported meshes are often rotated (Blender's -90 on X): scale each local axis by the
                // stretch of the world axis it lines up with
                System.Func<Vector3, float> k = d => Mathf.Abs(d.x) > 0.7f ? sx : Mathf.Abs(d.z) > 0.7f ? sz : 1f;
                c.localScale = new Vector3(s.x * k(c.right), s.y * k(c.up), s.z * k(c.forward));
                scaled++;
            }
            else if (c.GetComponent<SceneObj>() != null || c.GetComponent<Light>() != null || c.GetComponent<Camera>() != null || c.childCount == 0)
            {
                c.position = np; moved++;
            }
            else StretchWalk(c, room, ax, az, sx, sz, ref scaled, ref moved);
        }
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
            { "wall", Mat("NX_Wall", new Color(0.27f, 0.26f, 0.27f), Color.black) },
            { "floor", Mat("NX_Floor", new Color(0.40f, 0.39f, 0.40f), Color.black) },
            { "panel", Mat("NX_Panel", new Color(0.33f, 0.32f, 0.33f), Color.black) },
            { "red", Mat("NX_RedStripe", new Color(0.85f, 0.04f, 0.04f), new Color(0.55f, 0f, 0f)) },
            { "dark", Mat("NX_DarkTrim", new Color(0.11f, 0.10f, 0.11f), Color.black) },
            { "black", Mat("NX_Black", new Color(0.02f, 0.02f, 0.02f), Color.black) },
            { "steel", Mat("NX_Steel", new Color(0.52f, 0.51f, 0.52f), Color.black) },
            { "screen", Mat("NX_Screen", new Color(0.9f, 0.1f, 0.1f), new Color(1.0f, 0.12f, 0.12f)) },
            { "sky", Mat("NX_Sky", new Color(0.75f, 0.05f, 0.05f), new Color(0.65f, 0.03f, 0.03f)) },
            { "lit", Mat("NX_LitWindow", new Color(1f, 0.8f, 0.8f), new Color(1f, 0.7f, 0.7f)) },
            { "asphalt", Mat("NX_Asphalt", new Color(0.22f, 0.21f, 0.22f), Color.black) },
            { "stain", Mat("NX_Stain", new Color(0.12f, 0.11f, 0.11f), Color.black) },
            { "white", Mat("NX_White", new Color(0.86f, 0.86f, 0.86f), Color.black) },
            { "glass", Mat("NX_Glass", new Color(0.55f, 0.08f, 0.10f), new Color(0.45f, 0.05f, 0.08f)) },
            { "beam", BeamMat() },
        };
        return d;
    }

    static Material Mat(string name, Color c, Color emission)
    {
        string path = MatDir + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, path); }
        m.shader = Shader.Find("Standard");
        m.color = c;
        m.SetFloat("_Glossiness", 0.08f);
        if (emission != Color.black) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
        else { m.DisableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Color.black); }
        EditorUtility.SetDirty(m);
        return m;
    }

    // Soft red light cone under the ceiling lamps (additive, so it brightens what is behind it).
    static Material BeamMat()
    {
        string path = MatDir + "/NX_Beam.mat";
        var sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive") ?? Shader.Find("Standard");
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
        m.shader = sh;
        if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", new Color(0.55f, 0.04f, 0.04f, 0.5f));
        else m.color = new Color(0.6f, 0.05f, 0.05f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static Mesh beamMesh;
    static Mesh BeamMesh()
    {
        if (beamMesh != null) return beamMesh;
        string path = MatDir + "/NX_BeamMesh.asset";
        beamMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (beamMesh != null) return beamMesh;
        beamMesh = new Mesh { name = "NX_BeamMesh" };
        // Trapezoid in XY: narrow top at y=1, wide bottom at y=0; both faces.
        beamMesh.vertices = new[] { new Vector3(-0.15f, 1, 0), new Vector3(0.15f, 1, 0), new Vector3(0.5f, 0, 0), new Vector3(-0.5f, 0, 0) };
        beamMesh.uv = new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        beamMesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 };
        beamMesh.RecalculateNormals();
        beamMesh.RecalculateBounds();
        AssetDatabase.CreateAsset(beamMesh, path);
        return beamMesh;
    }

    static Bounds FloorBounds(RoomObj room)
    {
        var floors = room.GetComponentsInChildren<Collider>(true).Where(c => c.CompareTag("Floor")).ToArray();
        if (floors.Length == 0) floors = room.GetComponentsInChildren<Collider>(true);
        var b = floors[0].bounds;
        foreach (var c in floors) b.Encapsulate(c.bounds);
        return b;
    }

    // Room frame shared by every room: x0..x1 side walls, zb back wall, zf the open (camera) side, y floor top.
    class Frame
    {
        public Transform Parent; public float x0, x1, zb, zf, y; public Dictionary<string, Material> M;
        public float W { get { return x1 - x0; } }
        public float D { get { return zb - zf; } }
        public float Cx { get { return (x0 + x1) * 0.5f; } }
        public float Cz { get { return (zb + zf) * 0.5f; } }
    }

    static void Dress(RoomObj room, RoomSpec spec, int index, Dictionary<string, Material> mats, GameObject charPrefab)
    {
        // Look (from the video frames): charcoal walls, light gray floor, bright red band and skirting,
        // red seams where floor meets wall, plain white light. No fog: the video's red cast is its colour grade.
        foreach (var r in room.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (r.GetComponentInParent<SceneObj>() != null && r.GetComponentInParent<SceneObj>() != room) continue; // leave proxies and markers
            var col = r.GetComponent<Collider>();
            bool isFloor = col != null && col.CompareTag("Floor");
            r.sharedMaterial = isFloor ? mats["floor"] : mats["wall"];
        }
        room.AmbientColor = new Color(0.30f, 0.29f, 0.29f);
        room.FogDensity = 0f;
        room.FogColor = Color.black;
        if (room.CameraPoint != null)
            foreach (var gf in room.CameraPoint.GetComponents<MonoBehaviour>().Where(b => b != null && b.GetType().Name == "GlobalFog")) gf.enabled = false;
        foreach (var l in room.GetComponentsInChildren<Light>(true)) { l.color = new Color(1f, 0.96f, 0.95f); }

        Physics.SyncTransforms();
        var fb = FloorBounds(room);
        var layout = room.transform.Find("Layout") ?? room.transform;
        var setGo = new GameObject("NX_Set");
        setGo.transform.SetParent(layout, false);
        setGo.transform.position = new Vector3(fb.center.x, fb.max.y, fb.center.z);
        setGo.isStatic = true;
        var f = new Frame { Parent = setGo.transform, x0 = fb.min.x, x1 = fb.max.x, zb = fb.max.z, zf = fb.min.z, y = fb.max.y, M = mats };
        // The floor runs under the walls: find the walls' inner faces by raycasting out from the middle of the room.
        Physics.SyncTransforms();
        var walls = room.GetComponentsInChildren<Collider>(true).Where(c => !c.CompareTag("Floor")).ToArray();
        var origin = new Vector3(fb.center.x, f.y + 1.2f, fb.center.z);
        f.zb = WallHit(walls, origin, Vector3.forward, f.zb);
        f.x0 = WallHit(walls, origin, Vector3.left, f.x0);
        f.x1 = WallHit(walls, origin, Vector3.right, f.x1);
        Debug.Log("NexusProminence: " + room.name + " walls x " + f.x0 + ".." + f.x1 + " back z " + f.zb + " (floor " + fb.min.x + ".." + fb.max.x + ", " + fb.max.z + ")");

        Shell(f, room);
        switch (index)
        {
            case 0: Street(f); break;
            case 1:
                WantedHall(f);
                // The video grades this hall deep red: tint the room's own light and ambient.
                foreach (var l in room.GetComponentsInChildren<Light>(true)) l.color = new Color(1f, 0.42f, 0.42f);
                room.AmbientColor = new Color(0.55f, 0.16f, 0.16f);
                break;
            case 2: ControlsOffice(f); break;
            case 3: CargoBay(f); break;
            case 4: SniperLoft(f); break;
            case 5: ShutterGarage(f); break;
            case 6: SpotlightCorridor(f); break;
            case 7: LockerRoom(f); break;
            case 8: BreakRoom(f); break;
            case 9: ControlTower(f); break;
        }

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
                // Spread through the floor, not one row: the video's rooms have crates and desks out in the open.
                Vector2 at = spec.CoverAt != null && c < spec.CoverAt.Length ? spec.CoverAt[c]
                    : new Vector2((c + 1f) / (spec.Cover.Length + 1f), c % 2 == 0 ? 0.35f : 0.65f);
                covers[c].transform.position = new Vector3(Mathf.Lerp(f.x0 + 1.5f, f.x1 - 1.5f, at.x), p.y, Mathf.Lerp(fb.min.z + 2f, f.zb - 1.5f, at.y));
                covers[c].gameObject.name = "PX_Cover_" + spec.Cover[c];
                EditorUtility.SetDirty(covers[c]);
            }
        }
        if (!spec.KeepLocker) foreach (var l in room.GetComponentsInChildren<ProxyObj_Locker>(true)) Object.DestroyImmediate(l.gameObject);
        if (!spec.KeepSnipers) foreach (var s in room.GetComponentsInChildren<Marker_Waypoint>(true).Where(w => w.gameObject.name.StartsWith("Sniperpoint"))) Object.DestroyImmediate(s.gameObject);

        // Enemies: Nexus faction, spread across the floor right of centre (the player enters from the left).
        if (charPrefab == null) { Debug.LogWarning("NexusProminence: " + CharStart + " missing; no enemies placed."); return; }
        var folder = room.CharacterFolder != null ? room.CharacterFolder.transform : room.transform;
        int total = spec.Foes.Sum(foe => foe.Count), n = 0;
        foreach (var foe in spec.Foes)
            for (int k = 0; k < foe.Count; k++, n++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab, room.gameObject.scene);
                go.transform.SetParent(folder, false);
                // Staged like the video: a loose group right of centre, staggered front to back.
                float t = total == 1 ? 0.7f : Mathf.Lerp(0.4f, 0.92f, n / (float)(total - 1));
                float depth = new[] { 0.5f, 0.25f, 0.75f, 0.4f, 0.6f }[n % 5];
                go.transform.position = new Vector3(Mathf.Lerp(f.x0 + 1f, f.x1 - 1f, t), fb.max.y, Mathf.Lerp(fb.min.z + 2f, f.zb - 1.5f, depth));
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

    static float WallHit(Collider[] walls, Vector3 origin, Vector3 dir, float fallback)
    {
        float best = float.MaxValue; RaycastHit hit;
        foreach (var c in walls)
            if (c.Raycast(new Ray(origin, dir), out hit, 200f) && hit.distance < best) best = hit.distance;
        if (best == float.MaxValue) return fallback;
        var p = origin + dir * best;
        return Vector3.Dot(p, dir) * (dir.x + dir.y + dir.z) ; // the hit's coordinate on that axis
    }

    // ---- shared shell: bands, skirting, seams, panels, ceiling pipe, door frames ----
    static void Shell(Frame f, RoomObj room)
    {
        const float bandLo = 1.05f, bandH = 0.55f;
        // back wall
        Box(f, "Band_Back", f.Cx, f.y + bandLo + bandH / 2, f.zb - 0.06f, f.W, bandH, 0.12f, "red");
        Box(f, "Skirt_Back", f.Cx, f.y + 0.14f, f.zb - 0.06f, f.W, 0.28f, 0.12f, "red");
        Box(f, "Seam_Back", f.Cx, f.y + 0.01f, f.zb - 0.35f, f.W, 0.02f, 0.18f, "red");
        // side walls
        foreach (var side in new[] { -1, 1 })
        {
            float x = side < 0 ? f.x0 + 0.06f : f.x1 - 0.06f;
            string s = side < 0 ? "L" : "R";
            Box(f, "Band_" + s, x, f.y + bandLo + bandH / 2, f.Cz, 0.12f, bandH, f.D, "red");
            Box(f, "Skirt_" + s, x, f.y + 0.14f, f.Cz, 0.12f, 0.28f, f.D, "red");
            Box(f, "Seam_" + s, x + side * -0.35f, f.y + 0.01f, f.Cz, 0.18f, 0.02f, f.D, "red");
        }
        // back wall panels (pilasters) and the pipe along the top, as in the corridor shots
        for (float x = f.x0 + 4f; x < f.x1 - 2f; x += 7.5f)
            Box(f, "Pilaster", x, f.y + 3.2f, f.zb - 0.12f, 0.45f, 6.4f, 0.2f, "panel");
        Box(f, "Pipe_Back", f.Cx, f.y + 6.0f, f.zb - 0.3f, f.W, 0.14f, 0.14f, "steel");
        for (float x = f.x0 + 2f; x < f.x1; x += 6f)
            Box(f, "PipeClamp", x, f.y + 6.1f, f.zb - 0.2f, 0.1f, 0.35f, 0.2f, "dark");
        var doors = room.GetComponentsInChildren<ProxyObj_Door>(true);
        foreach (var side in new[] { -1, 1 })
        {
            float x = side < 0 ? f.x0 + 0.07f : f.x1 - 0.07f;
            var sideDoors = doors.Where(d => (d.transform.position.x < f.Cx) == (side < 0)).Select(d => d.transform.position.z).ToArray();
            // the video's diagonal red stripe climbing each side wall from the band toward the ceiling at the back
            var diag = Box(f, "DiagStripe", x, f.y + 3.6f, f.zb - Mathf.Min(3.2f, f.D * 0.25f), 0.12f, 0.5f, 6.5f, "red");
            diag.transform.rotation = Quaternion.Euler(-38f, 0, 0);
            // a banner on each side wall, as far from that wall's doors as possible
            float bz = new[] { f.zb - 3f, f.Cz, f.zf + 4f }
                .OrderByDescending(z => sideDoors.Length == 0 ? 99f : sideDoors.Min(dz => Mathf.Abs(dz - z))).First();
            if (sideDoors.Length == 0 || sideDoors.Min(dz => Mathf.Abs(dz - bz)) > 2.2f) Banner(f, x, f.y + 3.4f, bz, true);
        }
        // dark corner posts where the back wall meets the sides, and floor panel seams
        Box(f, "CornerPost", f.x0 + 0.3f, f.y + 3.6f, f.zb - 0.3f, 0.6f, 7.2f, 0.6f, "dark");
        Box(f, "CornerPost", f.x1 - 0.3f, f.y + 3.6f, f.zb - 0.3f, 0.6f, 7.2f, 0.6f, "dark");
        for (float z = f.zb - 4f; z > f.zf + 1f; z -= 4f)
            Box(f, "FloorSeam", f.Cx, f.y + 0.006f, z, f.W - 1f, 0.004f, 0.05f, "dark");
        // the open floor: flat details only (nothing to walk around): drain grates, a painted red line
        // across the front third, loose papers; the game's cover props and the enemies fill the rest
        var rng = new System.Random(room.name.GetHashCode());
        Box(f, "FloorLine", f.Cx, f.y + 0.008f, f.zf + f.D * 0.3f, f.W - 3f, 0.004f, 0.12f, "red");
        for (int i = 0; i < Mathf.Max(2, (int)(f.W / 12f)); i++)
        {
            float gx = Mathf.Lerp(f.x0 + 3f, f.x1 - 3f, (i + 0.5f) / Mathf.Max(2, (int)(f.W / 12f))), gz = f.zf + f.D * (0.15f + 0.5f * (float)rng.NextDouble());
            Box(f, "Grate", gx, f.y + 0.01f, gz, 1.4f, 0.008f, 1.0f, "dark");
            for (int k = 0; k < 4; k++) Box(f, "GrateSlot", gx - 0.45f + k * 0.3f, f.y + 0.016f, gz, 0.1f, 0.004f, 0.8f, "black");
        }
        for (int i = 0; i < (int)(f.W / 4f); i++)
        {
            var paper = Box(f, "Paper", f.x0 + 2f + (float)rng.NextDouble() * (f.W - 4f), f.y + 0.012f, f.zf + 1f + (float)rng.NextDouble() * (f.D - 3f), 0.5f, 0.004f, 0.65f, "white");
            paper.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 180f, 0);
        }
        // door frames: black opening, dark jambs, red-and-dark hazard bar above
        foreach (var door in doors)
        {
            var p = door.transform.position;
            bool left = p.x < f.Cx;
            float x = left ? f.x0 + 0.04f : f.x1 - 0.04f;
            float inward = left ? 1 : -1;
            Box(f, "DoorHole", x, f.y + 1.9f, p.z, 0.05f, 3.8f, 2.6f, "black");
            Box(f, "DoorJambA", x + inward * 0.1f, f.y + 2.0f, p.z - 1.4f, 0.2f, 4.0f, 0.22f, "dark");
            Box(f, "DoorJambB", x + inward * 0.1f, f.y + 2.0f, p.z + 1.4f, 0.2f, 4.0f, 0.22f, "dark");
            Box(f, "DoorHeader", x + inward * 0.1f, f.y + 4.05f, p.z, 0.2f, 0.22f, 3.0f, "dark");
            for (int k = 0; k < 5; k++)
                Box(f, "Hazard", x + inward * 0.08f, f.y + 4.55f, p.z - 1.2f + k * 0.6f, 0.08f, 0.4f, 0.3f, k % 2 == 0 ? "red" : "dark");
        }
    }

    // ---- per-room dressing (layout and colour from the frames; no art, logos or signs copied) ----

    // 1: the street side (video 0:20-0:60). Left: a tall window onto the red city with a glowing sill, a counter
    // rail and a sign hung from the ceiling. Middle: a dark pillar with a bin and cups. Right: the way inside,
    // a recessed door panel with a red emblem board, a medkit cabinet and striped skirting. Stained dark floor.
    static void Street(Frame f)
    {
        var rng = new System.Random(7);
        Box(f, "Asphalt", f.Cx, f.y + 0.004f, f.Cz, f.W, 0.008f, f.D, "asphalt");
        for (int i = 0; i < 14; i++)
            Box(f, "Stain", f.x0 + 2f + (float)rng.NextDouble() * (f.W - 4f), f.y + 0.01f, f.zf + 1f + (float)rng.NextDouble() * (f.D - 2f),
                0.6f + (float)rng.NextDouble() * 1.8f, 0.006f, 0.4f + (float)rng.NextDouble() * 1.2f, "stain");

        float split = f.x0 + f.W * 0.74f;
        float wx0 = f.x0 + 1.6f, wx1 = split - 1.2f, wy0 = f.y + 1.6f, wy1 = f.y + 6.6f, wz = f.zb - 0.05f;
        Box(f, "Window_Sky", (wx0 + wx1) / 2, (wy0 + wy1) / 2, wz, wx1 - wx0, wy1 - wy0, 0.04f, "sky");
        for (float x = wx0 + 0.3f; x < wx1 - 0.6f; )
        {
            float w = 1.0f + (float)rng.NextDouble() * 1.8f;
            float h = 2.4f + (float)rng.NextDouble() * 2.6f;
            float cx = Mathf.Min(x + w / 2, wx1 - w / 2);
            Box(f, "Tower", cx, wy0 + h / 2, wz - 0.05f, w, h, 0.04f, rng.NextDouble() < 0.5 ? "black" : "dark");
            for (float yy = wy0 + 0.3f; yy < wy0 + h - 0.3f; yy += 0.4f)
                for (float xx = cx - w / 2 + 0.2f; xx < cx + w / 2 - 0.15f; xx += 0.3f)
                    if (rng.NextDouble() < 0.6) Box(f, "LitWin", xx, yy, wz - 0.08f, 0.14f, 0.2f, 0.02f, "lit");
            if (rng.NextDouble() < 0.45) // billboard on a tower face
            {
                Box(f, "BillFrame", cx, wy0 + h * 0.6f, wz - 0.1f, w * 0.8f, 1.2f, 0.02f, "white");
                Box(f, "Bill", cx, wy0 + h * 0.6f, wz - 0.11f, w * 0.7f, 1.05f, 0.02f, "screen");
            }
            x += w + 0.1f;
        }
        // thick frame, glowing red sill strip and the dark counter rail in front of it
        Box(f, "Window_Head", (wx0 + wx1) / 2, wy1 + 0.3f, f.zb - 0.3f, wx1 - wx0 + 0.8f, 0.6f, 0.6f, "dark");
        Box(f, "Window_SideA", wx0 - 0.2f, (wy0 + wy1) / 2, f.zb - 0.3f, 0.5f, wy1 - wy0, 0.6f, "dark");
        Box(f, "Window_SideB", wx1 + 0.2f, (wy0 + wy1) / 2, f.zb - 0.3f, 0.5f, wy1 - wy0, 0.6f, "dark");
        Box(f, "Window_Sill", (wx0 + wx1) / 2, wy0 + 0.1f, f.zb - 0.35f, wx1 - wx0, 0.2f, 0.6f, "screen");
        Box(f, "CounterRail", (wx0 + wx1) / 2, wy0 + 0.35f, f.zb - 0.75f, wx1 - wx0 - 0.4f, 0.12f, 0.25f, "dark");
        Box(f, "CounterLeg", wx0 + 0.6f, f.y + 0.95f, f.zb - 0.75f, 0.12f, 1.9f, 0.12f, "dark");
        Box(f, "CounterLeg", wx1 - 0.6f, f.y + 0.95f, f.zb - 0.75f, 0.12f, 1.9f, 0.12f, "dark");
        // sign hung from the ceiling in the middle of the window
        float sx = (wx0 + wx1) / 2;
        Box(f, "SignRodA", sx - 1f, wy1 + 0.2f, f.zb - 1.2f, 0.06f, 1.2f, 0.06f, "dark");
        Box(f, "SignRodB", sx + 1f, wy1 + 0.2f, f.zb - 1.2f, 0.06f, 1.2f, 0.06f, "dark");
        Box(f, "Sign", sx, wy1 - 0.7f, f.zb - 1.2f, 2.8f, 0.9f, 0.15f, "dark");
        Box(f, "SignFace", sx, wy1 - 0.7f, f.zb - 1.3f, 2.5f, 0.65f, 0.02f, "glass");
        // pillar between window and the inside
        Box(f, "Pillar", split, f.y + 3.6f, f.zb - 0.6f, 1.4f, 7.2f, 1.2f, "dark");
        Box(f, "PillarSkirt", split, f.y + 0.2f, f.zb - 1.22f, 1.4f, 0.4f, 0.04f, "red");
        Cyl(f, "TrashBin", split + 1.6f, f.y + 0.75f, f.zb - 1.0f, 1.1f, 1.5f, "steel");
        Box(f, "TrashLid", split + 1.6f, f.y + 1.52f, f.zb - 1.0f, 1.2f, 0.08f, 1.2f, "dark");
        for (int i = 0; i < 3; i++) Cyl(f, "Cup", split + 0.8f + i * 0.28f, f.y + 0.13f, f.zb - 1.7f - (i % 2) * 0.2f, 0.18f, 0.26f, "white");
        // inside: recessed door panel with an emblem board, medkit cabinet, striped skirting
        float dx = split + (f.x1 - split) * 0.62f;
        Box(f, "DoorPanelFrame", dx, f.y + 3.0f, f.zb - 0.12f, 3.6f, 5.6f, 0.08f, "dark");
        Box(f, "DoorPanel", dx, f.y + 3.0f, f.zb - 0.17f, 3.2f, 5.2f, 0.04f, "panel");
        Box(f, "EmblemBoard", dx, f.y + 4.0f, f.zb - 0.2f, 2.2f, 1.6f, 0.03f, "red");
        Box(f, "EmblemCut", dx, f.y + 3.9f, f.zb - 0.22f, 1.2f, 0.7f, 0.03f, "panel");
        float mx = split + (f.x1 - split) * 0.2f;
        Box(f, "Medkit", mx, f.y + 3.0f, f.zb - 0.2f, 1.1f, 1.3f, 0.25f, "white");
        Box(f, "MedkitBase", mx, f.y + 2.3f, f.zb - 0.2f, 1.1f, 0.15f, 0.26f, "red");
        Box(f, "MedkitCrossV", mx, f.y + 3.05f, f.zb - 0.34f, 0.18f, 0.6f, 0.02f, "steel");
        Box(f, "MedkitCrossH", mx, f.y + 3.05f, f.zb - 0.34f, 0.6f, 0.18f, 0.02f, "steel");
        for (float x = split + 1f; x < f.x1 - 0.4f; x += 0.9f)
            Box(f, "SkirtStripe", x, f.y + 0.14f, f.zb - 0.13f, 0.35f, 0.24f, 0.02f, "dark");
        for (float x = f.x0 + 2f; x < f.x1 - 1f; x += 3.2f) // back-wall panel seams
            Box(f, "PanelSeam", x, f.y + 3.6f, f.zb - 0.02f, 0.06f, 7.2f, 0.02f, "dark");
    }

    // 2: red-lit hall (video 1:28-2:00): wanted posters in red frames hung on rods, a glass display case of
    // supplies, tall dark doorways, red conduits from the ceiling, floor hatches, stripes on the band.
    static void WantedHall(Frame f)
    {
        float[] doors = { f.x0 + 0.30f * f.W, f.x0 + 0.78f * f.W };
        foreach (var x in doors)
        {
            Box(f, "BackDoorFrame", x, f.y + 2.4f, f.zb - 0.15f, 3.0f, 4.8f, 0.1f, "dark");
            Box(f, "BackDoor", x, f.y + 2.3f, f.zb - 0.2f, 2.5f, 4.6f, 0.04f, "black");
        }
        for (int i = 0; i < 3; i++)
        {
            float x = f.x0 + 0.12f * f.W + i * 0.27f * f.W;
            if (i == 1) x += 0.02f * f.W;
            float py = f.y + 3.3f;
            Box(f, "PosterRod", x, py + 2.0f, f.zb - 0.3f, 0.08f, 2.4f, 0.08f, "red");
            Box(f, "PosterFrame", x, py, f.zb - 0.3f, 2.3f, 2.9f, 0.12f, "red");
            Box(f, "Poster", x, py + 0.15f, f.zb - 0.38f, 1.8f, 2.1f, 0.02f, "white");
            Box(f, "PosterHead", x, py + 0.35f, f.zb - 0.41f, 0.9f, 1.05f, 0.02f, "panel");
            Box(f, "PosterShades", x, py + 0.45f, f.zb - 0.43f, 0.75f, 0.16f, 0.02f, "black");
            Box(f, "PosterCaption", x, py - 0.7f, f.zb - 0.41f, 1.8f, 0.4f, 0.02f, "dark");
        }
        // display case: dark cabinet, glowing glass, shelves with boxes and a medkit
        float cx = f.x0 + 0.55f * f.W;
        Box(f, "Case", cx, f.y + 2.3f, f.zb - 0.8f, 2.6f, 4.6f, 1.2f, "dark");
        Box(f, "CaseGlass", cx, f.y + 2.8f, f.zb - 1.42f, 2.1f, 3.0f, 0.03f, "glass");
        for (int k = 0; k < 3; k++)
        {
            float sy = f.y + 1.6f + k * 1.0f;
            Box(f, "CaseShelf", cx, sy, f.zb - 1.45f, 2.1f, 0.06f, 0.04f, "steel");
            Box(f, "CaseItem", cx - 0.5f, sy + 0.25f, f.zb - 1.47f, 0.5f, 0.4f, 0.02f, k == 1 ? "white" : "panel");
            Box(f, "CaseItem", cx + 0.4f, sy + 0.2f, f.zb - 1.47f, 0.6f, 0.3f, 0.02f, "steel");
        }
        Box(f, "CaseKeypad", cx - 0.85f, f.y + 2.4f, f.zb - 1.43f, 0.3f, 0.5f, 0.02f, "steel");
        Box(f, "CaseSlot", cx, f.y + 0.7f, f.zb - 1.43f, 1.4f, 0.5f, 0.02f, "black");
        // conduits down from the ceiling and stripes on the band
        foreach (var x in new[] { f.x0 + 0.42f * f.W, f.x0 + 0.92f * f.W })
            Box(f, "Conduit", x, f.y + 4.6f, f.zb - 0.2f, 0.25f, 5.2f, 0.2f, "red");
        foreach (var x in new[] { f.x0 + 0.2f * f.W, f.x0 + 0.66f * f.W })
            for (int k = 0; k < 4; k++) Box(f, "BandStripe", x + k * 0.5f, f.y + 1.32f, f.zb - 0.13f, 0.22f, 0.5f, 0.02f, "dark");
        // floor hatches with grilles
        foreach (var hx in new[] { f.x0 + 0.25f * f.W, f.x0 + 0.62f * f.W })
        {
            float hz = f.zb - 3.2f;
            Box(f, "HatchFrame", hx, f.y + 0.012f, hz, 2.6f, 0.024f, 1.8f, "steel");
            Box(f, "Hatch", hx, f.y + 0.02f, hz, 2.2f, 0.024f, 1.4f, "black");
            for (int i = 0; i < 5; i++) Box(f, "HatchSlot", hx - 0.8f + i * 0.4f, f.y + 0.03f, hz, 0.14f, 0.02f, 1.1f, "dark");
        }
    }

    // 3: office with a long service counter, a glowing back window and a door marked by a red header.
    static void ControlsOffice(Frame f)
    {
        Box(f, "BackWindow", f.Cx + 3f, f.y + 3.2f, f.zb - 0.06f, 12f, 3.2f, 0.04f, "screen");
        Box(f, "BackWindowFrame", f.Cx + 3f, f.y + 1.5f, f.zb - 0.15f, 12.4f, 0.25f, 0.2f, "dark");
        Box(f, "Counter", f.Cx + 3f, f.y + 0.6f, f.zb - 2.2f, 12f, 1.2f, 1.0f, "panel");
        Box(f, "CounterTop", f.Cx + 3f, f.y + 1.25f, f.zb - 2.2f, 12.3f, 0.1f, 1.2f, "dark");
        Box(f, "CounterStripe", f.Cx + 3f, f.y + 0.9f, f.zb - 2.72f, 12f, 0.2f, 0.02f, "red");
        Box(f, "NoticeA", f.x0 + 4f, f.y + 3.2f, f.zb - 0.1f, 1.6f, 1.1f, 0.06f, "panel");
        Box(f, "NoticeB", f.x0 + 6f, f.y + 3.0f, f.zb - 0.1f, 1.0f, 1.4f, 0.06f, "panel");
        Box(f, "BackDoorHole", f.x0 + 9.5f, f.y + 1.9f, f.zb - 0.05f, 2.4f, 3.8f, 0.04f, "black");
        Box(f, "BackDoorHeader", f.x0 + 9.5f, f.y + 4.0f, f.zb - 0.12f, 2.8f, 0.3f, 0.15f, "red");
        // more of the office: a door marked by a hazard header on the left, a banner, filing cabinets, a wall clock
        BackDoorway(f, f.x0 + 2.6f);
        Banner(f, f.x1 - 2.6f, f.y + 3.4f, f.zb - 0.1f, false);
        for (int i = 0; i < 3; i++) Box(f, "FileCabinet", f.x1 - 6.2f + i * 1.0f, f.y + 1.1f, f.zb - 0.55f, 0.9f, 2.2f, 0.9f, "steel");
        Cyl(f, "Clock", f.x0 + 5f, f.y + 5.2f, f.zb - 0.1f, 0.8f, 0.1f, "white");
    }

    // 4: cargo bay: strapped crates stacked against the back and a pallet line, red floor hazard band.
    static void CargoBay(Frame f)
    {
        Crate(f, f.x0 + 4f, f.zb - 1.4f, 2.2f);
        Crate(f, f.x0 + 4f, f.zb - 1.4f, 1.6f, 2.2f);
        Crate(f, f.x0 + 7f, f.zb - 1.4f, 1.8f);
        Crate(f, f.x1 - 5f, f.zb - 1.4f, 2.4f);
        Crate(f, f.x1 - 8f, f.zb - 1.2f, 1.6f);
        Box(f, "Pallet", f.Cx, f.y + 0.15f, f.zb - 1.2f, 3f, 0.3f, 2f, "steel");
        for (int i = 0; i < 12; i++)
            Box(f, "FloorHazard", f.x0 + 2f + i * 2.4f, f.y + 0.012f, f.zb - 3.3f, 1.2f, 0.02f, 0.4f, "red");
        // a wide bay: loading door at the right with hazard header, banners, more crates in the far corners
        BackDoorway(f, f.x1 - 3f);
        Banner(f, f.x0 + 0.38f * f.W, f.y + 3.6f, f.zb - 0.1f, false);
        Banner(f, f.x0 + 0.62f * f.W, f.y + 3.6f, f.zb - 0.1f, false);
        Crate(f, f.x0 + 1.6f, f.zb - 3.2f, 1.4f);
        Crate(f, f.x1 - 1.6f, f.zb - 4.4f, 1.4f);
        Box(f, "HangSign", f.Cx, f.y + 6.2f, f.zb - 3f, 3f, 0.5f, 0.1f, "red");
    }

    // 5: sniper loft: a ledge across the back wall at head height, ladder up to it, a big open-frame crate.
    static void SniperLoft(Frame f)
    {
        float ly = f.y + 3.4f, lx0 = f.Cx - 2f, lx1 = f.x1 - 1f;
        Box(f, "Ledge", (lx0 + lx1) / 2, ly, f.zb - 1.0f, lx1 - lx0, 0.3f, 2.0f, "panel");
        Box(f, "LedgeLip", (lx0 + lx1) / 2, ly + 0.05f, f.zb - 2.02f, lx1 - lx0, 0.4f, 0.06f, "red");
        Box(f, "LedgeRail", (lx0 + lx1) / 2, ly + 1.0f, f.zb - 1.95f, lx1 - lx0, 0.08f, 0.08f, "steel");
        for (float x = lx0 + 0.5f; x < lx1; x += 2.5f) Box(f, "LedgePost", x, ly + 0.55f, f.zb - 1.95f, 0.08f, 0.9f, 0.08f, "steel");
        Box(f, "LedgeSupport", lx0 + 0.3f, ly / 2 + f.y / 2, f.zb - 1.8f, 0.3f, ly - f.y, 0.3f, "dark");
        // ladder
        float lad = lx0 - 1.0f;
        Box(f, "LadderRailA", lad - 0.35f, (f.y + ly + 1f) / 2, f.zb - 1.8f, 0.08f, ly + 1f - f.y, 0.08f, "steel");
        Box(f, "LadderRailB", lad + 0.35f, (f.y + ly + 1f) / 2, f.zb - 1.8f, 0.08f, ly + 1f - f.y, 0.08f, "steel");
        for (float yy = f.y + 0.35f; yy < ly + 0.8f; yy += 0.4f) Box(f, "Rung", lad, yy, f.zb - 1.8f, 0.7f, 0.06f, 0.06f, "steel");
        // the red stripe climbs the wall beside the ledge, as in the frames
        Box(f, "BandRise", lx0 - 0.2f, (f.y + 1.6f + ly + 1.2f) / 2, f.zb - 0.07f, 0.55f, ly + 1.2f - f.y - 1.6f, 0.12f, "red");
        Box(f, "BandHigh", (lx0 + f.x1) / 2, ly + 1.0f, f.zb - 0.07f, f.x1 - lx0, 0.55f, 0.12f, "red");
        OpenCrate(f, f.x0 + 5f, f.zb - 1.8f, 2.6f);
        Banner(f, f.x0 + 0.3f * f.W, f.y + 3.4f, f.zb - 0.1f, false);
        BackDoorway(f, f.x1 - 3.5f);
    }

    // 6: garage: a big roll-up shutter over a low control console, side door frame.
    static void ShutterGarage(Frame f)
    {
        float sx = f.Cx - 2f, sw = 9f, sy0 = f.y + 1.8f, sh = 4.2f;
        Box(f, "ShutterBox", sx, sy0 + sh + 0.3f, f.zb - 0.35f, sw + 0.6f, 0.6f, 0.6f, "dark");
        for (float yy = sy0; yy < sy0 + sh; yy += 0.3f)
            Box(f, "Slat", sx, yy + 0.13f, f.zb - 0.15f, sw, 0.24f, 0.08f, "steel");
        Box(f, "ShutterGuideA", sx - sw / 2 - 0.1f, sy0 + sh / 2, f.zb - 0.2f, 0.2f, sh + 0.4f, 0.25f, "dark");
        Box(f, "ShutterGuideB", sx + sw / 2 + 0.1f, sy0 + sh / 2, f.zb - 0.2f, 0.2f, sh + 0.4f, 0.25f, "dark");
        Box(f, "Console", sx, f.y + 0.8f, f.zb - 0.6f, sw - 1f, 1.6f, 0.9f, "panel");
        for (int i = 0; i < 4; i++) Box(f, "ConsoleLamp", sx - 3f + i * 2f, f.y + 1.0f, f.zb - 1.07f, 0.8f, 0.6f, 0.03f, i == 3 ? "screen" : "dark");
        Box(f, "WallPanel", f.x1 - 4f, f.y + 3.2f, f.zb - 0.1f, 2.2f, 3.0f, 0.08f, "dark");
        Box(f, "WallPanelStripes", f.x1 - 4f, f.y + 2.0f, f.zb - 0.15f, 2.2f, 0.3f, 0.04f, "red");
        // vending machine and a banner beside the shutter, oil stains on the floor
        Box(f, "Vending", f.x0 + 2.4f, f.y + 1.9f, f.zb - 0.7f, 1.6f, 3.8f, 1.2f, "dark");
        Box(f, "VendingFace", f.x0 + 2.4f, f.y + 2.3f, f.zb - 1.32f, 1.2f, 2.4f, 0.03f, "screen");
        Banner(f, f.x1 - 7f, f.y + 3.4f, f.zb - 0.1f, false);
        foreach (var x in new[] { 0.3f, 0.55f, 0.75f }) Box(f, "OilStain", f.x0 + x * f.W, f.y + 0.01f, f.Cz + (x - 0.5f) * 3f, 1.6f, 0.006f, 1.0f, "stain");
    }

    // 7: corridor lit by ceiling lamps throwing red cones down the back wall; dark slate board.
    static void SpotlightCorridor(Frame f)
    {
        for (int i = 0; i < 4; i++)
        {
            float x = f.x0 + 4f + i * (f.W - 8f) / 3f;
            Box(f, "Lamp", x, f.y + 6.3f, f.zb - 0.5f, 1.4f, 0.25f, 0.6f, "dark");
            Box(f, "LampGlow", x, f.y + 6.16f, f.zb - 0.5f, 1.1f, 0.04f, 0.4f, "screen");
            var beam = new GameObject("Beam");
            beam.transform.SetParent(f.Parent, false);
            beam.transform.position = new Vector3(x, f.y, f.zb - 0.4f);
            beam.transform.localScale = new Vector3(7f, 6.2f, 1f);
            beam.AddComponent<MeshFilter>().sharedMesh = BeamMesh();
            var r = beam.AddComponent<MeshRenderer>();
            r.sharedMaterial = f.M["beam"];
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
        Box(f, "Slate", f.Cx + 1f, f.y + 3.2f, f.zb - 0.1f, 6f, 2.4f, 0.06f, "dark");
        Box(f, "SlateFrame", f.Cx + 1f, f.y + 1.95f, f.zb - 0.14f, 6.3f, 0.12f, 0.12f, "steel");
        Box(f, "Shutter", f.x0 + 3f, f.y + 2.2f, f.zb - 0.12f, 2f, 4.4f, 0.06f, "steel");
        for (float yy = f.y + 0.3f; yy < f.y + 4.4f; yy += 0.35f) Box(f, "ShutterLine", f.x0 + 3f, yy, f.zb - 0.16f, 2f, 0.05f, 0.02f, "dark");
        // banners between the lamps and doorways at both ends of the long corridor
        for (int i = 0; i < 3; i++) Banner(f, f.x0 + 4f + (i + 0.5f) * (f.W - 8f) / 3f, f.y + 3.4f, f.zb - 0.1f, false);
        BackDoorway(f, f.x0 + 1.9f);
        BackDoorway(f, f.x1 - 1.9f);
    }

    // 8: locker room: a bank of tall lockers, a bench, a cabinet; a ceiling lamp cone.
    static void LockerRoom(Frame f)
    {
        float lx = f.x0 + 4f;
        for (int i = 0; i < 6; i++)
        {
            float x = lx + i * 1.15f;
            Box(f, "Locker", x, f.y + 1.7f, f.zb - 0.6f, 1.1f, 3.4f, 1.0f, "steel");
            for (int v = 0; v < 3; v++) Box(f, "Vent", x, f.y + 2.9f - v * 0.18f, f.zb - 1.12f, 0.6f, 0.06f, 0.02f, "dark");
            Box(f, "Handle", x + 0.35f, f.y + 1.8f, f.zb - 1.12f, 0.06f, 0.35f, 0.04f, "dark");
        }
        Box(f, "Bench", lx + 3f, f.y + 0.5f, f.zb - 2.4f, 5f, 0.18f, 0.7f, "dark");
        Box(f, "BenchLegA", lx + 1f, f.y + 0.25f, f.zb - 2.4f, 0.15f, 0.5f, 0.5f, "dark");
        Box(f, "BenchLegB", lx + 5f, f.y + 0.25f, f.zb - 2.4f, 0.15f, 0.5f, 0.5f, "dark");
        Box(f, "Cabinet", f.x1 - 5f, f.y + 1.1f, f.zb - 0.6f, 2.4f, 2.2f, 1.0f, "panel");
        Box(f, "BoardFrame", f.x1 - 9f, f.y + 3.3f, f.zb - 0.1f, 1.4f, 1.8f, 0.1f, "red");
        Box(f, "Board", f.x1 - 9f, f.y + 3.3f, f.zb - 0.16f, 1.1f, 1.5f, 0.04f, "panel");
        Banner(f, f.x1 - 2.2f, f.y + 3.4f, f.zb - 0.1f, false);
        Cyl(f, "Bin", f.x1 - 7f, f.y + 0.6f, f.zb - 0.7f, 0.9f, 1.2f, "steel");
    }

    // 9: break room: serving counter under a row of glowing menu panels, tables, a drum.
    static void BreakRoom(Frame f)
    {
        float cx = f.Cx + 2f;
        Box(f, "ServeCounter", cx, f.y + 0.7f, f.zb - 1.0f, 10f, 1.4f, 1.4f, "panel");
        Box(f, "ServeTop", cx, f.y + 1.45f, f.zb - 1.0f, 10.3f, 0.1f, 1.6f, "dark");
        Box(f, "ServeStripe", cx, f.y + 1.0f, f.zb - 1.72f, 10f, 0.25f, 0.02f, "red");
        for (int i = 0; i < 3; i++) Box(f, "MenuPanel", cx - 3.4f + i * 3.4f, f.y + 4.1f, f.zb - 0.1f, 2.8f, 1.5f, 0.06f, "screen");
        for (int i = 0; i < 3; i++) Box(f, "MenuFrame", cx - 3.4f + i * 3.4f, f.y + 4.1f, f.zb - 0.06f, 3.0f, 1.7f, 0.04f, "dark");
        Box(f, "Table", f.x0 + 6f, f.y + 0.9f, f.zb - 4f, 2.6f, 0.12f, 1.4f, "dark");
        Box(f, "TableLeg", f.x0 + 6f, f.y + 0.45f, f.zb - 4f, 0.2f, 0.9f, 0.2f, "dark");
        Cyl(f, "Drum", f.x0 + 2.5f, f.y + 0.8f, f.zb - 0.9f, 1.2f, 1.6f, "steel");
        Box(f, "DrumBand", f.x0 + 2.5f, f.y + 1.1f, f.zb - 0.9f, 1.25f, 0.15f, 1.25f, "red");
        // a slate board with chalk lines, a fridge and a vending machine, a banner
        Box(f, "Slate", f.x0 + 4.5f, f.y + 3.6f, f.zb - 0.1f, 3.2f, 2.0f, 0.06f, "dark");
        for (int i = 0; i < 4; i++) Box(f, "Chalk", f.x0 + 4.5f - 0.2f * (i % 2), f.y + 4.2f - i * 0.4f, f.zb - 0.14f, 2.2f - 0.3f * i, 0.06f, 0.02f, "white");
        Box(f, "Fridge", f.x1 - 2.2f, f.y + 2.0f, f.zb - 0.7f, 1.6f, 4.0f, 1.2f, "white");
        Box(f, "FridgeLine", f.x1 - 2.2f, f.y + 2.7f, f.zb - 1.31f, 1.6f, 0.05f, 0.02f, "dark");
        Box(f, "Vending", f.x1 - 4.2f, f.y + 1.9f, f.zb - 0.7f, 1.6f, 3.8f, 1.2f, "dark");
        Box(f, "VendingFace", f.x1 - 4.2f, f.y + 2.3f, f.zb - 1.32f, 1.2f, 2.4f, 0.03f, "screen");
        Banner(f, f.x0 + 1.9f, f.y + 3.4f, f.zb - 0.1f, false);
    }

    // 10: control tower: a wall of red monitors above a long desk, server racks at the sides.
    static void ControlTower(Frame f)
    {
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 6; c++)
            {
                float x = f.Cx - 6.25f + c * 2.5f, yy = f.y + 2.6f + r * 1.5f;
                Box(f, "MonitorCase", x, yy, f.zb - 0.2f, 2.3f, 1.35f, 0.25f, "black");
                Box(f, "MonitorScreen", x, yy, f.zb - 0.34f, 2.0f, 1.1f, 0.02f, (r + c) % 4 == 1 ? "dark" : "screen");
            }
        Box(f, "Desk", f.Cx, f.y + 0.75f, f.zb - 1.6f, 15f, 1.5f, 1.4f, "panel");
        Box(f, "DeskTop", f.Cx, f.y + 1.55f, f.zb - 1.6f, 15.4f, 0.1f, 1.6f, "dark");
        for (int i = 0; i < 4; i++) Box(f, "Keyboard", f.Cx - 5f + i * 3.3f, f.y + 1.63f, f.zb - 1.9f, 1.4f, 0.06f, 0.5f, "black");
        foreach (var x in new[] { f.x0 + 2.5f, f.x0 + 4f, f.x1 - 4f, f.x1 - 2.5f })
        {
            Box(f, "Rack", x, f.y + 2.0f, f.zb - 0.8f, 1.3f, 4.0f, 1.2f, "black");
            for (float yy = f.y + 0.6f; yy < f.y + 3.8f; yy += 0.45f) Box(f, "RackLed", x - 0.35f, yy, f.zb - 1.42f, 0.25f, 0.06f, 0.02f, "screen");
        }
        // the top of the tower: a red sky strip above the monitor wall, banners flanking it, a door to the server room
        Box(f, "SkyStrip", f.Cx, f.y + 6.6f, f.zb - 0.05f, f.W - 2f, 0.9f, 0.04f, "sky");
        Box(f, "SkyStripFrame", f.Cx, f.y + 6.1f, f.zb - 0.12f, f.W - 2f, 0.12f, 0.12f, "dark");
        Banner(f, f.Cx - 9f, f.y + 3.6f, f.zb - 0.1f, false);
        Banner(f, f.Cx + 9f, f.y + 3.6f, f.zb - 0.1f, false);
        BackDoorway(f, f.x1 - 6.5f);
    }

    // Faction-style banner: red board, dark border, a plain chevron (an original mark, not the game's logo).
    static void Banner(Frame f, float x, float y, float z, bool onSide)
    {
        float w = 1.8f, h = 2.6f;
        if (onSide)
        {
            float inward = x < f.Cx ? 1 : -1;
            Box(f, "BannerBorder", x, y, z, 0.05f, h + 0.2f, w + 0.2f, "dark");
            Box(f, "Banner", x + inward * 0.04f, y, z, 0.04f, h, w, "red");
            var a = Box(f, "BannerMarkA", x + inward * 0.07f, y + 0.1f, z - 0.3f, 0.02f, 1.2f, 0.25f, "dark");
            a.transform.rotation = Quaternion.Euler(35f, 0, 0);
            var b = Box(f, "BannerMarkB", x + inward * 0.07f, y + 0.1f, z + 0.3f, 0.02f, 1.2f, 0.25f, "dark");
            b.transform.rotation = Quaternion.Euler(-35f, 0, 0);
        }
        else
        {
            Box(f, "BannerBorder", x, y, z, w + 0.2f, h + 0.2f, 0.05f, "dark");
            Box(f, "Banner", x, y, z - 0.04f, w, h, 0.04f, "red");
            var a = Box(f, "BannerMarkA", x - 0.3f, y + 0.1f, z - 0.07f, 0.25f, 1.2f, 0.02f, "dark");
            a.transform.rotation = Quaternion.Euler(0, 0, 35f);
            var b = Box(f, "BannerMarkB", x + 0.3f, y + 0.1f, z - 0.07f, 0.25f, 1.2f, 0.02f, "dark");
            b.transform.rotation = Quaternion.Euler(0, 0, -35f);
        }
    }

    // Recessed doorway in the back wall with the video's red-and-dark hazard header.
    static void BackDoorway(Frame f, float x)
    {
        Box(f, "BackDoorFrame", x, f.y + 2.4f, f.zb - 0.12f, 3.0f, 4.8f, 0.1f, "dark");
        Box(f, "BackDoor", x, f.y + 2.3f, f.zb - 0.18f, 2.5f, 4.6f, 0.04f, "black");
        for (int k = 0; k < 5; k++)
            Box(f, "BackHazard", x - 1.2f + k * 0.6f, f.y + 5.2f, f.zb - 0.12f, 0.3f, 0.4f, 0.06f, k % 2 == 0 ? "red" : "dark");
    }

    static void Crate(Frame f, float x, float z, float size, float stackOn = 0f)
    {
        float cy = f.y + stackOn + size / 2;
        Box(f, "Crate", x, cy, z, size, size, size, "panel");
        Box(f, "CrateStrapA", x - size * 0.25f, cy, z, 0.18f, size + 0.02f, size + 0.02f, "red");
        Box(f, "CrateStrapB", x + size * 0.25f, cy, z, 0.18f, size + 0.02f, size + 0.02f, "red");
        Box(f, "CrateLid", x, cy + size / 2, z, size + 0.06f, 0.08f, size + 0.06f, "dark");
    }

    static void OpenCrate(Frame f, float x, float z, float s)
    {
        float h = s / 2;
        foreach (var dx in new[] { -h, h })
            foreach (var dz in new[] { -h, h })
                Box(f, "Edge", x + dx, f.y + h, z + dz, 0.15f, s, 0.15f, "dark");
        foreach (var dy in new[] { 0f, s })
        {
            Box(f, "Edge", x, f.y + dy, z - h, s, 0.15f, 0.15f, "dark");
            Box(f, "Edge", x, f.y + dy, z + h, s, 0.15f, 0.15f, "dark");
            Box(f, "Edge", x - h, f.y + dy, z, 0.15f, 0.15f, s, "dark");
            Box(f, "Edge", x + h, f.y + dy, z, 0.15f, 0.15f, s, "dark");
        }
        var brace = Box(f, "Brace", x, f.y + h, z - h, s * 1.35f, 0.12f, 0.12f, "dark");
        brace.transform.rotation = Quaternion.Euler(0, 0, 45);
    }

    // Visual-only box (no collider, so pathing and the room's own collision are untouched).
    static GameObject Box(Frame f, string name, float x, float y, float z, float sx, float sy, float sz, string mat)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(s.GetComponent<Collider>());
        s.name = "NX_" + name;
        s.transform.SetParent(f.Parent, true);
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(sx, sy, sz);
        var r = s.GetComponent<MeshRenderer>();
        r.sharedMaterial = f.M[mat];
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        s.isStatic = true;
        return s;
    }

    static void Cyl(Frame f, string name, float x, float y, float z, float diameter, float height, string mat)
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.DestroyImmediate(s.GetComponent<Collider>());
        s.name = "NX_" + name;
        s.transform.SetParent(f.Parent, true);
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(diameter, height / 2, diameter);
        s.GetComponent<MeshRenderer>().sharedMaterial = f.M[mat];
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
