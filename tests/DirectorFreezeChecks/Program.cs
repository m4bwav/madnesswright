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

    // Infinite ammo (0.11.0): the game's own InfiniteAmmo trait flag (Combatant_Base.infAmmo), kept
    // on the body, put back each frame after the game recomputes it, ended by death.
    static void Ammo()
    {
        var ammo = new AmmoFeature();
        void Toggle(Combatant_Base c) { Game.Selected = c; Feature.Press(KeyCode.Backslash); Tick(ammo); }

        var player = new Controller_Player();
        Controller_Player.PlayerControllers[0] = player;
        var home = new Combatant_Base("home", player);
        var npc = new Combatant_Base("npc", new Controller_NPC());
        var born = new Combatant_Base("born with the trait", new Controller_NPC());
        born.traits.Add(TraitList.InfiniteAmmo);
        born.infAmmo = true;
        Feature.Bodies = new[] { home, npc, born };

        Toggle(npc);
        Check(AmmoFeature.Has(npc) && npc.infAmmo, "ammo: the key did not give the NPC infinite ammo");
        Check(!home.infAmmo && !AmmoFeature.Has(home), "ammo: the player got infinite ammo too");
        Toggle(npc);
        Check(!AmmoFeature.Has(npc) && !npc.infAmmo, "ammo: the key again did not take it away");

        Toggle(home);
        Check(AmmoFeature.Has(home) && home.infAmmo, "ammo: a player's own body cannot get infinite ammo");
        Toggle(home);
        Check(!home.infAmmo, "ammo: the player's infinite ammo stayed after the second press");

        Toggle(born);
        Toggle(born);
        Check(born.infAmmo, "ammo: turning it off took away the character's own InfiniteAmmo trait");

        // The game recomputes the flag from the traits (ApplyAllStats on a level-up, a shop purchase,
        // a boss pattern); the feature puts it back.
        Toggle(npc);
        npc.infAmmo = false;
        Tick(ammo);
        Check(npc.infAmmo, "ammo: the flag was not put back after the game reset it");

        // F5 takes over a body: infinite ammo stays with the body, not the player.
        SwapUnawareBehaviour.SwapTo(player, npc);
        Tick(ammo);
        Check(player.myCombatant == npc, "ammo: F5 did not take over");
        Check(AmmoFeature.Has(npc) && npc.infAmmo, "ammo: F5 took infinite ammo off the body");
        Check(!AmmoFeature.Has(home) && !home.infAmmo, "ammo: F5 gave the old body infinite ammo");
        SwapUnawareBehaviour.SwapTo(player, home);
        Tick(ammo);
        Check(AmmoFeature.Has(npc) && npc.infAmmo && !home.infAmmo, "ammo: F8 home moved infinite ammo");

        // Death ends it.
        npc.dead = true;
        Tick(ammo);
        Check(!AmmoFeature.Has(npc) && !npc.infAmmo, "ammo: a dead body kept infinite ammo");
        npc.dead = false;

        // A scene load clears everyone but, with the keep box, the players' bodies.
        Toggle(npc);
        Toggle(home);
        AmmoFeature.ClearAll(sparePlayers: true);
        Check(!AmmoFeature.Has(npc) && !npc.infAmmo, "ammo: a load kept an NPC's infinite ammo");
        Check(AmmoFeature.Has(home) && home.infAmmo, "ammo: the keep box lost the player's infinite ammo");
        AmmoFeature.Set(npc, true);
        Check(AmmoFeature.Has(npc) && npc.infAmmo, "ammo: a scene could not set infinite ammo");
        AmmoFeature.ClearAll(sparePlayers: false);
        Check(!AmmoFeature.Has(home) && !home.infAmmo && !npc.infAmmo && born.infAmmo, "ammo: clearing all left a flag wrong");

        Game.Selected = null;
        Feature.Press(KeyCode.Backslash);
        Tick(ammo);
        Controller_Player.PlayerControllers[0] = null;
    }

    static WeaponEntry W(string card, string name, string category, int order, bool hidden = false)
    {
        return new WeaponEntry { Card = card, Name = name, Category = category, Order = order, Hidden = hidden };
    }

    // The Spawn a weapon list (0.11.0): the game's categories in their own order, "Other" last.
    static void Catalog()
    {
        var groups = WeaponCatalog.Group(new[]
        {
            W("Rifle_AK", "AK", "Rifle", 4),
            W("Pistol_b", "beretta", "Pistol", 0),
            W("Mod_Thing", "Strange thing", null, 100),
            W("Pistol_A", "Automag", "Pistol", 0),
            W("Fists_Brass", "Brass knuckles", "Unarmed", 7, hidden: true),
            W("Pistol_A", "Automag duplicate", "Pistol", 0),
            W("None_Card", "", "", 100),
            W("Pistol_C", "Automag", "Pistol", 0),
            null,
        });
        var names = groups.Select(g => g.Name).ToArray();
        Check(names.SequenceEqual(new[] { "Pistol", "Rifle", "Unarmed", WeaponCatalog.Other }),
            "catalog: groups are " + string.Join(", ", names));
        var pistols = groups[0].Weapons.Select(w => w.Card).ToArray();
        Check(pistols.SequenceEqual(new[] { "Pistol_A", "Pistol_C", "Pistol_b" }),
            "catalog: pistols sorted as " + string.Join(", ", pistols));
        Check(groups[0].Weapons[0].Label == "Automag [Pistol_A]" && groups[0].Weapons[1].Label == "Automag [Pistol_C]",
            "catalog: two weapons with one name are not told apart: " + groups[0].Weapons[0].Label);
        Check(groups[0].Weapons[2].Label == "beretta", "catalog: a unique name got a card suffix");
        var other = groups[3].Weapons.Select(w => w.Label).ToArray();
        Check(other.SequenceEqual(new[] { "None_Card", "Strange thing" }), "catalog: Other holds " + string.Join(", ", other));
        Check(WeaponCatalog.Group(new WeaponEntry[0]).Count == 0, "catalog: an empty list made groups");
    }

    static void Main()
    {
        freeze = new FreezeFeature();
        typeof(FreezeFeature).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(freeze, null);
        alert = new AlertFeature();
        Run(sameSquad: false);
        Run(sameSquad: true);
        Console.WriteLine("Director freeze checks passed on both swap paths");
        Ammo();
        Console.WriteLine("Director infinite ammo checks passed");
        Catalog();
        Console.WriteLine("Director weapon list checks passed");
    }
}

public enum TraitList { Zed, InfiniteAmmo }

namespace UnityEngine
{
    public class MonoBehaviour
    {
        protected static T[] FindObjectsOfType<T>() => Feature.Bodies.Cast<T>().ToArray();
    }
    public enum KeyCode { F1, F2, F5, F6, F7, F8, F9, F10, Backslash }
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
        public static Combatant_Base CharacterUnderMouse() => Selected;
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
        public bool dead;
        // As the game's: Combatant_Character.SetTraitBools sets infAmmo from the InfiniteAmmo trait.
        public bool infAmmo;
        public readonly HashSet<TraitList> traits = new();
        public bool HaveTrait(TraitList t) => traits.Contains(t);
        public Combatant_Base(string name, Controller_Base controller)
        {
            this.name = name;
            controller.SetConnection(this);
        }
        public bool AmFrozen() => frozen;
        public bool AmDead() => dead;
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
