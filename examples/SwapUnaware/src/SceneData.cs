using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // A saved scene. Version 1 (0.4.0) was Playground-only and keyed by the Unity scene name;
    // version 2 adds the stage key, each character's room and serial number, and your place;
    // version 3 adds holsters, ammo, your loadout and loose weapons (SceneItems.cs);
    // version 4 the bodies already lying in the level, so a load clears only newer ones;
    // version 5 the looping emote each character was playing;
    // version 6 the other players' characters (local co-op), marked by player number;
    // version 7 who had infinite ammo (0.11.0). Older files load with infinite ammo off for everyone.
    [Serializable]
    public partial class SceneData
    {
        public const int CurrentVersion = 7;

        public int version = CurrentVersion;
        public string stage;        // Unity scene name (version 1's only key)
        public string savedAt;
        public int mode = (int)GameModes.Playground;
        public string stageKey;     // mode | stage card | scene name or path
        public string stageName;    // shown in the list
        public string scenePath;    // what GameTransitions.LoadScene takes
        public bool hasPlayer;
        public Vector3 playerPos;
        public float playerFacing;
        public RoomRef playerRoom = new RoomRef();
        public bool playerInfAmmo;  // version 7
        public List<CharRecord> chars = new List<CharRecord>();
        // Version 4: Char_Data.myUniqueID of every body in the level at save time, joined by '|'.
        public string dead;
        // Set by a load: saved characters another player plays now, put back by PutPlayers. Not saved.
        internal int playersPut;
        // Set by a load (the list's "Keep my character and weapons" box, 0.10.0): the players keep
        // the weapons they hold now instead of the saved ones. Not saved.
        internal bool keepMine;

        [Serializable]
        public class RoomRef { public string name; public string serial; public Vector3 pos; }

        [Serializable]
        public class CharRecord
        {
            public string card;
            public Vector3 pos;
            public float facing;
            public int squad;       // -1: your squad
            public int team;        // Factions
            public float damage;    // Target_Object.myTrauma
            public int alert;       // AI_Wakeup.AlertStatus
            public bool frozen, guard;
            public List<ItemRecord> items = new List<ItemRecord>();
            public List<PointRecord> patrol = new List<PointRecord>();
            // Version 2
            public RoomRef room = new RoomRef();
            public string serial;   // the level's scripts find characters by it
            public string id;       // Char_Data.myUniqueID: same character while the level stays loaded
            public bool essential;  // player squad, playable or amEssential: never removed or respawned
            public bool wave;       // spawned by a wave or squad spawner
            // Version 5
            public string emote;    // clip name of the looping emote playing at save time
            // Version 6
            public int player;      // 2 to 4: a player on a pad played this character at save time
            // Version 7
            public bool infAmmo;    // Director's infinite ammo (AmmoFeature)
        }

        [Serializable]
        public class ItemRecord
        {
            public string card;
            public int hand;
            public bool thrown;
            // Version 3
            public int slot;        // 0 hands, 1 holsters
            public bool hasAmmo;
            public int ammo;
            public float mags;
        }

        [Serializable]
        public class PointRecord { public Vector3 pos; public bool stop; }

        internal bool IsPlayground { get { return version < 2 || mode == (int)GameModes.Playground; } }

        // ---- the file ----

        // JsonUtility drops every field whose type is one of this mod's own classes (the mod's
        // assembly is loaded at runtime, so Unity's serializer does not know them): 0.4.0 and 0.5.0
        // wrote scenes with no rooms and no characters. JsonUtility still writes and reads each flat
        // record (strings, numbers, Vector3); these join and split the nesting around it.
        internal string ToJson()
        {
            var cs = chars.Select(c => With(JsonUtility.ToJson(c),
                "room", JsonUtility.ToJson(c.room ?? new RoomRef()), "items", Arr(c.items), "patrol", Arr(c.patrol)));
            var fs = (floor ?? new List<FloorRecord>()).Select(f => With(JsonUtility.ToJson(f), "room", JsonUtility.ToJson(f.room ?? new RoomRef())));
            return With(JsonUtility.ToJson(this),
                "playerRoom", JsonUtility.ToJson(playerRoom ?? new RoomRef()), "chars", "[" + string.Join(",\n", cs) + "]",
                "playerItems", Arr(playerItems), "floor", "[" + string.Join(",\n", fs) + "]");
        }

        internal static SceneData FromJson(string json)
        {
            var d = JsonUtility.FromJson<SceneData>(json);
            if (d == null) return null;
            d.playerRoom = Obj<RoomRef>(Field(json, "playerRoom")) ?? new RoomRef();
            d.chars = new List<CharRecord>();
            foreach (var cj in Elements(Field(json, "chars")))
            {
                var c = JsonUtility.FromJson<CharRecord>(cj);
                if (c == null) continue;
                c.room = Obj<RoomRef>(Field(cj, "room")) ?? new RoomRef();
                c.items = Elements(Field(cj, "items")).Select(JsonUtility.FromJson<ItemRecord>).Where(x => x != null).ToList();
                c.patrol = Elements(Field(cj, "patrol")).Select(JsonUtility.FromJson<PointRecord>).Where(x => x != null).ToList();
                d.chars.Add(c);
            }
            d.playerItems = Elements(Field(json, "playerItems")).Select(JsonUtility.FromJson<ItemRecord>).Where(x => x != null).ToList();
            d.floor = new List<FloorRecord>();
            foreach (var fj in Elements(Field(json, "floor")))
            {
                var f = JsonUtility.FromJson<FloorRecord>(fj);
                if (f == null) continue;
                f.room = Obj<RoomRef>(Field(fj, "room")) ?? new RoomRef();
                d.floor.Add(f);
            }
            return d;
        }

        static string Arr<T>(List<T> list)
        {
            return "[" + string.Join(",", (list ?? new List<T>()).Select(x => JsonUtility.ToJson(x))) + "]";
        }

        static T Obj<T>(string json) where T : class
        {
            return string.IsNullOrEmpty(json) || json[0] != '{' ? null : JsonUtility.FromJson<T>(json);
        }

        // Adds "key": value pairs to the end of a JSON object.
        static string With(string obj, params string[] pairs)
        {
            int end = obj.LastIndexOf('}');
            var head = obj.Substring(0, end).TrimEnd();
            for (int i = 0; i < pairs.Length; i += 2)
                head += (head.EndsWith("{") ? "" : ",") + "\n\"" + pairs[i] + "\": " + pairs[i + 1];
            return head + "\n}";
        }

        // The raw value of a top-level key of a JSON object, or null.
        static string Field(string json, string key)
        {
            if (json == null) return null;
            int i = json.IndexOf('{');
            if (i < 0) return null;
            i = Ws(json, i + 1);
            while (i < json.Length && json[i] == '"')
            {
                int e = Skip(json, i);
                var k = json.Substring(i + 1, e - i - 2);
                i = Ws(json, e);
                if (i >= json.Length || json[i] != ':') return null;
                i = Ws(json, i + 1);
                int ve = Skip(json, i);
                if (k == key) return json.Substring(i, ve - i);
                i = Ws(json, ve);
                if (i < json.Length && json[i] == ',') i = Ws(json, i + 1);
            }
            return null;
        }

        // The raw elements of a JSON array (none when it is missing).
        static IEnumerable<string> Elements(string arr)
        {
            if (string.IsNullOrEmpty(arr) || arr[0] != '[') yield break;
            int i = Ws(arr, 1);
            while (i < arr.Length && arr[i] != ']')
            {
                int e = Skip(arr, i);
                yield return arr.Substring(i, e - i);
                i = Ws(arr, e);
                if (i < arr.Length && arr[i] == ',') i = Ws(arr, i + 1);
            }
        }

        static int Ws(string s, int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            return i;
        }

        // The index just past the JSON value that starts at i.
        static int Skip(string s, int i)
        {
            if (i >= s.Length) return i;
            if (s[i] == '"')
            {
                for (i++; i < s.Length && s[i] != '"'; i++) if (s[i] == '\\') i++;
                return Math.Min(i + 1, s.Length);
            }
            if (s[i] == '{' || s[i] == '[')
            {
                int depth = 0;
                while (i < s.Length)
                {
                    char ch = s[i];
                    if (ch == '"') { i = Skip(s, i); continue; }
                    if (ch == '{' || ch == '[') depth++;
                    else if ((ch == '}' || ch == ']') && --depth == 0) return i + 1;
                    i++;
                }
                return i;
            }
            while (i < s.Length && ",}] \t\r\n".IndexOf(s[i]) < 0) i++;
            return i;
        }

        // ---- where we are ----

        internal static bool InPlayground { get { return Game_Manager.currentGameMode == GameModes.Playground; } }

        internal static string ScenePath() { return Game_Manager.GameTransitions.NameOrPathOfCurrentScene(); }

        internal static string StageKey()
        {
            var st = Stage_Manager.currentStage;
            return Game_Manager.currentGameMode + "|" + (st != null ? st.name : "hub") + "|" + ScenePath();
        }

        internal static string StageName()
        {
            var scene = SceneManager.GetActiveScene().name;
            if (InPlayground) return "Playground " + scene;
            var st = Stage_Manager.currentStage;
            if (st == null) return scene;
            if (!string.IsNullOrEmpty(st.FullName)) return st.FullName;
            if (!string.IsNullOrEmpty(st.ShortName)) return st.ShortName;
            return st.name;
        }

        internal string Where { get { return string.IsNullOrEmpty(stageName) ? "Playground " + stage : stageName; } }

        internal bool SavedHere()
        {
            if (version < 2) return InPlayground && stage == SceneManager.GetActiveScene().name;
            return stageKey == StageKey();
        }

        // ---- rooms ----

        internal static RoomRef Ref(Room_Main r)
        {
            return r == null ? new RoomRef() : new RoomRef { name = r.name, serial = r.SerialNumber, pos = r.transform.position };
        }

        // Rooms have no id: match name and serial, then the nearest of those (names can repeat).
        internal static Room_Main FindRoom(RoomRef r)
        {
            if (r == null || string.IsNullOrEmpty(r.name)) return null;
            var rooms = Room_Main.allRooms.Where(x => x != null && x.name == r.name).ToList();
            var same = rooms.Where(x => (x.SerialNumber ?? "") == (r.serial ?? "")).ToList();
            if (same.Count > 0) rooms = same;
            return rooms.OrderBy(x => (x.transform.position - r.pos).sqrMagnitude).FirstOrDefault();
        }

        // In story levels and arenas the game locks and switches off a room you leave.
        internal static bool Usable(Room_Main r)
        {
            return r != null && r.gameObject.activeInHierarchy && !r.amLockedOut;
        }

        // ---- who a scene holds ----

        // Bosses and cutscene actors carry links a respawn cannot rebuild; scenes leave them alone.
        internal static bool Untouchable(Combatant_Base c)
        {
            return c.myCutscene != null || (c.myController != null && c.myController.AmType_Boss());
        }

        internal static bool Essential(Combatant_Base c)
        {
            if (c.amEssential || c == SwapUnawareBehaviour.Home || Game.Playable(c)) return true;
            var sq = c.myData != null ? c.myData.mySquad : null;
            return sq != null && Squad_Main.AmPlayerSquad(sq);
        }

        internal static Quaternion Facing(float y) { return Quaternion.Euler(0f, y, 0f); }

        internal static SceneData Capture()
        {
            bool pg = InPlayground;
            var d = new SceneData
            {
                stage = SceneManager.GetActiveScene().name,
                savedAt = DateTime.Now.ToString("s"),
                mode = (int)Game_Manager.currentGameMode,
                stageKey = StageKey(),
                stageName = StageName(),
                scenePath = ScenePath(),
            };
            var me = Game.Me();
            if (me != null)
            {
                d.hasPlayer = true;
                d.playerPos = me.Position;
                d.playerFacing = me.transform.eulerAngles.y;
                d.playerRoom = Ref(me.myRoom);
                d.playerItems = Loadout(me);
                d.playerInfAmmo = AmmoFeature.Has(me);
            }
            var mySquad = me != null && me.myData != null ? me.myData.mySquad : null;
            var squads = new List<Squad_Main>();
            var bodies = new List<string>();
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (Game.Corpse(c) && !string.IsNullOrEmpty(c.myData.myUniqueID)) { bodies.Add(c.myData.myUniqueID); continue; }
                if (!Game.Alive(c) || c == me || c.myData == null || c.myData.myCard == null || !(c.myController is Controller_NPC || Game.IsPlayer(c))) continue;
                if (!pg && Untouchable(c)) continue;
                var sq = c.myData.mySquad;
                int si = -2;
                if (sq != null && sq == mySquad) si = -1;
                else if (sq != null) { si = squads.IndexOf(sq); if (si < 0) { squads.Add(sq); si = squads.Count - 1; } }
                var r = new CharRecord
                {
                    card = c.myData.myCard.name,
                    pos = c.Position,
                    facing = c.transform.eulerAngles.y,
                    squad = si,
                    team = (int)c.myFaction,
                    damage = c.myTrauma,
                    alert = (int)c.myController.myAlertStatus,
                    frozen = FreezeFeature.IsFrozen(c),
                    guard = GuardFeature.IsGuard(c),
                    room = Ref(c.myRoom),
                    serial = c.SerialNumber,
                    id = c.myData.myUniqueID,
                    essential = Essential(c),
                    wave = c.myWave != null,
                    emote = IdleAnimFeature.CurrentEmote(c),
                    player = Game.PlayerNumber(c),
                    infAmmo = AmmoFeature.Has(c),
                };
                r.items = Loadout(c);
                PatrolFeature.Route route;
                if (PatrolFeature.routes.TryGetValue(c, out route))
                    foreach (var p in route.points) r.patrol.Add(new PointRecord { pos = p.pos, stop = p.stop });
                d.chars.Add(r);
            }
            d.dead = string.Join("|", bodies.ToArray());
            d.floor = CaptureFloor();
            SceneMess.Mark(d);
            return d;
        }

        // ---- loading in the Playground: clear the room, spawn everyone (0.4.0 behaviour) ----

        internal class PlaygroundResult
        {
            public int chars, kept, rejoined;
            public bool yours;
            public FloorResult floor;
            public List<KeyValuePair<Combatant_Base, CharRecord>> placed = new List<KeyValuePair<Combatant_Base, CharRecord>>();
        }

        internal PlaygroundResult RestorePlayground()
        {
            FreezeFeature.ReleaseAll();
            ClearRoom();
            var me = Game.Me();
            RestoreAmmo(me);
            var res = new PlaygroundResult();
            var todo = PutPlayers(true);
            // Clear All empties only the room you are in. Saved characters still standing in another
            // room are put back rather than doubled, and the respawned join their squads (0.7.5).
            var byId = new Dictionary<string, Combatant_Base>();
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
                if (Game.IsNpc(c) && c != me && !c.AmDespawning && !string.IsNullOrEmpty(c.myData.myUniqueID))
                    byId[c.myData.myUniqueID] = c;
            var kept = new Dictionary<CharRecord, Combatant_Base>();
            foreach (var r in todo)
            {
                Combatant_Base c;
                if (!string.IsNullOrEmpty(r.id) && byId.TryGetValue(r.id, out c)) { kept[r] = c; byId.Remove(r.id); }
            }
            var squads = new Dictionary<string, Squad_Main>();
            SeedSquads(kept, squads);
            foreach (var r in todo)
            {
                var room = FindRoom(r.room);
                if (!Usable(room)) room = Cam_Main.currentRoom;
                Combatant_Base c;
                if (kept.TryGetValue(r, out c)) { Put(c, r, room, HasItems); res.kept++; }
                else
                {
                    Squad_Main mine = r.squad == -1 && me != null && me.myData != null ? me.myData.mySquad : null;
                    c = Spawn(r, room, squads, mine, HasItems);
                    if (c == null) continue;
                }
                res.chars++;
                res.placed.Add(new KeyValuePair<Combatant_Base, CharRecord>(c, r));
            }
            res.rejoined = Rejoin(res.placed, squads);
            res.yours = HasItems && hasPlayer && !keepMine && ResetLoadout(me, playerItems);
            res.floor = HasItems ? RestoreFloor() : new FloorResult();
            return res;
        }

        // ---- loading in a story level, arena or custom stage ----

        internal class Result
        {
            public int moved, respawned, removed, skipped, yourAmmo, corpses, mess, rejoined;
            public FloorResult floor = new FloorResult();
            // Everyone put back or respawned, for the check a moment later of who has walked off.
            public List<KeyValuePair<Combatant_Base, CharRecord>> placed = new List<KeyValuePair<Combatant_Base, CharRecord>>();
        }

        // Called once you stand in your saved room (its own characters have spawned by then).
        // Matches the characters already there to the saved ones, puts them back, respawns the
        // missing, then removes unscripted extras. Respawning first keeps every faction alive, so
        // no "faction dead" trigger fires; each removal still counts as a kill for the level.
        internal Result RestoreLevel()
        {
            var res = new Result();
            FreezeFeature.ReleaseAll();
            var me = Game.Me();
            RestoreAmmo(me);
            var todo = PutPlayers(false);
            var live = UnityEngine.Object.FindObjectsOfType<Combatant_Base>()
                .Where(c => Game.IsNpc(c) && c != me && !Untouchable(c) && c.myData != null && c.myData.myCard != null && Usable(c.myRoom))
                .ToList();
            var match = Match(live, todo);

            // A respawned character joins the live squad of its saved squadmates; a fresh squad of
            // its own would be a foe to them when the faction is None, as Playground spawns are.
            var squads = new Dictionary<string, Squad_Main>();
            SeedSquads(match, squads);
            var respawned = new HashSet<string>();
            foreach (var r in todo)
            {
                try
                {
                    Combatant_Base c;
                    var room = FindRoom(r.room);
                    if (match.TryGetValue(r, out c))
                    {
                        live.Remove(c);
                        var from = c.Position;
                        var items = Put(c, r, Usable(room) ? room : null, HasItems);
                        res.moved++;
                        res.placed.Add(new KeyValuePair<Combatant_Base, CharRecord>(c, r));
                        Line(r, "put back (" + How(r, c) + "), moved " + Vector3.Distance(from, c.Position).ToString("0.0") + " m"
                            + (Usable(room) ? "" : " (room closed: left where it was)"), items, c);
                        continue;
                    }
                    if (r.essential || !Usable(room)) { res.skipped++; Line(r, r.essential ? "skipped: essential, not respawned" : "skipped: room closed", null); continue; }
                    string given;
                    c = Spawn(r, room, squads, null, HasItems, out given);
                    if (c == null) { res.skipped++; Line(r, "skipped: respawn failed", null); continue; }
                    if (!string.IsNullOrEmpty(r.serial)) c.SetSerialNumber(r.serial);
                    if (r.wave) JoinWave(c);
                    if (!string.IsNullOrEmpty(r.id)) respawned.Add(r.id);
                    res.respawned++;
                    res.placed.Add(new KeyValuePair<Combatant_Base, CharRecord>(c, r));
                    Line(r, "respawned", given, c);
                }
                catch (Exception e) { Game.Error("scene: restore " + r.card, e); res.skipped++; }
            }
            res.rejoined = Rejoin(res.placed, squads);
            res.corpses = ClearNewBodies(respawned);
            res.mess = SceneMess.ClearNewer(this);

            // Left over: characters the scene does not hold. The level's scripted ones (serial
            // numbers) and the player's squad stay; the rest go the way F1 removes them.
            Game.PruneClaimants();
            foreach (var c in live)
            {
                if (Essential(c) || c.HasSerialNumber() || c == SwapUnawareBehaviour.Home) continue;
                try
                {
                    Game.ReleaseClaims(c.myController);
                    c.RemoveFromGame();
                    res.removed++;
                }
                catch (Exception e) { Game.Error("scene: remove " + c.name, e); }
            }
            // Your weapons belong to the career here: the same weapons go back in your hands with
            // their saved ammo. Before the floor, which would clear one you threw.
            if (HasItems && hasPlayer && !keepMine) res.yourAmmo = RefillAmmo(me, playerItems);
            // Last, so weapons dropped by the bodies removed above go too.
            if (HasItems) res.floor = RestoreFloor();
            return res;
        }

        // One Player.log line per saved character, so a playtest can tell which one went wrong.
        static void Line(CharRecord r, string what, string items, Combatant_Base c = null)
        {
            Game.Log("scene:   " + r.card + (string.IsNullOrEmpty(r.serial) ? "" : " [" + r.serial + "]") + " " + what
                + (items == null ? "" : "; weapons: " + (items.Length > 0 ? items : "none"))
                + (c == null ? "" : "; " + SquadOf(c) + ", saved squad " + r.squad + " " + (Factions)r.team));
        }

        internal static string SquadOf(Combatant_Base c)
        {
            var sq = c.myData != null ? c.myData.mySquad : null;
            return "squad " + (sq != null ? sq.squadName : "none") + " " + c.myFaction;
        }

        // Infinite ammo starts from nobody, then each saved character gets theirs back (Extras,
        // PutPlayers) and you get yours. The keep box leaves every player's as it is.
        void RestoreAmmo(Combatant_Base me)
        {
            AmmoFeature.ClearAll(keepMine);
            if (!keepMine) AmmoFeature.Set(me, playerInfAmmo);
        }

        // ---- the other players (local co-op, version 6) ----

        // The Playground's Clear All (InteractiveMode.ClearAll_DoIt) spares only player one's body
        // and weapons, so a second player's character would go too. The same steps, sparing every player.
        static void ClearRoom()
        {
            Decay_Manager.ClearAll_Character();
            Decay_Manager.ClearAll_Holes();
            Decay_Manager.ClearAll_Casing();
            Decay_Manager.ClearAll_Splats();
            Decay_Manager.ClearAll_Debris();
            var room = Cam_Main.currentRoom;
            if (room == null) return;
            foreach (var o in new List<Target_Object>(room.myOccupants))
                if (!Game.IsPlayer(o as Combatant_Base)) o.RemoveFromGame();
            foreach (var it in room.GetComponentsInChildren<Item_Held>())
                if (!Game.IsPlayer(it.myOwner)) it.DeSpawn();
        }

        // A saved character a player on a pad now plays (saved as theirs, or a squadmate they took
        // over since) goes back to its spot with its saved weapons (in a story or arena career only
        // the ammo, as yours), and nothing else: no alert, freeze, guard, patrol or squad change,
        // and never a respawn or removal. Returns the records left for the usual restore.
        // The body you play now is never doubled: if the scene holds it (taken over with F5 since
        // the save), its record is left out and the body stays as it is (0.10.0).
        List<CharRecord> PutPlayers(bool playground)
        {
            var bodies = new Dictionary<string, Combatant_Base>();
            foreach (var c in Game.OtherPlayers())
                if (c.myData != null && !string.IsNullOrEmpty(c.myData.myUniqueID)) bodies[c.myData.myUniqueID] = c;
            var me = Game.Me();
            string myId = me != null && me.myData != null ? me.myData.myUniqueID : null;
            var todo = new List<CharRecord>();
            playersPut = 0;
            foreach (var r in chars)
            {
                if (!string.IsNullOrEmpty(myId) && r.id == myId)
                {
                    Game.Log("scene:   " + r.card + " is the body you play now; left as it is");
                    continue;
                }
                Combatant_Base c;
                if (string.IsNullOrEmpty(r.id) || !bodies.TryGetValue(r.id, out c))
                {
                    if (r.player > 0) Game.Log("scene:   " + r.card + " was played by player " + r.player + " at save time; no player plays it now, so it loads as a squadmate");
                    todo.Add(r);
                    continue;
                }
                bodies.Remove(r.id);
                playersPut++;
                try
                {
                    var from = c.Position;
                    var room = FindRoom(r.room);
                    if (!Usable(room) && playground) room = Cam_Main.currentRoom;
                    if (Usable(room)) c.TeleportTo(r.pos, Facing(r.facing) * Vector3.forward, room);
                    string what = "";
                    if (!keepMine) AmmoFeature.Set(c, r.infAmmo);
                    if (HasItems && keepMine) what = "; weapons kept";
                    else if (HasItems)
                    {
                        if (playground) what = ResetLoadout(c, r.items) ? "; weapons reset" : "; weapons not reset";
                        else what = "; " + RefillAmmo(c, r.items) + " of " + r.items.Count + " weapons reset";
                    }
                    Game.Log("scene:   " + r.card + " played by player " + Game.PlayerNumber(c) + (r.player > 0 ? "" : " (a squadmate at save time)")
                        + ": put back, moved " + Vector3.Distance(from, c.Position).ToString("0.0") + " m" + what);
                }
                catch (Exception e) { Game.Error("scene: player " + Game.PlayerNumber(c), e); }
            }
            foreach (var c in bodies.Values)
                Game.Log("scene:   player " + Game.PlayerNumber(c) + " (" + c.name + ") was not in the scene; left where they are");
            return todo;
        }

        // ---- squads ----

        // -1 is your squad; -2 had none, so those share one squad per faction.
        static string SquadKey(CharRecord r) { return r.squad == -2 ? "-2|" + r.team : r.squad.ToString(); }

        // The live squad each saved squad maps to, taken from the characters put back (the same
        // character first, then by serial, then the nearest) whose faction is still the saved one.
        static void SeedSquads(Dictionary<CharRecord, Combatant_Base> match, Dictionary<string, Squad_Main> squads)
        {
            foreach (var p in match.OrderBy(p => How(p.Key, p.Value).StartsWith("same") ? 0 : How(p.Key, p.Value).StartsWith("by") ? 1 : 2))
            {
                var r = p.Key;
                var sq = p.Value.myData.mySquad;
                if (r.squad == -2 || sq == null || p.Value.myFaction != (Factions)r.team || squads.ContainsKey(SquadKey(r))) continue;
                squads[SquadKey(r)] = sq;
            }
        }

        // A character put back in a squad that is not its saved squadmates', where the two would
        // fight (a None faction, or a faction changed since the save), joins theirs. Your squad
        // and essential characters are never moved. Returns how many moved.
        static int Rejoin(List<KeyValuePair<Combatant_Base, CharRecord>> placed, Dictionary<string, Squad_Main> squads)
        {
            int n = 0;
            foreach (var p in placed)
            {
                var c = p.Key;
                var r = p.Value;
                Squad_Main sq;
                if (r.squad < 0 || r.essential || !Game.Alive(c) || !squads.TryGetValue(SquadKey(r), out sq) || c.myData.mySquad == sq) continue;
                if (!Combat_Manager.FactionIsAttackable(c.myFaction, sq.squadFaction)) continue;
                try
                {
                    var was = SquadOf(c);
                    sq.AddMember(c.myData);
                    // AddMember takes a wave unit out of its wave unless the new squad has one.
                    if (r.wave && c.myWave == null) JoinWave(c);
                    Game.ReleaseClaims(c.myController);
                    n++;
                    Game.Log("scene:   " + r.card + " rejoined its saved squadmates: " + was + " -> " + SquadOf(c));
                }
                catch (Exception e) { Game.Error("scene: rejoin " + r.card, e); }
            }
            return n;
        }

        // Saved squadmates now hostile to each other, for the check a moment after a load.
        internal static List<string> HostileSquadmates(List<KeyValuePair<Combatant_Base, CharRecord>> placed)
        {
            var found = new List<string>();
            var me = Game.Me();
            foreach (var g in placed.Where(p => p.Value.squad != -2 && Game.Alive(p.Key)).GroupBy(p => p.Value.squad))
            {
                var list = g.Select(p => p.Key).ToList();
                if (g.Key == -1)
                {
                    if (me != null) list.Add(me);
                    list.AddRange(Game.OtherPlayers().Where(x => !list.Contains(x)));
                }
                for (int i = 0; i < list.Count; i++)
                    for (int j = i + 1; j < list.Count; j++)
                        if (Combat_Manager.FactionIsAttackable(list[i], list[j]) || Combat_Manager.FactionIsAttackable(list[j], list[i]))
                            found.Add(list[i].name + " (" + SquadOf(list[i]) + ") and " + list[j].name + " (" + SquadOf(list[j]) + ")");
            }
            return found;
        }

        // ---- emotes (version 5) ----

        // Puts the looping emote each character was playing back, a moment after the load (a
        // character just respawned or teleported cannot emote yet). Only for characters still
        // unaware: one that has noticed a fight would drop the pose anyway. Returns how many.
        internal static int Repose(List<KeyValuePair<Combatant_Base, CharRecord>> placed)
        {
            int n = 0;
            foreach (var p in placed)
            {
                var c = p.Key;
                var r = p.Value;
                if (string.IsNullOrEmpty(r.emote) || !Game.Alive(c) || c.myController.myAlertStatus != AI_Wakeup.AlertStatus.Unaware) continue;
                try
                {
                    var clip = IdleAnimFeature.FindClip(c, r.emote);
                    if (clip == null) { Game.Log("scene:   " + r.card + ": animation " + r.emote + " not found"); continue; }
                    // A frozen character's animator stands still: let it go, start the pose, freeze it again.
                    bool frozen = FreezeFeature.IsFrozen(c);
                    if (frozen) FreezeFeature.Release(c);
                    bool ok = IdleAnimFeature.Pose(c, clip, false);
                    if (frozen) FreezeFeature.Freeze(c);
                    if (ok) n++;
                    Game.Log("scene:   " + r.card + (ok ? " plays " : " could not play ") + r.emote + " again");
                }
                catch (Exception e) { Game.Error("scene: emote " + r.card, e); }
            }
            return n;
        }

        static string How(CharRecord r, Combatant_Base c)
        {
            if (!string.IsNullOrEmpty(r.id) && c.myData.myUniqueID == r.id) return "same character";
            if (!string.IsNullOrEmpty(r.serial) && c.SerialNumber == r.serial) return "by serial";
            return "nearest " + r.card;
        }

        // The bodies of characters who died after the save: those respawned above (their old
        // bodies), and in a version 4 file every other body that was not lying there at save time.
        // Kept as Clear corpses keeps them (Director.cs): squad, playable, boss, cutscene, scripted,
        // essential, and bodies still running their death. Rooms the game has closed are skipped.
        int ClearNewBodies(HashSet<string> respawned)
        {
            var old = version >= 4 ? new HashSet<string>((dead ?? "").Split('|')) : null;
            var me = Game.Me();
            int n = 0;
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (!Game.Corpse(c) || c == me || Game.IsPlayer(c) || !Usable(c.myRoom)) continue;
                var id = c.myData.myUniqueID ?? "";
                bool mine = respawned.Contains(id);
                if (!mine && (old == null || old.Contains(id) || c.HasSerialNumber())) continue;
                if (Essential(c) || Untouchable(c) || !Decay_Manager.ReturnCanCondemnCharacter(c) || !c.ReturnAppearDeadAndInert()) continue;
                try
                {
                    Decay_Manager.DestroyMe_Character(c);
                    n++;
                }
                catch (Exception e) { Game.Error("scene: clear body " + c.name, e); }
            }
            return n;
        }

        // Same character first (the level is still loaded), then serial number, then the nearest
        // character with the same card in the same room.
        static Dictionary<CharRecord, Combatant_Base> Match(List<Combatant_Base> live, List<CharRecord> chars)
        {
            var m = new Dictionary<CharRecord, Combatant_Base>();
            var used = new HashSet<Combatant_Base>();
            foreach (var r in chars)
            {
                var c = live.FirstOrDefault(x => !used.Contains(x) && !string.IsNullOrEmpty(r.id) && x.myData.myUniqueID == r.id);
                if (c != null) { m[r] = c; used.Add(c); }
            }
            foreach (var r in chars)
            {
                if (m.ContainsKey(r) || string.IsNullOrEmpty(r.serial)) continue;
                var c = live.FirstOrDefault(x => !used.Contains(x) && x.SerialNumber == r.serial);
                if (c != null) { m[r] = c; used.Add(c); }
            }
            var pairs = new List<KeyValuePair<CharRecord, Combatant_Base>>();
            foreach (var r in chars)
                if (!m.ContainsKey(r))
                    foreach (var c in live)
                        if (!used.Contains(c) && c.myData.myCard.name == r.card && (c.myRoom == null ? "" : c.myRoom.name) == (r.room.name ?? ""))
                            pairs.Add(new KeyValuePair<CharRecord, Combatant_Base>(r, c));
            foreach (var p in pairs.OrderBy(p => (p.Value.Position - p.Key.pos).sqrMagnitude))
            {
                if (m.ContainsKey(p.Key) || used.Contains(p.Value)) continue;
                m[p.Key] = p.Value;
                used.Add(p.Value);
            }
            return m;
        }

        // Puts a character that is already in the level back the way it was saved, weapons and ammo
        // too (items: a version 3 file). room null: its saved room is closed now, so it stays where it is.
        // Returns what it gave back, for the log (null without items).
        static string Put(Combatant_Base c, CharRecord r, Room_Main room, bool items)
        {
            if (GuardFeature.IsGuard(c)) GuardFeature.Unguard(c);
            PatrolFeature.DropRoute(c);
            Game.ReleaseClaims(c.myController);
            if (room != null)
            {
                // TeleportTo ends one combat order only; a walk the level or the fight ordered after
                // the save would take the character straight back off its spot. The level's
                // standing orders (persistent or repeating) stay.
                Orders_Manager.ClearMoveOrders(c, Orders_Manager.OrderUrgency.Combat, true);
                Orders_Manager.ClearMoveOrders(c, Orders_Manager.OrderUrgency.Idle, true);
                c.TeleportTo(r.pos, Facing(r.facing) * Vector3.forward, room);
            }
            c.myTrauma = r.damage;
            Controller_Base.SetAlertStatus(c.myController, (AI_Wakeup.AlertStatus)r.alert);
            string given = null;
            if (items) ResetLoadout(c, r.items, out given);
            Extras(c, r);
            return given;
        }

        static void Extras(Combatant_Base c, CharRecord r)
        {
            if (r.patrol.Count > 0) PatrolFeature.SetRoute(c, r.patrol.Select(p => new PatrolFeature.Point { pos = p.pos, stop = p.stop }).ToList());
            if (r.guard) GuardFeature.Guard(c);
            if (r.frozen) FreezeFeature.Freeze(c);
            if (r.infAmmo) AmmoFeature.Set(c, true);
        }

        // mine: the squad that "your squad" (-1) records join; null gives them a squad of their own,
        // so story careers never store them. items: a version 3 file, whose loadout replaces any
        // weapons the card hands out (older files only add theirs).
        static Combatant_Base Spawn(CharRecord r, Room_Main room, Dictionary<string, Squad_Main> squads, Squad_Main mine, bool items)
        {
            string given;
            return Spawn(r, room, squads, mine, items, out given);
        }

        static Combatant_Base Spawn(CharRecord r, Room_Main room, Dictionary<string, Squad_Main> squads, Squad_Main mine, bool items, out string given)
        {
            given = null;
            try
            {
                var card = StatCard_Base.ReturnCardByName<StatCard_Character>(r.card);
                if (card == null) { Debug.Log("[" + ModEntry.Id + "] scene: no card " + r.card); return null; }
                var data = Char_Data.CreateCharacter(card);
                var c = Combatant_Base.CreateCombatant(data, room, Controller_Base.ControllerType.NPC, r.pos, Facing(r.facing), false);
                if (c == null) return null;
                Squad_Main sq = r.squad == -1 ? mine : null;
                if (sq == null && !squads.TryGetValue(SquadKey(r), out sq)) { sq = Squad_Main.CreateSquad((Factions)r.team); squads[SquadKey(r)] = sq; }
                sq.AddMember(c.myData);
                if (items) ResetLoadout(c, r.items, out given);
                else foreach (var it in r.items) Give(c, it);
                c.myTrauma = r.damage;
                Controller_Base.SetAlertStatus(c.myController, (AI_Wakeup.AlertStatus)r.alert);
                Extras(c, r);
                return c;
            }
            catch (Exception e) { Game.Error("scene: spawn " + r.card, e); return null; }
        }

        // A respawned wave unit joins the wave now running (the game's own WaveContainer.AddUnit,
        // which battle rooms use), so the wave waits for it and counts its death.
        static void JoinWave(Combatant_Base c)
        {
            var w = UnityEngine.Object.FindObjectsOfType<Combatant_Base>()
                .Where(x => x != c && Game.Alive(x) && x.myWave != null)
                .OrderByDescending(x => x.myFaction == c.myFaction)
                .Select(x => x.myWave).FirstOrDefault();
            if (w != null) w.AddUnit(c);
        }

    }
}
