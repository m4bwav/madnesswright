# Workshop upload without prompts

A 30-line program that publishes or updates a MADNESS: Project Nexus Workshop item (app 488860) through the Steam client you are logged into. The community MPN-Workshop-Uploader asks its questions on the console and fails when they are piped in; this one takes arguments, so an agent or a script can run it. No password is typed: Steam must be running and logged in.

## Build

1. Download `MPNWorkshopUploader.zip` from the 1.1.0 release of https://github.com/placenamehereig/MPN-Workshop-Uploader (MIT) and copy `Facepunch.Steamworks.Win64.dll` and `steam_api64.dll` into `lib/` here.
2. `dotnet build -c Release -o out`

## Run

```
out/wsup.exe <content-folder> <preview.jpg> "<title>" <description.txt> "<tags,comma,separated>" [item-id]
```

- Without `item-id` it creates a new item; with one it updates that item (title, description, content, preview, tags) and adds a change note.
- Visibility is set to Public. Change `WithPublicVisibility()` to `WithFriendsOnlyVisibility()` or `WithPrivateVisibility()` in Program.cs for anything else.
- Pass absolute paths: with relative ones the upload reaches 100% and then prints `result FileNotFound`.
- The content folder holds what goes in the mod folder (`info.txt`, `Managed/`, cards, bundles). Keep the preview under 1 MB.
- It prints `result OK id <n>`. Check the item with `ISteamRemoteStorage/GetPublishedFileDetails` (visibility 0 is public) and open `https://steamcommunity.com/sharedfiles/filedetails/?id=<n>`.

Used for [SwapUnaware](../SwapUnaware/README.md) (item 3822329165): created on 2026-10-18; updated in place on 2026-10-21 (new title, description, preview and DLL), and the item stayed public (visibility 0) with its subscribers; updated again on 2026-10-22 (0.6.0, absolute paths).
