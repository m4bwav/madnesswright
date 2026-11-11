using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch helper: SYNCH VISUALS on room 1, save, build bundles (compressed).
public static class NexusBatch
{
    public static void SyncAndBuild()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/NexusProminence_PG.unity");
        RoomObj first = null;
        foreach (var r in Object.FindObjectsOfType<RoomObj>())
            if (r.name == "Room_01_StreetCheckpoint") first = r;
        if (first == null) { Debug.LogError("NexusBatch: room 1 not found"); EditorApplication.Exit(2); return; }
        var ed = Editor.CreateEditor(first);
        ed.GetType().GetMethod("ActivateRoomVisuals", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ed, null);
        Object.DestroyImmediate(ed);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("NexusBatch: synched and saved");
        typeof(AssetBundle_Builder_Editor).GetMethod("Build_Compressed", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        Debug.Log("NexusBatch: build done");
    }
}
