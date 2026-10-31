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
        var shared = new Squad_Main();
        if (sameSquad) foreach (var c in bodies) c.myData.mySquad = shared;
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

    // Boss bar (0.12.0): the game's own UI_Game.AddBoss. Linking a bar ends the character's other
    // health UI (CharUI.LinkToCharacter), so a player's or squadmate's HUD must survive it.
    static void BossBar()
    {
        var bars = new BossBarFeature();
        void Toggle(Combatant_Base c) { Game.Selected = c; Feature.Press(KeyCode.RightBracket); Tick(bars); }

        var player = new Controller_Player();
        Controller_Player.PlayerControllers[0] = player;
        var home = new Combatant_Base("home", player);
        var hud = new CharUI();
        hud.LinkToCharacter(home);
        var npc = new Combatant_Base("npc", new Controller_NPC());
        Feature.Bodies = new[] { home, npc };

        Toggle(npc);
        Check(BossBarFeature.Has(npc) && UI_Game.HasBoss(npc), "boss bar: the key gave the NPC no boss bar");
        Check(UI_Game.currentUI_Game.BossfightIcons.Single(b => b.linkedTarget == npc).shown, "boss bar: the new bar was not shown");
        Toggle(npc);
        Check(!BossBarFeature.Has(npc) && !UI_Game.HasBoss(npc), "boss bar: the key again did not remove it");

        Toggle(home);
        Check(UI_Game.HasBoss(home), "boss bar: a player's own body cannot get a boss bar");
        Check(!hud.ended && home.myHealthUI == hud, "boss bar: adding it ended the player's HUD health bar");
        Toggle(home);
        Check(!UI_Game.HasBoss(home), "boss bar: the player's boss bar stayed after the second press");
        Check(!hud.ended && home.myHealthUI == hud, "boss bar: removing it ended the player's HUD health bar");

        // Death: the bar stays two seconds, then goes.
        Toggle(npc);
        npc.dead = true;
        Tick(bars);
        Check(UI_Game.HasBoss(npc), "boss bar: removed the moment the character died");
        Time.unscaledTime += 2.5f;
        Tick(bars);
        Check(!BossBarFeature.Has(npc) && !UI_Game.HasBoss(npc), "boss bar: a dead character kept its bar");
        npc.dead = false;

        // A scene load clears them all.
        Toggle(npc);
        Toggle(home);
        BossBarFeature.ClearAll();
        Check(!UI_Game.HasBoss(npc) && !UI_Game.HasBoss(home) && !hud.ended, "boss bar: clearing all left a bar or ended the HUD");

        Game.Selected = null;
        Feature.Press(KeyCode.RightBracket);
        Tick(bars);
        Controller_Player.PlayerControllers[0] = null;
    }

    // Wave builder (0.12.0): the roster, its saved form, and what each spawned unit gets.
    static void Wave()
    {
        var w = new WaveRoster();
        var grunt = w.Add("Grunt_A", "Grunt");
        Check(grunt.Weapon == null && !grunt.Infinite && !grunt.Boss, "wave: a new unit is not on its own weapons, normal ammo, no bar");
        grunt.Weapon = "Pistol_A"; grunt.WeaponName = "Automag"; grunt.Infinite = true;
        var copy = w.Copy(0);
        Check(w.Units.Count == 2 && copy.Card == "Grunt_A" && copy.Weapon == "Pistol_A" && copy.Infinite && copy != grunt, "wave: copy did not duplicate the unit");
        copy.Boss = true;
        Check(!grunt.Boss, "wave: the copy shares its settings with the original");
        var agent = w.Add("Agent\tX", "Agent\twith tab");
        Check(w.Copy(7) == null && w.Units.Count == 3, "wave: copying a row that is not there added one");
        Check(WaveRoster.Describe(grunt) == "Grunt: Automag, infinite ammo", "wave: describe says " + WaveRoster.Describe(grunt));
        Check(WaveRoster.Describe(copy) == "Grunt: Automag, infinite ammo, boss bar", "wave: describe says " + WaveRoster.Describe(copy));
        Check(WaveRoster.Describe(agent).EndsWith(": own weapons"), "wave: describe says " + WaveRoster.Describe(agent));

        var back = WaveRoster.Load(WaveRoster.Save(w.Units));
        Check(back.Count == 3, "wave: save and load kept " + back.Count + " of 3 units");
        Check(back[1].Card == "Grunt_A" && back[1].Weapon == "Pistol_A" && back[1].WeaponName == "Automag" && back[1].Infinite && back[1].Boss, "wave: save and load changed a unit");
        Check(back[2].Weapon == null && !back[2].Infinite && back[2].Card == "Agent X", "wave: a unit on its own weapons came back as " + back[2].Weapon + " / " + back[2].Card);
        Check(WaveRoster.Load("# a comment\n\nbroken line\nGrunt_B\t\t1\t0\tGrunt B\t\n").Count == 1, "wave: a damaged file was not read past its bad lines");
        w.Remove(0);
        w.Remove(9);
        Check(w.Units.Count == 2 && w.Units[0] == copy, "wave: remove took the wrong row");
        Check(WaveRoster.FileName(" my wave: 1/2 ") == "my wave_ 1_2", "wave: file name " + WaveRoster.FileName(" my wave: 1/2 "));
        Check(WaveRoster.FileName("  ") == "wave", "wave: an empty name gave " + WaveRoster.FileName("  "));

        // Each spawned unit gets its ammo and bar through the features, so the keys can take them off.
        var npc = new Combatant_Base("wave grunt", new Controller_NPC());
        var plain = new Combatant_Base("plain grunt", new Controller_NPC());
        WaveRoster.Apply(npc, copy);
        WaveRoster.Apply(plain, agent);
        Check(AmmoFeature.Has(npc) && npc.infAmmo && BossBarFeature.Has(npc) && UI_Game.HasBoss(npc), "wave: the unit did not get infinite ammo and a boss bar");
        Check(!AmmoFeature.Has(plain) && !plain.infAmmo && !BossBarFeature.Has(plain), "wave: a plain unit got infinite ammo or a bar");
        AmmoFeature.ClearAll(false);
        BossBarFeature.ClearAll();
        WaveSecondSlots();
    }

    // Wave builder (0.13.0): a second weapon and a throwable per unit. The second weapon goes in the off
    // hand when the game's Char_Data.ReturnCanDual allows the pair (else Combatant_Character drops it),
    // otherwise into the stowed slot as a backup; a grenade-slot pick is a throwable (Wave_Generator.GiveItem).
    static void WaveSecondSlots()
    {
        var w = new WaveRoster();
        var u = w.Add("Grunt_A", "Grunt");
        Check(u.Weapon2 == null && u.Throwable == null, "wave2: a new unit has a second weapon or a throwable");
        u.Weapon = "Pistol_A"; u.WeaponName = "Automag";
        u.Weapon2 = "Pistol_B"; u.Weapon2Name = "Beretta";
        u.Throwable = "Grenade_Frag"; u.ThrowableName = "Frag"; u.Infinite = true; u.Dual = true;
        Check(WaveRoster.Describe(u) == "Grunt: Automag and Beretta, throws Frag, infinite ammo", "wave2: describe says " + WaveRoster.Describe(u));
        var c = w.Copy(0);
        Check(c.Weapon2 == "Pistol_B" && c.Throwable == "Grenade_Frag", "wave2: copy lost the second weapon or throwable");

        var text = WaveRoster.Save(w.Units);
        Check(text.StartsWith("# Director wave v3"), "wave2: the file version was not bumped: " + text.Split('\n')[0]);
        var back = WaveRoster.Load(text);
        Check(back.Count == 2 && back[0].Weapon2 == "Pistol_B" && back[0].Weapon2Name == "Beretta" && back[0].Throwable == "Grenade_Frag" && back[0].ThrowableName == "Frag" && back[0].Infinite,
            "wave2: save and load lost the second weapon or throwable");
        // A 0.12.0 file (six fields, old header) still loads, with no second weapon and no throwable.
        var old = WaveRoster.Load("# Director wave: card, weapon, infinite ammo, boss bar, name, weapon name\nGrunt_A\tPistol_A\t1\t0\tGrunt\tAutomag\n");
        Check(old.Count == 1 && old[0].Weapon == "Pistol_A" && old[0].Infinite && old[0].Weapon2 == null && old[0].Throwable == null, "wave2: a 0.12.0 file did not load as none");
        var blank = WaveRoster.Load(WaveRoster.Save(new[] { new WaveUnit { Card = "Grunt_A" } }));
        Check(blank[0].Weapon2 == null && blank[0].Throwable == null, "wave2: empty slots came back as names");

        Check(WaveRoster.PlaceSecond(false, false, true) == SecondPlace.OffHand, "wave2: a pair the game allows was not dual wielded");
        Check(WaveRoster.PlaceSecond(false, false, false) == SecondPlace.Backup, "wave2: a pair the game refuses was not made a backup");
        Check(WaveRoster.PlaceSecond(true, false, true) == SecondPlace.Throwable, "wave2: a grenade picked as second weapon was not made the throwable");
        Check(WaveRoster.PlaceSecond(true, true, true) == SecondPlace.None, "wave2: a second grenade replaced the picked throwable");
        WaveDual();
        WaveLimit();
    }

    // Wave builder (0.14.0): "Dual wielding: off/on" per enemy. Off means one weapon: the off-hand pick
    // is kept in the row (and the file) but not given. A 0.13.0 (v2) unit with a 2nd weapon reads as on.
    static void WaveDual()
    {
        var u = new WaveUnit { Card = "Grunt_A", Name = "Grunt", Weapon = "Pistol_A", WeaponName = "Automag", Weapon2 = "Pistol_B", Weapon2Name = "Beretta" };
        Check(!u.Dual, "dual: a new unit dual wields");
        Check(WaveRoster.SecondWeapon(u) == null, "dual: switched off, the unit still gets its off-hand weapon");
        Check(WaveRoster.Describe(u) == "Grunt: Automag", "dual: describe with dual off says " + WaveRoster.Describe(u));
        u.Dual = true;
        Check(WaveRoster.SecondWeapon(u) == "Pistol_B", "dual: switched on, the off-hand weapon is not given");
        Check(WaveRoster.DualNote(u, true) == null, "dual: an allowed pair got a note");
        Check(WaveRoster.DualNote(u, null) == null, "dual: an unknown pair got a note");
        Check((WaveRoster.DualNote(u, false) ?? "").Contains("backup"), "dual: a refused pair does not say it is carried as a backup");
        var none = new WaveUnit { Card = "Grunt_A", Dual = true };
        Check((WaveRoster.DualNote(none, null) ?? "").Contains("pick"), "dual: on with no off-hand weapon does not ask for one");

        var off = u.Clone(); off.Dual = false;
        var back = WaveRoster.Load(WaveRoster.Save(new[] { u, off }));
        Check(back[0].Dual && back[0].Weapon2 == "Pistol_B", "dual: save and load lost dual wielding on");
        Check(!back[1].Dual && back[1].Weapon2 == "Pistol_B", "dual: save and load turned dual wielding on or lost the off-hand pick");
        var v2 = WaveRoster.Load("# Director wave v2: card, weapon, infinite ammo, boss bar, name, weapon name, second weapon, its name, throwable, its name\n" +
            "Grunt_A\tPistol_A\t0\t0\tGrunt\tAutomag\tPistol_B\tBeretta\t\t\nGrunt_B\tPistol_A\t0\t0\tGrunt B\tAutomag\t\t\tGrenade_Frag\tFrag\n");
        Check(v2.Count == 2 && v2[0].Dual && v2[0].Weapon2 == "Pistol_B", "dual: a v2 enemy with a 2nd weapon did not read as dual wielding on");
        Check(!v2[1].Dual && v2[1].Throwable == "Grenade_Frag", "dual: a v2 enemy with only a throwable read as dual wielding");
        Check(WaveRoster.MaxAliveIn(WaveRoster.Save(new[] { u })) == 0, "dual: a wave with no limit saved one");
    }

    // Wave builder (0.14.0): "Max enemies" is a cap on how many of a wave's enemies are alive at once,
    // as the game's own waves cap units alive (WaveContainer.UnitCount_Max, Wave_Tandem.CanSpawnWithoutBreachingMax):
    // when one dies the next queued one spawns, until the list is used up. 0 means no limit.
    static void WaveLimit()
    {
        var w = new WaveRoster();
        for (int i = 0; i < 5; i++) w.Add("Grunt_" + i, "Grunt " + i);
        Check(w.MaxAlive == 0, "limit: a new wave has a limit");
        var all = new WaveQueue(w.Units, 0);
        Check(all.Due().Count == 5, "limit: no limit did not spawn the whole wave at once");

        w.MaxAlive = 2;
        var q = new WaveQueue(w.Units, w.MaxAlive);
        var live = new List<Combatant_Base>();
        int most = 0;
        void Fill()
        {
            foreach (var u in q.Due()) { var c = new Combatant_Base(u.Card, new Controller_NPC()); live.Add(c); q.Spawned(c); }
            most = Math.Max(most, live.Count(Game.Alive));
        }
        Fill();
        Check(live.Count == 2 && q.Left == 3, "limit: the first spawn made " + live.Count + " with " + q.Left + " queued");
        Fill();
        Check(live.Count == 2, "limit: a second tick spawned past the limit with nobody dead");
        live[0].dead = true;
        Fill();
        Check(live.Count == 3 && q.Left == 2, "limit: a kill did not bring in the next enemy");
        Check(live[2].name == "Grunt_2", "limit: the queue came in out of order: " + live[2].name);
        while (!q.Done) { live.First(c => !c.dead).dead = true; Fill(); }
        Check(live.Count == 5 && most == 2, "limit: spawned " + live.Count + " of 5, with up to " + most + " alive");
        Check(q.Total == 5 && q.Due().Count == 0, "limit: a used-up wave still spawns");

        // A failed spawn does not hold a place.
        var f = new WaveQueue(w.Units, 2);
        var first = f.Due();
        f.Spawned(null); f.Spawned(new Combatant_Base("ok", new Controller_NPC()));
        Check(first.Count == 2 && f.Due().Count == 1, "limit: a failed spawn held a place");

        // The limit is saved in a comment line, so 0.13.0 still reads the enemies; v2 files read as no limit.
        var text = WaveRoster.Save(w.Units, 2);
        Check(WaveRoster.MaxAliveIn(text) == 2 && WaveRoster.Load(text).Count == 5, "limit: save and load lost the limit or an enemy");
        Check(WaveRoster.MaxAliveIn("# Director wave v2: card\nGrunt_A\t\t0\t0\n") == 0, "limit: a v2 file read as having a limit");
        Check(WaveRoster.MaxAliveIn("# max alive: lots\n") == 0, "limit: a damaged limit line read as a limit");
        Check(WaveRoster.Load(text).All(u => u.Card.StartsWith("Grunt_")), "limit: the limit line was read as an enemy");
    }

    // Team change (0.12.0): [ turns an enemy into an ally of the keyboard player and back, through the
    // game's own Squad_Main.AddMember; hostility is Combat_Manager.FactionIsAttackable.
    static void Team()
    {
        var team = new TeamFeature();
        void Toggle(Combatant_Base c) { Game.Selected = c; Feature.Press(KeyCode.LeftBracket); Tick(team); }
        TeamFeature.Reset();

        var player = new Controller_Player();
        Controller_Player.PlayerControllers[0] = player;
        var home = new Combatant_Base("home", player);
        var mine = Squad_Main.CreateSquad(Factions.Player);
        mine.AddMember(home.myData);
        var spawned = Squad_Main.CreateSquad(Factions.None);
        var grunt = new Combatant_Base("grunt", new Controller_NPC());
        spawned.AddMember(grunt.myData);
        var mate = new Combatant_Base("mate", new Controller_NPC());
        mine.AddMember(mate.myData);
        var sanford = new Combatant_Base("career", new Controller_NPC()) { careerCharacter = true };
        mine.AddMember(sanford.myData);
        Feature.Bodies = new[] { home, grunt, mate, sanford };

        Check(Combat_Manager.FactionIsAttackable(grunt, home) && !Combat_Manager.FactionIsAttackable(mate, home), "team: the stub's hostility is wrong");
        Toggle(grunt);
        Check(!Combat_Manager.FactionIsAttackable(grunt, home) && !Combat_Manager.FactionIsAttackable(home, grunt), "team: the enemy did not become an ally");
        Check(grunt.myFaction == Factions.Player && grunt.myData.mySquad != spawned, "team: the ally kept its old squad or faction");
        Toggle(grunt);
        Check(grunt.myData.mySquad == spawned && Combat_Manager.FactionIsAttackable(grunt, home), "team: [ again did not send it back to its own squad");
        Check(grunt.myController.myAlertStatus == AI_Wakeup.AlertStatus.Combat, "team: the new enemy is not fighting");

        Toggle(mate);
        Check(Combat_Manager.FactionIsAttackable(mate, home) && mate.myData.mySquad != mine, "team: the squadmate did not become an enemy");
        Toggle(mate);
        Check(!Combat_Manager.FactionIsAttackable(mate, home), "team: [ again did not make the squadmate an ally");

        // Two enemies turned share one squad, so they do not fight each other.
        var other = new Combatant_Base("other", new Controller_NPC());
        mine.AddMember(other.myData);
        Toggle(other);
        Toggle(mate);
        Check(other.myData.mySquad == mate.myData.mySquad && !Combat_Manager.FactionIsAttackable(other, mate), "team: two turned enemies fight each other");

        Toggle(home);
        Check(home.myData.mySquad == mine && home.myFaction == Factions.Player, "team: [ changed a player's team");
        Toggle(sanford);
        Check(sanford.myData.mySquad == mine, "team: [ changed a career character's team");

        // A player in a None squad (a Playground character): allies join the player's own squad.
        TeamFeature.Reset();
        var loner = Squad_Main.CreateSquad(Factions.None);
        loner.AddMember(home.myData);
        Toggle(grunt);
        Check(grunt.myData.mySquad == loner && !Combat_Manager.FactionIsAttackable(grunt, home), "team: with a None-faction player the ally is still hostile");

        Game.Selected = null;
        Feature.Press(KeyCode.LeftBracket);
        Tick(team);
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
        Ammo();
        Console.WriteLine("Director infinite ammo checks passed");
        Catalog();
        Console.WriteLine("Director weapon list checks passed");
        BossBar();
        Console.WriteLine("Director boss bar checks passed");
        Wave();
        Console.WriteLine("Director wave builder checks passed");
        Team();
        Console.WriteLine("Director team change checks passed");
        Filter();
        Console.WriteLine("Director list filter and weapon search checks passed");
    }

    // The filter box and weapon search (0.15.0): only lists over 20 get the box; caseless, plain
    // text means contains, * and ? are wildcards that also match anywhere.
    static void Filter()
    {
        Check(!ListFilter.Shows(20) && ListFilter.Shows(21) && !ListFilter.Shows(0), "filter: the box shows at the wrong length");
        Check(ListFilter.Matches("", "AK-47") && ListFilter.Matches("   ", "AK-47") && ListFilter.Matches(null, "AK-47"), "filter: an empty box hides things");
        Check(ListFilter.Matches("ak", "AK-47") && ListFilter.Matches("47", "AK-47"), "filter: contains is not caseless");
        Check(!ListFilter.Matches("m16", "AK-47"), "filter: a miss matched");
        Check(ListFilter.Matches("rifle", "AK-47", "Rifle_AK47"), "filter: the card name is not searched");
        Check(ListFilter.Matches("a*7", "Big AK-47 gun") && ListFilter.Matches("A*47", "ak-47"), "filter: * does not span");
        Check(!ListFilter.Matches("a*z", "AK-47"), "filter: * matched a missing letter");
        Check(ListFilter.Matches("ak-?7", "AK-47") && !ListFilter.Matches("ak-?7", "AK-7"), "filter: ? is not exactly one");
        Check(ListFilter.Matches("*", "x") && ListFilter.Matches("**", ""), "filter: a lone * misses");
        Check(ListFilter.Matches(" ak ", "AK-47"), "filter: spaces around the text are not trimmed");
        Check(ListFilter.Matches("[ak]", "Gun [AK] x") && ListFilter.Matches("a.k", "a.k") && !ListFilter.Matches("a.k", "abk"), "filter: other characters are not literal");
        var left = ListFilter.Apply(new[] { "Axe", "AK-47", "Bat" }, "a", s => new[] { s });
        Check(left.SequenceEqual(new[] { "Axe", "AK-47", "Bat" }), "filter: Apply lost an item or the order: " + string.Join(",", left));
        Check(ListFilter.Apply(new[] { "Axe", "Bat" }, "x", s => new[] { s }).SequenceEqual(new[] { "Axe" }), "filter: Apply kept a miss");

        RecentPicks.Clear();
        foreach (var c in new[] { "a", "b", "a", "c", "d", "e", "f" }) RecentPicks.Add(c);
        Check(RecentPicks.Cards.SequenceEqual(new[] { "f", "e", "d", "c", "a" }), "recent: kept " + string.Join(",", RecentPicks.Cards));
        RecentPicks.Add(null);
        Check(RecentPicks.Cards.Count == RecentPicks.Max, "recent: a null pick was kept");
        RecentPicks.Clear();
    }
}

public enum TraitList { Zed, InfiniteAmmo }

namespace UnityEngine
{
    public class MonoBehaviour
    {
        protected static T[] FindObjectsOfType<T>() => Feature.Bodies.Cast<T>().ToArray();
    }
    public enum KeyCode { F1, F2, F5, F6, F7, F8, F9, F10, Backslash, RightBracket, LeftBracket }
    public struct Color
    {
        public static Color yellow, red, white, cyan, green;
    }
    public static class Time { public static float unscaledTime; }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { }
    public enum LoadSceneMode { Single }
    public static class SceneManager { public static event System.Action<Scene, LoadSceneMode> sceneLoaded; }
}

public enum Factions { Player = 0, TwoShoes = 1, None = 8, Neutral = 10, Enemy = 11 }

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
        public static bool CareerCharacter(Combatant_Base c) => c.careerCharacter;
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
    public class UI_Game
    {
        public static UI_Game currentUI_Game = new();
        public List<CharUI> BossfightIcons = new();
        static List<CharUI> bosses => currentUI_Game.BossfightIcons;
        public static void UnLinkCharacter(Combatant_Base c) { }
        public static void ApplySquadmates() { }
        public static bool HasBoss(Combatant_Base c) => bosses.Exists(b => b.linkedTarget == c);
        // As the game's AddBossLocal: one bar per target, made with CharUI.CreateIcon (Init links it).
        public static void AddBoss(Combatant_Base c)
        {
            if (HasBoss(c)) return;
            var bar = new CharUI();
            bar.LinkToCharacter(c);
            bosses.Add(bar);
        }
        public static void RemoveBoss(Combatant_Base c)
        {
            var bar = bosses.Find(b => b.linkedTarget == c);
            if (bar == null) return;
            bosses.Remove(bar);
            bar.EndMe();
        }
    }
    // As the game's CharUI: linking ends the target's other health UI; ending clears the field only if it is this.
    public class CharUI
    {
        public Combatant_Base linkedTarget;
        public bool ended, shown;
        public void ShowMe(bool resetCorpus) => shown = true;
        public void LinkToCharacter(Combatant_Base c)
        {
            linkedTarget = c;
            if (c.myHealthUI != null && c.myHealthUI != this) c.myHealthUI.EndMe();
            c.myHealthUI = this;
        }
        public void EndMe()
        {
            ended = true;
            if (linkedTarget != null && linkedTarget.myHealthUI == this) linkedTarget.myHealthUI = null;
        }
    }
    public static class Cam_Main
    {
        public static void CancelZoom() { }
    }
    public class CharData { public Squad_Main mySquad; public Combatant_Base owner; }
    // As the game's: AddMember leaves the old squad and takes this squad's faction.
    public class Squad_Main
    {
        public Factions squadFaction;
        public static Squad_Main CreateSquad(Factions f) => new Squad_Main { squadFaction = f };
        public void AddMember(CharData d)
        {
            d.mySquad = this;
            if (d.owner != null) d.owner.myFaction = squadFaction;
        }
    }
    // As the game's Combat_Manager.FactionIsAttackable.
    public static class Combat_Manager
    {
        public static bool FactionIsAttackable(Factions a, Factions d) => ((a != d && a != Factions.Neutral) || a == Factions.None) && d != Factions.Neutral;
        public static bool FactionIsAttackable(Combatant_Base a, Combatant_Base d) => FactionIsAttackable(a.myFaction, d.myFaction) && a.myData.mySquad != d.myData.mySquad;
    }
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
        public CharUI myHealthUI;
        public Factions myFaction = Factions.None;
        public bool careerCharacter;
        public readonly HashSet<TraitList> traits = new();
        public bool HaveTrait(TraitList t) => traits.Contains(t);
        public Combatant_Base(string name, Controller_Base controller)
        {
            this.name = name;
            myData.owner = this;
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
