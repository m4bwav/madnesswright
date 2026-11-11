using System.IO;
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
        }
    }
}
