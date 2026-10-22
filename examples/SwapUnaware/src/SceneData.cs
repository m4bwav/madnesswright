using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // A saved scene. Version 1 (0.4.0) was Playground-only and keyed by the Unity scene name;
    // version 2 adds the stage key, each character's room and serial number, and your place;
    // version 3 adds holsters, ammo, your loadout and loose weapons (SceneItems.cs).
    [Serializable]
    public partial class SceneData
    {
        public const int CurrentVersion = 3;

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
        public List<CharRecord> chars = new List<CharRecord>();

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
                if (pg) d.playerItems = Loadout(me);
            }
            var mySquad = me != null && me.myData != null ? me.myData.mySquad : null;
            var squads = new List<Squad_Main>();
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Combatant_Base>())
            {
                if (!Game.Alive(c) || c == me || c.myData == null || c.myData.myCard == null || !(c.myController is Controller_NPC)) continue;
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
                };
                r.items = Loadout(c);
                PatrolFeature.Route route;
                if (PatrolFeature.routes.TryGetValue(c, out route))
                    foreach (var p in route.points) r.patrol.Add(new PointRecord { pos = p.pos, stop = p.stop });
                d.chars.Add(r);
            }
            d.floor = CaptureFloor();
            return d;
        }

        // ---- loading in the Playground: clear the room, spawn everyone (0.4.0 behaviour) ----

        internal class PlaygroundResult { public int chars; public bool yours; public FloorResult floor; }

        internal PlaygroundResult RestorePlayground()
        {
            FreezeFeature.ReleaseAll();
            InteractiveMode.ClearAll_DoIt();
            var me = Game.Me();
            var squads = new Dictionary<int, Squad_Main>();
            int n = 0;
            foreach (var r in chars)
            {
                var room = FindRoom(r.room);
                if (!Usable(room)) room = Cam_Main.currentRoom;
                Squad_Main mine = r.squad == -1 && me != null && me.myData != null ? me.myData.mySquad : null;
                if (Spawn(r, room, squads, mine, HasItems) != null) n++;
            }
            bool yours = HasItems && hasPlayer && ResetLoadout(me, playerItems);
            return new PlaygroundResult { chars = n, yours = yours, floor = HasItems ? RestoreFloor() : new FloorResult() };
        }

        // ---- loading in a story level, arena or custom stage ----

        internal class Result { public int moved, respawned, removed, skipped; public FloorResult floor = new FloorResult(); }

        // Called once you stand in your saved room (its own characters have spawned by then).
        // Matches the characters already there to the saved ones, puts them back, respawns the
        // missing, then removes unscripted extras. Respawning first keeps every faction alive, so
        // no "faction dead" trigger fires; each removal still counts as a kill for the level.
        internal Result RestoreLevel()
        {
            var res = new Result();
            FreezeFeature.ReleaseAll();
            var me = Game.Me();
            var live = UnityEngine.Object.FindObjectsOfType<Combatant_Base>()
                .Where(c => Game.IsNpc(c) && c != me && !Untouchable(c) && c.myData != null && c.myData.myCard != null && Usable(c.myRoom))
                .ToList();
            var match = Match(live);

            var squads = new Dictionary<int, Squad_Main>();
            foreach (var r in chars)
            {
                try
                {
                    Combatant_Base c;
                    var room = FindRoom(r.room);
                    if (match.TryGetValue(r, out c))
                    {
                        live.Remove(c);
                        Put(c, r, Usable(room) ? room : null, HasItems);
                        res.moved++;
                        continue;
                    }
                    if (r.essential || !Usable(room)) { res.skipped++; continue; }
                    c = Spawn(r, room, squads, null, HasItems);
                    if (c == null) { res.skipped++; continue; }
                    if (!string.IsNullOrEmpty(r.serial)) c.SetSerialNumber(r.serial);
                    if (r.wave) JoinWave(c);
                    res.respawned++;
                }
                catch (Exception e) { Game.Error("scene: restore " + r.card, e); res.skipped++; }
            }

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
            // Last, so weapons dropped by the bodies removed above go too.
            if (HasItems) res.floor = RestoreFloor();
            return res;
        }

        // Same character first (the level is still loaded), then serial number, then the nearest
        // character with the same card in the same room.
        Dictionary<CharRecord, Combatant_Base> Match(List<Combatant_Base> live)
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
        static void Put(Combatant_Base c, CharRecord r, Room_Main room, bool items)
        {
            if (GuardFeature.IsGuard(c)) GuardFeature.Unguard(c);
            PatrolFeature.DropRoute(c);
            Game.ReleaseClaims(c.myController);
            if (room != null) c.TeleportTo(r.pos, Facing(r.facing) * Vector3.forward, room);
            c.myTrauma = r.damage;
            Controller_Base.SetAlertStatus(c.myController, (AI_Wakeup.AlertStatus)r.alert);
            if (items) ResetLoadout(c, r.items);
            Extras(c, r);
        }

        static void Extras(Combatant_Base c, CharRecord r)
        {
            if (r.patrol.Count > 0) PatrolFeature.SetRoute(c, r.patrol.Select(p => new PatrolFeature.Point { pos = p.pos, stop = p.stop }).ToList());
            if (r.guard) GuardFeature.Guard(c);
            if (r.frozen) FreezeFeature.Freeze(c);
        }

        // mine: the squad that "your squad" (-1) records join; null gives them a squad of their own,
        // so story careers never store them. items: a version 3 file, whose loadout replaces any
        // weapons the card hands out (older files only add theirs).
        static Combatant_Base Spawn(CharRecord r, Room_Main room, Dictionary<int, Squad_Main> squads, Squad_Main mine, bool items)
        {
            try
            {
                var card = StatCard_Base.ReturnCardByName<StatCard_Character>(r.card);
                if (card == null) { Debug.Log("[" + ModEntry.Id + "] scene: no card " + r.card); return null; }
                var data = Char_Data.CreateCharacter(card);
                var c = Combatant_Base.CreateCombatant(data, room, Controller_Base.ControllerType.NPC, r.pos, Facing(r.facing), false);
                if (c == null) return null;
                Squad_Main sq = r.squad == -1 ? mine : null;
                if (sq == null && !squads.TryGetValue(r.squad, out sq)) { sq = Squad_Main.CreateSquad((Factions)r.team); squads[r.squad] = sq; }
                sq.AddMember(c.myData);
                if (items) ResetLoadout(c, r.items);
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
