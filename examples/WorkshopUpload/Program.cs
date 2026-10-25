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

    static async Task<int> Main(string[] a)
    {
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
