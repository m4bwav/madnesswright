using System;
using System.IO;
using System.Threading.Tasks;
using Steamworks;
using Ugc = Steamworks.Ugc;

// Non-interactive Workshop upload for MADNESS: Project Nexus (app 488860) through the running,
// logged-in Steam client. Args: content-dir preview title description-file tags [item-id change-notes-file]
// An update needs a change-notes file: the item's Change Notes tab shows its text to subscribers.
static class Program
{
    class P : IProgress<float> { float last; public void Report(float v) { if (v - last >= 0.1f) { last = v; Console.WriteLine("progress " + (int)(v * 100) + "%"); } } }

    const string Api = "steam_api64";
    [System.Runtime.InteropServices.DllImport(Api)] static extern IntPtr SteamAPI_SteamUGC_v014();
    [System.Runtime.InteropServices.DllImport(Api)] static extern IntPtr SteamAPI_SteamUtils_v009();
    [System.Runtime.InteropServices.DllImport(Api)] static extern ulong SteamAPI_ISteamUGC_StartItemUpdate(IntPtr s, uint app, ulong file);
    [System.Runtime.InteropServices.DllImport(Api)] static extern bool SteamAPI_ISteamUGC_AddItemPreviewFile(IntPtr s, ulong h, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPUTF8Str)] string path, int type);
    [System.Runtime.InteropServices.DllImport(Api)] static extern bool SteamAPI_ISteamUGC_AddItemPreviewVideo(IntPtr s, ulong h, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPUTF8Str)] string id);
    [System.Runtime.InteropServices.DllImport(Api)] static extern bool SteamAPI_ISteamUGC_RemoveItemPreview(IntPtr s, ulong h, uint index);
    [System.Runtime.InteropServices.DllImport(Api)] static extern ulong SteamAPI_ISteamUGC_SubmitItemUpdate(IntPtr s, ulong h, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPUTF8Str)] string note);
    [System.Runtime.InteropServices.DllImport(Api)] static extern bool SteamAPI_ISteamUtils_IsAPICallCompleted(IntPtr s, ulong call, out bool failed);
    [System.Runtime.InteropServices.DllImport(Api)] static extern bool SteamAPI_ISteamUtils_GetAPICallResult(IntPtr s, ulong call, IntPtr buf, int size, int cb, out bool failed);

    // previews <item-id> <change-notes-file> <image|youtube:ID>... : adds extra images and YouTube videos to an
    // item without touching its content, title or main preview. "--clear N" right after the notes file first
    // removes the item's N existing extra previews (highest index first), so new screenshots replace the old.
    static int Previews(string[] a)
    {
        string note = File.ReadAllText(a[2]).Trim();
        if (note.Length == 0) { Console.WriteLine("error empty change notes"); return 2; }
        SteamClient.Init(488860, false);
        try
        {
            IntPtr ugc = SteamAPI_SteamUGC_v014(), utils = SteamAPI_SteamUtils_v009();
            ulong h = SteamAPI_ISteamUGC_StartItemUpdate(ugc, 488860, ulong.Parse(a[1]));
            int first = 3;
            if (a.Length > 4 && a[3] == "--clear")
            {
                for (int k = int.Parse(a[4]) - 1; k >= 0; k--) Console.WriteLine("remove " + k + " " + SteamAPI_ISteamUGC_RemoveItemPreview(ugc, h, (uint)k));
                first = 5;
            }
            for (int i = first; i < a.Length; i++)
            {
                bool ok = a[i].StartsWith("youtube:") ? SteamAPI_ISteamUGC_AddItemPreviewVideo(ugc, h, a[i].Substring(8))
                                                      : SteamAPI_ISteamUGC_AddItemPreviewFile(ugc, h, Path.GetFullPath(a[i]), 0);
                Console.WriteLine("add " + a[i] + " " + ok);
            }
            ulong call = SteamAPI_ISteamUGC_SubmitItemUpdate(ugc, h, note);
            bool failed;
            for (int t = 0; t < 1200 && !SteamAPI_ISteamUtils_IsAPICallCompleted(utils, call, out failed); t++) { SteamClient.RunCallbacks(); System.Threading.Thread.Sleep(250); }
            IntPtr buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(16);
            try
            {
                SteamAPI_ISteamUtils_GetAPICallResult(utils, call, buf, 16, 3404, out failed);
                int result = System.Runtime.InteropServices.Marshal.ReadInt32(buf);
                Console.WriteLine("result " + result + " failed " + failed);
                return result == 1 && !failed ? 0 : 1;
            }
            finally { System.Runtime.InteropServices.Marshal.FreeHGlobal(buf); }
        }
        finally { SteamClient.Shutdown(); }
    }

    static async Task<int> Main(string[] a)
    {
        if (a.Length > 3 && a[0] == "previews") return Previews(a);
        if (a.Length == 6 || (a.Length > 6 && File.ReadAllText(a[6]).Trim().Length == 0))
        {
            Console.WriteLine("error an update needs a non-empty change-notes file (what changed for the player)");
            return 2;
        }
        SteamClient.Init(488860, true);
        try
        {
            Ugc.Editor ed = a.Length > 5 ? new Ugc.Editor(new Steamworks.Data.PublishedFileId { Value = ulong.Parse(a[5]) }) : Ugc.Editor.NewCommunityFile;
            ed = ed.WithTitle(a[2]).WithDescription(File.ReadAllText(a[3])).WithContent(a[0]).WithPreviewFile(a[1]).WithPublicVisibility();
            foreach (var t in a[4].Split(',')) ed = ed.WithTag(t.Trim());
            if (a.Length > 5) ed = ed.WithChangeLog(File.ReadAllText(a[6]).Trim());
            var r = await ed.SubmitAsync(new P());
            Console.WriteLine("result " + r.Result + " id " + r.FileId.Value + " needs_legal_agreement " + r.NeedsWorkshopAgreement);
            return r.Result == Result.OK ? 0 : 1;
        }
        finally { SteamClient.Shutdown(); }
    }
}
