using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SwapUnaware
{
    // [ (left bracket) on the character under the mouse: enemy becomes ally, ally becomes enemy, as seen
    // by the keyboard player (the game's Combat_Manager.FactionIsAttackable). An ally joins a squad of your
    // faction (or your own squad when your faction is None) and lets go of its target; an enemy goes back
    // to the squad it came from, or else joins one Director enemy squad (Factions.None, like the
    // Playground's spawns), and turns to fight. Players and the career's own characters are left alone.
    // Team changes go through the game's own Squad_Main.AddMember, which moves faction and wave too.
    public class TeamFeature : Feature
    {
        public KeyCode Key = KeyCode.LeftBracket;

        static readonly Dictionary<Combatant_Base, Squad_Main> cameFrom = new Dictionary<Combatant_Base, Squad_Main>();
        static Squad_Main allies, enemies;

        protected override string Name { get { return "team change ([)"; } }
        protected override void Register() { Menu("Change team: enemy / ally", Key, true); }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }

        void OnSceneLoaded(Scene s, LoadSceneMode m) { Reset(); }

        internal static void Reset()
        {
            cameFrom.Clear();
            allies = null;
            enemies = null;
        }

        protected override void Tick()
        {
            if (!Pressed(Key)) return;
            var c = Game.CharacterUnderMouse();
            if (c == null) { Game.Say("no character under the mouse"); return; }
            Game.Say(Toggle(c));
        }

        // What happened, for the HUD line.
        internal static string Toggle(Combatant_Base c)
        {
            var me = Game.Me();
            if (me == null || c.myData == null) return "no team to change";
            if (c == me || Game.IsPlayer(c)) return c.name + " is a player; [ changes only computer characters";
            if (Game.CareerCharacter(c)) return c.name + " belongs to your career; its team stays";
            Game.ReleaseClaims(c.myController);
            if (Combat_Manager.FactionIsAttackable(c, me))
            {
                if (!cameFrom.ContainsKey(c)) cameFrom[c] = c.myData.mySquad;
                AllySquad(me).AddMember(c.myData);
                c.FlashColor(Color.cyan, 8f, 1f);
                return c.name + ": now on YOUR side ([ again to turn it)";
            }
            Squad_Main back;
            if (!cameFrom.TryGetValue(c, out back) || back == null || back == c.myData.mySquad || !Combat_Manager.FactionIsAttackable(back.squadFaction, me.myFaction))
                back = EnemySquad();
            cameFrom.Remove(c);
            back.AddMember(c.myData);
            Controller_Base.SetAlertStatus(c.myController, AI_Wakeup.AlertStatus.Combat);
            c.FlashColor(Color.red, 8f, 1f);
            return c.name + ": now your ENEMY ([ again to turn it back)";
        }

        static Squad_Main AllySquad(Combatant_Base me)
        {
            if (me.myFaction == Factions.None && me.myData.mySquad != null) return me.myData.mySquad;
            if (allies == null || allies.squadFaction != me.myFaction) allies = Squad_Main.CreateSquad(me.myFaction);
            return allies;
        }

        static Squad_Main EnemySquad()
        {
            if (enemies == null) enemies = Squad_Main.CreateSquad(Factions.None);
            return enemies;
        }

        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    }
}
