using System.Reflection;
using SwapUnaware;
using UnityEngine;

static class Program
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Tick(object feature)
    {
        var method = feature.GetType().GetMethod("Tick", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(feature, null);
    }

    static FreezeFeature freeze;
    static AlertFeature alert;

    static void F2() { Feature.Press(KeyCode.F2); Tick(freeze); }

    // Every check that must hold while a player drives a body: never locked by Director, never pinned.
    static void PlayerFree(Controller_Player player, string when)
    {
        var me = player.myCombatant;
        Check(!player.AmLocked(), when + ": the player's controller is locked");
        Check(!me.AmFrozen() && me.myAnimator.myAnim.speed == 1, when + ": the player's body is frozen");
    }

    static void Held(Combatant_Base c, string when)
    {
        Check(FreezeFeature.IsFrozen(c) && c.AmFrozen() && c.myAnimator.myAnim.speed == 0, when + ": " + c.name + " is not frozen");
        Check(c.myController.AmLocked(), when + ": " + c.name + "'s controller is not locked");
    }

    static void AllReleased(Combatant_Base[] bodies, string when)
    {
        foreach (var c in bodies)
        {
            Check(!FreezeFeature.IsFrozen(c) && !c.AmFrozen() && c.myAnimator.myAnim.speed == 1, when + ": " + c.name + " still frozen");
            Check(!c.myController.AmLocked(), when + ": " + c.name + "'s controller still locked");
        }
    }

    static void Run(bool sameSquad)
    {
        var path = sameSquad ? "same squad: " : "cross squad: ";
        var player = new Controller_Player();
        Controller_Player.PlayerControllers[0] = player;
        var home = new Combatant_Base("home", player);
        var npc = new Combatant_Base("npc", new Controller_NPC());
        var other = new Combatant_Base("other", new Controller_NPC());
        var bodies = new[] { home, npc, other };
        if (sameSquad) foreach (var c in bodies) c.myData.mySquad = bodies;
        Feature.Bodies = bodies;

        // 0.10.1's own sequence: F2, F10 through all three states, F5 onto the frozen NPC, F8 away, F2.
        Game.Selected = npc;
        F2();
        Held(npc, path + "F2");
        foreach (var status in new[] { AI_Wakeup.AlertStatus.Hunting, AI_Wakeup.AlertStatus.Combat, AI_Wakeup.AlertStatus.Unaware })
        {
            Feature.Press(KeyCode.F10);
            Tick(alert);
            Check(npc.myController.myAlertStatus == status, path + "F10 changed its alert cycle");
            Held(npc, path + "F10");
        }
        SwapUnawareBehaviour.SwapTo(player, npc);
        Tick(freeze);
        Check(player.myCombatant == npc, path + "F5 did not take over");
        PlayerFree(player, path + "F5 onto a frozen body");
        Check(!home.myController.AmLocked(), path + "F5 left a lock on the old body");
        SwapUnawareBehaviour.SwapTo(player, home);
        Tick(freeze);
        PlayerFree(player, path + "back home");
        Held(npc, path + "leaving a body frozen when taken");
        F2();
        AllReleased(bodies, path + "F2 release");

        // The requester's order: F5 onto someone, F2 (freezes the home body), walk, F8 home, F2.
        SwapUnawareBehaviour.SwapTo(player, npc);
        F2();
        PlayerFree(player, path + "F5 then F2");
        Held(home, path + "F5 then F2");
        Held(other, path + "F5 then F2");
        SwapUnawareBehaviour.SwapTo(player, home);
        Tick(freeze);
        PlayerFree(player, path + "F8 home into a frozen body");
        Check(!FreezeFeature.IsFrozen(npc) && !npc.AmFrozen(), path + "the body you left was frozen though F2 skipped it");
        F2();
        AllReleased(bodies, path + "F5, F2, F8, F2");

        // F5 across two frozen bodies, then F2 while inside one.
        F2();
        SwapUnawareBehaviour.SwapTo(player, npc);
        Tick(freeze);
        SwapUnawareBehaviour.SwapTo(player, other);
        Tick(freeze);
        PlayerFree(player, path + "chained F5");
        Held(npc, path + "chained F5");
        Check(!FreezeFeature.IsFrozen(home) && !home.myController.AmLocked(), path + "chained F5 froze the home body F2 skipped");
        F2();
        AllReleased(bodies, path + "F2 inside a frozen body");

        // The game swaps two frozen NPCs' controllers itself (not through F5): both stay locked.
        SwapUnawareBehaviour.SwapTo(player, home);
        F2();
        var a = npc.myController;
        var b = other.myController;
        npc.AssignController(b, true);
        Tick(freeze);
        Check(npc.myController == b && other.myController == a, path + "stub swap");
        Held(npc, path + "game swap");
        Held(other, path + "game swap");
        F2();
        AllReleased(bodies, path + "after a game swap");

        // Safety net: a lock on a player controller the bookkeeping lost is cleared by F2's release.
        F2();
        player.LockController(freeze);
        F2();
        AllReleased(bodies, path + "stray player lock");
        Controller_Player.PlayerControllers[0] = null;
    }

    static void Main()
    {
        freeze = new FreezeFeature();
        typeof(FreezeFeature).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(freeze, null);
        alert = new AlertFeature();
        Run(sameSquad: false);
        Run(sameSquad: true);
        Console.WriteLine("Director freeze checks passed on both swap paths");
    }
}

namespace UnityEngine
{
    public class MonoBehaviour
    {
        protected static T[] FindObjectsOfType<T>() => Feature.Bodies.Cast<T>().ToArray();
    }
    public enum KeyCode { F1, F2, F5, F6, F7, F8, F9, F10 }
    public struct Color
    {
        public static Color yellow, red, white, cyan, green;
    }
    public static class Time { public static float unscaledTime; }
}

namespace SwapUnaware
{
    public abstract class Feature : MonoBehaviour
    {
        static readonly HashSet<KeyCode> keys = new();
        public static Combatant_Base[] Bodies;
        public static void Press(KeyCode key) => keys.Add(key);
        protected static bool Pressed(KeyCode key) => keys.Remove(key);
        protected static void Menu(string label, KeyCode key, bool pick = false) { }
        protected abstract string Name { get; }
        protected abstract void Tick();
        protected virtual void Register() { }
    }

    public static class Game
    {
        public static Combatant_Base Selected;
        public static Combatant_Base Me() => Controller_Player.PlayerController.myCombatant;
        public static Combatant_Base NpcUnderMouse() => Selected;
        public static bool Alive(Combatant_Base c) => c != null && !c.AmDead();
        public static bool IsNpc(Combatant_Base c) => Alive(c) && c.myController is Controller_NPC;
        public static bool IsPlayer(Combatant_Base c) => c?.myController is Controller_Player;
        public static Controller_Player[] PlayerControllers => Controller_Player.PlayerControllers;
        public static bool Playable(Combatant_Base c) => false;
        public static int PlayerNumber(Combatant_Base c) => 0;
        public static void PruneClaimants() { }
        public static void ReleaseClaims(Controller_Base c) { }
        public static void Say(string msg) { }
        public static void Error(string where, Exception e) => throw new Exception(where, e);
    }

    public class Controller_Base
    {
        readonly HashSet<object> locks = new();
        public Combatant_Base myCombatant;
        public AI_Wakeup.AlertStatus myAlertStatus;
        // The game's LockSources is a HashSet, and a new connection clears it (Controller_Base.NewConnection).
        public void LockController(object source) => locks.Add(source);
        public void ReleaseController(object source) => locks.Remove(source);
        public bool AmLocked() => locks.Count != 0;
        public void SetConnection(Combatant_Base c)
        {
            if (myCombatant == c) return;
            myCombatant = c;
            c.myController = this;
            locks.Clear();
        }
        public static void SetAlertStatus(Controller_Base ctl, AI_Wakeup.AlertStatus status)
        {
            ctl.myAlertStatus = status;
            ctl.myCombatant.SetFrozen(false);
            ctl.myCombatant.myAnimator.myAnim.speed = 1;
        }
    }

    public class Controller_NPC : Controller_Base { }
    public class Controller_Player : Controller_Base
    {
        public static Controller_Player[] PlayerControllers = new Controller_Player[4];
        public static Controller_Player PlayerController => PlayerControllers[0];
        public void SwapControllersWithSquadmate(Combatant_Base target) => target.AssignController(this, true);
    }
    public static class AI_Wakeup
    {
        public enum AlertStatus { Unaware, Hunting, Combat }
    }
    public static class AI_Manager
    {
        public static void SetKillTarget(Controller_Base ctl, Combatant_Base target) { }
    }
    public static class Pathfind_Main
    {
        public static void ClearWaypointsAndTarget(Combatant_Base c) { }
    }
    public static class PatrolFeature
    {
        public static bool HasRoute(Combatant_Base c) => false;
    }
    public static class UI_Game
    {
        public static void UnLinkCharacter(Combatant_Base c) { }
        public static void ApplySquadmates() { }
    }
    public static class Cam_Main
    {
        public static void CancelZoom() { }
    }
    public class CharData { public object mySquad; }
    public class Animation { public float speed = 1; }
    public class Animator { public Animation myAnim = new(); }
    public class Combatant_Base
    {
        bool frozen;
        public string name;
        public Controller_Base myController;
        public Animator myAnimator = new();
        public CharData myData = new();
        public bool wanderingDisabled;
        public bool amPlayer;
        public Combatant_Base(string name, Controller_Base controller)
        {
            this.name = name;
            controller.SetConnection(this);
        }
        public bool AmFrozen() => frozen;
        public bool AmDead() => false;
        public void SetFrozen(bool value) => frozen = value;
        public void FlashColor(Color color, float a, float b) { }
        public void RemoveFromGame() { }
        public void CancelFlash() { }
        // As the game's: the incoming controller's old body takes this body's controller.
        public void AssignController(Controller_Base incoming, bool overrideCurrentController)
        {
            var old = incoming.myCombatant;
            var mine = myController;
            incoming.SetConnection(this);
            if (old != null && old != this) mine.SetConnection(old);
        }
    }
}
