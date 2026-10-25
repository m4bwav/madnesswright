using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // Arena co-op. A pad that presses Start in an arena with no free squadmate waits forever
    // (Joystick_Manager.PlayersWaitingForAdd; arenas lack the story's door bonus). Here it gets a
    // panel driven by that pad: browse the origins player one's career has unlocked, pick one, and
    // a guest built from that origin (StatCard_Origin, at player one's level) appears beside player
    // one under that pad's control. The guest is in the player squad but not in the career: cash,
    // XP and checkpoints stay player one's, its death never ends the wave (DeathEvent_ArenaPlayer
    // ignores characters with no career), and it is back when the wave is over. Arena stages only,
    // never the HQ. Game side: ai-docs/notes/2026-10-24-arena-coop-research.md.
    public class ArenaGuestFeature : Feature
    {
        class Guest
        {
            public StatCard_Origin origin;
            public Combatant_Base body;
        }

        class Picker
        {
            public int player;
            public int index;
            public List<StatCard_Origin> origins;
        }

        static readonly FieldInfo waitingField = typeof(Joystick_Manager).GetField("PlayersWaitingForAdd", BindingFlags.NonPublic | BindingFlags.Static);

        readonly Dictionary<int, Guest> guests = new Dictionary<int, Guest>();
        readonly Dictionary<int, string> lastPick = new Dictionary<int, string>();
        readonly HashSet<int> declined = new HashSet<int>();
        Picker picker;
        GUIStyle style;

        protected override string Name { get { return "arena guests"; } }

        static string PicksFile { get { return Path.Combine(Path.Combine(Application.persistentDataPath, "PlaygroundDirector"), "arena-guests.txt"); } }

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            try
            {
                if (File.Exists(PicksFile))
                    foreach (var line in File.ReadAllLines(PicksFile))
                    {
                        var kv = line.Split('=');
                        int n;
                        if (kv.Length == 2 && int.TryParse(kv[0], out n)) lastPick[n] = kv[1].Trim();
                    }
            }
            catch (Exception e) { Game.Error("arena guests: read picks", e); }
        }

        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

        // A new scene (the HQ, a retry, the next arena): guests do not travel. Taken out of the
        // player squad here, before the scene's spawner reads the roster.
        void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            picker = null;
            var squad = Squad_Main.playerSquad;
            foreach (var g in guests.Values)
                try { if (squad != null && g.body != null && g.body.myData != null) squad.RemoveMember(g.body.myData); }
                catch (Exception e) { Game.Error("arena guests: drop", e); }
            if (guests.Count > 0) Game.Log("arena guests: " + guests.Count + " left the squad with the scene change");
            guests.Clear();
        }

        static bool InArena()
        {
            var stage = Stage_Manager.currentStage as StatCard_Stage_Arena;
            return Game_Manager.currentGameMode == GameModes.Arena && stage != null && !stage.AmHub
                && Headquarters_Manager.currentManager == null && Game_Manager.currentCareer != null && Game.Alive(Game.Me());
        }

        static List<int> Waiting()
        {
            return waitingField != null ? waitingField.GetValue(null) as List<int> : null;
        }

        protected override void Tick()
        {
            if (!InArena()) { picker = null; return; }
            if (picker != null) { Drive(); return; }
            declined.RemoveWhere(n => !Joystick_Manager.ReturnPlayerActive(n));
            var waiting = Waiting();
            if (waiting != null)
                foreach (int n in waiting)
                    if (n > 0 && Joystick_Manager.ReturnPlayerActive(n) && !declined.Contains(n)) { Open(n); return; }
            KeepGuests();
        }

        // The origins the career has unlocked, in the creator's order (UI_CharCreator.UpdateOrigins).
        static List<StatCard_Origin> Unlocked()
        {
            int imprints = Game_Manager.currentCareer.Imprints;
            var list = StatCard_Base.ReturnAllCardsByType<StatCard_Origin>()
                .Where(o => o != null && o.amEnabled && o.CharacterType != null && o.ImprintsTilUnlock <= imprints).ToList();
            list.Sort((a, b) => a.ImprintsTilUnlock != b.ImprintsTilUnlock ? a.ImprintsTilUnlock.CompareTo(b.ImprintsTilUnlock) : string.Compare(a.ShortName, b.ShortName, StringComparison.Ordinal));
            return list;
        }

        void Open(int n)
        {
            var origins = Unlocked();
            if (origins.Count == 0) { Game.Log("arena guests: no unlocked origin found"); return; }
            string last;
            int index = 0;
            if (lastPick.TryGetValue(n, out last)) index = Math.Max(0, origins.FindIndex(o => o.name == last));
            else
            {
                var mine = Game_Manager.currentCareer.chars_Core[0].myOrigins;
                if (mine != null && mine.Count > 0) index = Math.Max(0, origins.IndexOf(mine[mine.Count - 1]));
            }
            picker = new Picker { player = n, index = index, origins = origins };
            Game.Log("arena guests: player " + (n + 1) + " is picking an origin (" + origins.Count + " unlocked)");
        }

        // The waiting pad's D-pad browses, A picks, B cancels (the pad then waits as before).
        void Drive()
        {
            int n = picker.player;
            int pad = Joystick_Manager.ReturnJoystickOfPlayer(n);
            var waiting = Waiting();
            if (pad < 0 || waiting == null || !waiting.Contains(n)) { picker = null; return; }
            int count = picker.origins.Count;
            if (Joystick_Manager.Check_DpadPressed(Joystick_Manager.MenuButton_Dpad.Right, pad, false)
                || Joystick_Manager.Check_DpadPressed(Joystick_Manager.MenuButton_Dpad.Down, pad, false)) picker.index = (picker.index + 1) % count;
            else if (Joystick_Manager.Check_DpadPressed(Joystick_Manager.MenuButton_Dpad.Left, pad, false)
                || Joystick_Manager.Check_DpadPressed(Joystick_Manager.MenuButton_Dpad.Up, pad, false)) picker.index = (picker.index + count - 1) % count;
            else if (Joystick_Manager.Check_ButtonPressed(Joystick_Manager.MenuButton_Main.Cancel, pad, false))
            {
                Game.Say("player " + (n + 1) + " cancelled; leave (Start with both shoulders) and press Start to pick again");
                declined.Add(n);
                picker = null;
            }
            else if (Joystick_Manager.Check_ButtonPressed(Joystick_Manager.MenuButton_Main.Submit, pad, false))
            {
                var origin = picker.origins[picker.index];
                picker = null;
                waiting.Remove(n);
                lastPick[n] = origin.name;
                SavePicks();
                Game.Log("arena guest: player " + (n + 1) + " picked " + origin.name);
                Spawn(n, origin);
            }
        }

        void SavePicks()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PicksFile));
                File.WriteAllLines(PicksFile, lastPick.Select(kv => kv.Key + "=" + kv.Value).ToArray());
            }
            catch (Exception e) { Game.Error("arena guests: save picks", e); }
        }

        // Guests whose player left get the game's bot (PlayerSquad_AttemptRemovePlayer) and go with a
        // poof, as story bonus characters do. A dead guest is back beside player one once the wave is
        // over, if its player is still in.
        void KeepGuests()
        {
            foreach (var n in guests.Keys.ToList())
            {
                var g = guests[n];
                bool active = Joystick_Manager.ReturnPlayerActive(n);
                if (g.body == null) { guests.Remove(n); continue; }
                if (Game.Alive(g.body))
                {
                    if (!active || !Game.IsPlayer(g.body)) { Remove(n, g, "its player left"); }
                    continue;
                }
                if (!active) { Remove(n, g, "its player left"); continue; }
                if (!Arena_Manager.WavesActive()) Spawn(n, g.origin);
            }
        }

        void Remove(int n, Guest g, string why)
        {
            guests.Remove(n);
            try
            {
                if (Squad_Main.playerSquad != null && g.body.myData != null) Squad_Main.playerSquad.RemoveMember(g.body.myData);
                if (Game.Alive(g.body)) { g.body.TeleportPoof(); g.body.DeSpawn(); }
                Game.Log("arena guest: player " + (n + 1) + "'s " + g.origin.name + " removed (" + why + ")");
            }
            catch (Exception e) { Game.Error("arena guest: remove", e); }
        }

        // The story's bonus-character steps (Joystick_Manager.PlayerSquad_AddWaitingPlayers), with an
        // origin applied as the creator does (Char_Data.ApplyOriginAndResetFromStats). The body starts
        // with a bot, then takes the pad's controller the way the game's swap does (AssignController),
        // so a dead guest's body is left with the bot.
        void Spawn(int n, StatCard_Origin origin)
        {
            var me = Game.Me();
            var career = Game_Manager.currentCareer;
            var squad = Squad_Main.playerSquad;
            if (me == null || career == null || squad == null) return;
            Guest old;
            guests.TryGetValue(n, out old);
            try
            {
                var data = Char_Data.CreateCharacter(Catalog_Characters.ReturnCardForType("Arena"), null);
                data.ApplyOriginAndResetFromStats(origin, career.Imprints);
                data.myName = data.myNameShort = "P" + (n + 1) + " " + origin.ShortName;
                data.CheckForID();
                int level = me.myData != null ? me.myData.myLevel : 1;
                if (level > 1)
                    try { Career_Data.CheatCharacterLevel(data, level); }
                    catch (Exception e) { Game.Error("arena guest: level " + level, e); }
                if (old != null && old.body != null && old.body.myData != null) squad.RemoveMember(old.body.myData);
                squad.AddMember(data);
                var pos = Spawn_Manager.ReturnSpawnLocation(me.Position);
                var body = Combatant_Base.CreateCombatant(data, me.myRoom, Controller_Base.ControllerType.Bot, pos, me.transform.rotation, false);
                var pc = Controller_Player.PlayerControllers[n];
                body.AssignController(pc, true);
                pc.SetKeys(Joystick_Manager.ReturnJoystickOfPlayer(n));
                string weapon = Arm(body, origin);
                guests[n] = new Guest { origin = origin, body = body };
                Game.Say("player " + (n + 1) + " joins as " + origin.ShortName + (old != null ? " again" : ""));
                Game.Log("arena guest: player " + (n + 1) + " plays " + body.name + " (" + origin.name + ", level " + data.myLevel + ", " + weapon + ")");
            }
            catch (Exception e) { Game.Error("arena guest: spawn " + origin.name, e); }
        }

        // The origin's first warchest weapon (the creator puts these in a new career's warchest).
        static string Arm(Combatant_Base body, StatCard_Origin origin)
        {
            var card = origin.WarchestStarterWeapons != null ? origin.WarchestStarterWeapons.FirstOrDefault(w => w != null) : null;
            if (card == null) return "no starter weapon";
            try
            {
                var item = Catalog_Weapons.GenerateWeapon(card);
                return item != null && Char_DataAssigner.GiveItemToCharacter(item, body, 0, 0) != null ? card.name : card.name + " (failed)";
            }
            catch (Exception e) { Game.Error("arena guest: weapon " + card.name, e); return card.name + " (failed)"; }
        }

        void OnGUI()
        {
            if (picker == null) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box) { fontSize = Mathf.Max(14, Screen.height / 55), wordWrap = true, richText = true, alignment = TextAnchor.UpperLeft };
                style.normal.textColor = Color.white;
            }
            var o = picker.origins[picker.index];
            string special = o.SpecialSkills != null && o.SpecialSkills.Length > 0
                ? string.Join("\n", o.SpecialSkills.Where(s => s != null).Select(s => "<color=yellow>[SPECIAL]</color> " + s.Description).ToArray())
                : "<color=grey>No special traits</color>";
            string buffs = o.ReturnBuffsDescription();
            string text = "<b>Player " + (picker.player + 1) + ": pick an origin</b>   (" + (picker.index + 1) + " of " + picker.origins.Count + ")\n"
                + "<size=" + (style.fontSize + 6) + "><b>< " + o.FullName + " ></b></size>\n"
                + o.Description + "\n\n" + special + "\n"
                + (buffs != "" ? buffs + "\n" : "")
                + "Focus " + o.SkillFocus + ", weakness " + o.SkillWeakness + "\n\n"
                + "<color=#AACCFF>D-pad: browse   A: pick   B: cancel</color>";
            float w = Mathf.Min(Screen.width * 0.5f, 760f);
            float h = style.CalcHeight(new GUIContent(text), w);
            GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height * 0.62f - h / 2f, w, h), text, style);
        }
    }
}
