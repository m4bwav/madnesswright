using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch helper: render every room from its CameraPoint to PNG, for comparing against reference frames.
// Unity.exe -batchmode -quit -projectPath <SceneTools> -executeMethod NexusShots.Render -shotDir <dir>
public static class NexusShots
{
    public static void Render()
    {
        string dir = "NexusShots";
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-shotDir") dir = args[i + 1];
        Directory.CreateDirectory(dir);
        EditorSceneManager.OpenScene("Assets/Scenes/NexusProminence_PG.unity");
        var rooms = Object.FindObjectsOfType<RoomObj>();
        foreach (var room in rooms)
        {
            var cam = room.CameraPoint;
            if (cam == null) { Debug.LogWarning("NexusShots: no camera in " + room.name); continue; }
            RenderSettings.ambientLight = room.AmbientColor;
            RenderSettings.fog = room.FogDensity > 0;
            RenderSettings.fogColor = room.FogColor;
            RenderSettings.fogDensity = room.FogDensity;
            RenderSettings.fogMode = room.FogMode;
            RenderSettings.fogStartDistance = room.FogLinearStartEnd.x;
            RenderSettings.fogEndDistance = room.FogLinearStartEnd.y;
            foreach (var other in rooms) foreach (var l in other.GetComponentsInChildren<Light>(true)) l.enabled = other == room;
            if (room.CameraTarget != null) cam.transform.LookAt(room.CameraTarget.transform);
            var rt = new RenderTexture(960, 540, 24);
            bool was = cam.enabled; cam.enabled = true;
            cam.targetTexture = rt; cam.Render(); cam.targetTexture = null; cam.enabled = was;
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(dir, room.name + ".png"), tex.EncodeToPNG());
            Debug.Log("NexusShots: " + room.name + " fog " + room.FogDensity + " ambient " + room.AmbientColor);
            if (System.Array.IndexOf(args, "-corners") >= 0) Corners(room, dir);
        }
    }

    // Reproduces Cam_Main's follow (decompile, build 25615196; zoom off, mouse centred) with the player
    // standing 1.5 units inside each floor corner, renders what the game would show, and logs whether
    // the player marker lands inside the frame. A red pillar marks the player.
    static void Corners(RoomObj room, string dir)
    {
        var cam = room.CameraPoint; var tgt = room.CameraTarget;
        if (cam == null || tgt == null || room.BorderLeft == null || room.BorderRight == null) return;
        Physics.SyncTransforms();
        var floors = room.GetComponentsInChildren<Collider>().Where(c => c.gameObject.CompareTag("Floor")).ToList();
        if (floors.Count == 0) { Debug.LogWarning("NexusShots: no Floor collider in " + room.name); return; }
        var fb = floors[0].bounds; foreach (var c in floors) fb.Encapsulate(c.bounds);
        Vector3 start = tgt.transform.position, startPos = cam.transform.position, startFwd = cam.transform.forward;
        Vector3 offset = start - startPos;
        float aspect = 16f / 9f;
        float bx0 = room.BorderLeft.transform.position.x, bx1 = room.BorderRight.transform.position.x;
        float bz0 = room.BorderBack != null ? room.BorderBack.transform.position.z : fb.min.z;
        float bz1 = room.BorderForward != null ? room.BorderForward.transform.position.z : fb.max.z;
        if (bx0 > bx1) { var s = bx0; bx0 = bx1; bx1 = s; }
        if (bz0 > bz1) { var s = bz0; bz0 = bz1; bz1 = s; }
        float num3 = offset.magnitude * (cam.fieldOfView * 0.005f);
        float t2 = Mathf.Clamp01((bx1 - bx0 - 7f) * 0.1f);
        float num4 = num3 * Mathf.Lerp(0f, aspect - 1f, t2), num5 = num3 * Mathf.Lerp(0f, 1f / aspect - 1f, t2);
        Vector3 lp = room.Leeway_Pos, ll = room.Leeway_Look;
        var savedPos = cam.transform.position; var savedRot = cam.transform.rotation; float savedAspect = cam.aspect;
        var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
        var mr = marker.GetComponent<Renderer>(); mr.sharedMaterial = new Material(Shader.Find("Unlit/Color")); mr.sharedMaterial.color = Color.red;
        string[] names = { "FL", "FR", "BL", "BR" };
        // "front" is the camera side: the floor edge nearer the camera in z
        bool camAtMaxZ = startPos.z > fb.center.z;
        float zf = camAtMaxZ ? fb.max.z - 1.5f : fb.min.z + 1.5f, zb = camAtMaxZ ? fb.min.z + 1.5f : fb.max.z - 1.5f;
        Vector3[] pts = { new Vector3(fb.min.x + 1.5f, fb.max.y, zf), new Vector3(fb.max.x - 1.5f, fb.max.y, zf),
                          new Vector3(fb.min.x + 1.5f, fb.max.y, zb), new Vector3(fb.max.x - 1.5f, fb.max.y, zb) };
        for (int k = 0; k < 4; k++)
        {
            Vector3 p = pts[k] + Vector3.up * 1.2f; // focus point about chest height
            marker.transform.position = pts[k] + Vector3.up * 1f;
            Vector3 view = start + Vector3.Scale(p - start, lp), look = start + Vector3.Scale(p - start, ll);
            System.Func<float, float, float, float> cl = (v, a, b) => Mathf.Clamp(v, Mathf.Min(a, b), Mathf.Max(a, b));
            view.x = cl(view.x, start.x + (bx0 + num4 - start.x) * Mathf.Abs(lp.x), start.x + (bx1 - num4 - start.x) * Mathf.Abs(lp.x));
            view.z = cl(view.z, start.z + (bz0 + num5 - start.z) * Mathf.Abs(lp.z), start.z + (bz1 - num5 - start.z) * Mathf.Abs(lp.z));
            look.x = cl(look.x, start.x + (bx0 + num4 - start.x) * Mathf.Abs(ll.x), start.x + (bx1 - num4 - start.x) * Mathf.Abs(ll.x));
            look.z = cl(look.z, start.z + (bz0 + num5 - start.z) * Mathf.Abs(ll.z), start.z + (bz1 - num5 - start.z) * Mathf.Abs(ll.z));
            Vector3 pos = view - offset;
            cam.transform.position = pos;
            cam.transform.rotation = Quaternion.LookRotation(look - pos, Vector3.up) * Quaternion.FromToRotation(offset.normalized, startFwd);
            cam.aspect = aspect;
            Vector3 vp = cam.WorldToViewportPoint(pts[k] + Vector3.up * 0.1f), vh = cam.WorldToViewportPoint(pts[k] + Vector3.up * 1.8f);
            bool inside = vp.z > 0 && vp.x > 0.03f && vp.x < 0.97f && vp.y > 0.03f && vh.y < 0.97f;
            Debug.Log("NexusCorners: " + room.name + " " + names[k] + (inside ? " OK" : " OUT") + " feet " + vp.x.ToString("0.00") + "," + vp.y.ToString("0.00") + " head y " + vh.y.ToString("0.00"));
            var rt = new RenderTexture(640, 360, 24);
            bool was = cam.enabled; cam.enabled = true;
            cam.targetTexture = rt; cam.Render(); cam.targetTexture = null; cam.enabled = was;
            RenderTexture.active = rt;
            var tex = new Texture2D(640, 360, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 640, 360), 0, 0); tex.Apply(); RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(dir, room.name + "_" + names[k] + ".png"), tex.EncodeToPNG());
        }
        Object.DestroyImmediate(marker);
        cam.transform.position = savedPos; cam.transform.rotation = savedRot; cam.aspect = savedAspect;
    }
}
