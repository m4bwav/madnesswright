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

    static void Main()
    {
        var player = new Controller_Player();
        Controller_Player.PlayerController = player;
        var home = new Combatant_Base("home", player);
        var npc = new Combatant_Base("npc", new Controller_NPC());
        var other = new Combatant_Base("other", new Controller_NPC());
        var freeze = new FreezeFeature();
        typeof(FreezeFeature).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(freeze, null);
        var alert = new AlertFeature();

        Feature.Bodies = new[] { home, npc, other };
        Game.Selected = npc;
        Feature.Press(KeyCode.F2);
        Tick(freeze);
        Check(FreezeFeature.IsFrozen(npc) && npc.AmFrozen(), "F2 did not freeze the NPC");
        Check(npc.myController.AmLocked() && npc.myAnimator.myAnim.speed == 0, "F2 did not lock movement and animation");

        foreach (var status in new[] { AI_Wakeup.AlertStatus.Hunting, AI_Wakeup.AlertStatus.Combat, AI_Wakeup.AlertStatus.Unaware })
        {
            Feature.Press(KeyCode.F10);
            Tick(alert);
            Check(npc.myController.myAlertStatus == status, "F10 changed its alert cycle");
            Check(FreezeFeature.IsFrozen(npc) && npc.AmFrozen(), "F10 removed the freeze");
            Check(npc.myController.AmLocked() && npc.myAnimator.myAnim.speed == 0, "F10 allowed movement or animation");
        }

        SwapUnawareBehaviour.SwapTo(player, npc);
        Check(player.myCombatant == npc && FreezeFeature.IsFrozen(npc) && npc.AmFrozen(), "F5 lost freeze on takeover");
        Check(player.AmLocked() && !home.myController.AmLocked(), "F5 left the freeze lock on the old body");
        SwapUnawareBehaviour.SwapTo(player, home);
        Check(player.myCombatant == home && FreezeFeature.IsFrozen(npc) && npc.AmFrozen(), "switching away lost freeze");
        Check(npc.myController.AmLocked() && !player.AmLocked(), "switching away left the lock on the player");

        Feature.Press(KeyCode.F2);
        Tick(freeze);
        Check(!FreezeFeature.IsFrozen(npc) && !npc.AmFrozen(), "F2 did not release the frozen NPC");
        Check(!npc.myController.AmLocked() && npc.myAnimator.myAnim.speed == 1, "F2 did not restore movement and animation");
        Console.WriteLine("F10 alert cycle and F5 takeover/switch-away freeze regressions passed");
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
        public void LockController(object source) => locks.Add(source);
        public void ReleaseController(object source) => locks.Remove(source);
        public bool AmLocked() => locks.Count != 0;
        public void SetConnection(Combatant_Base c) { myCombatant = c; c.myController = this; }
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
        public static Controller_Player PlayerController;
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
        public void AssignController(Controller_Player player, bool overrideCurrentController)
        {
            var old = player.myCombatant;
            var npc = myController;
            player.SetConnection(this);
            npc.SetConnection(old);
            SetFrozen(false);
        }
    }
}
