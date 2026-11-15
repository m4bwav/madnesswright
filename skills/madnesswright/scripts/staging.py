"""Enemy staging checks for mpn.py scene-lint (S19, S20). Standard library only."""
# BehaviorList enum values used by staging_check (decompiled BehaviorList.cs, build 25615280)
BEHAVIORS = ["Mindless", "Hunter", "Coward", "Personal", "Turtle", "Evasive", "Enraged", "ItchyFinger", "WhiteNinja",
             "GrenadeLover", "ChargeUpLover", "BadBlock", "FavorDual", "Runner", "Accurate", "SlowWitted", "Tactical",
             "UNUSED_1", "ChargeUpASAP", "Defensive", "ThrowsOften", "Focused", "ADD", "FarSight", "Indirect", "Bubble",
             "HotFoot", "Camper", "Sniper", "NoSprint", "Sharky", "FreeAttacker", "TargetShy", "HoldCover"]
ALERTNESS = {"0": "Unaware", "1": "Hunting", "2": "Combat", "100": "Match"}


def staging_check(docs, instances, warnings, info, mod_refs):
    """S19/S20: how the placed enemies (Char_Start instances) are staged. S19: six or more enemies that all
    face one way, share one alertness and carry no added behaviour play as one fight repeated in every room
    (Nexus Prominence round 6). Vary facing (the marker's rotation is the spawn facing), alertness,
    AddBehaviors (Camper sends a character to the room's Sniper waypoints; HoldCover, Defensive, Hunter, Runner)
    or SpawnFromDoor (walks in through a door). S20: FixedInPlace freezes the character (Char_DataAssigner calls
    SetFrozen), so it never moves or fights; for a sniper that holds a post use AddBehaviors Camper instead."""
    chars = [i for i in instances if i["kind"] == "Char_Start"]
    if not chars:
        return
    alerts, faces, beh, from_door = {}, set(), {}, 0
    for c in chars:
        mods = {p: (v, r) for p, v, r in mod_refs(docs[c["fid"]]["body"])}
        a = ALERTNESS.get(mods.get("Alertness", ("0", "0"))[0], mods.get("Alertness", ("0", "0"))[0])
        alerts[a] = alerts.get(a, 0) + 1
        q = tuple(round(float(mods.get("m_LocalRotation." + k, ("1" if k == "w" else "0", "0"))[0] or 0), 2) for k in "yw")
        faces.add(q)
        for path, (v, _) in mods.items():
            if path.startswith("AddBehaviors.Array.data["):
                name = BEHAVIORS[int(v)] if v.isdigit() and int(v) < len(BEHAVIORS) else v
                beh[name] = beh.get(name, 0) + 1
        if mods.get("SpawnFromDoor", ("", "0"))[1] != "0":
            from_door += 1
        if mods.get("FixedInPlace", ("0", "0"))[0] == "1":
            warnings.append("S20 %s has FixedInPlace on: the game freezes it (SetFrozen), so it never moves or shoots; "
                            "for a sniper holding a post use AddBehaviors Camper with Sniper waypoints instead" % c["name"])
    info["enemy_staging"] = {"enemies": len(chars), "alertness": alerts, "facings": len(faces),
                             "behaviors": dict(sorted(beh.items())), "spawn_from_door": from_door}
    if len(chars) >= 6 and len(faces) == 1 and len(alerts) == 1 and not beh and not from_door:
        warnings.append("S19 all %d enemies face one way, share one alertness and have no added behaviour: every room plays "
                        "as the same fight. Stage them: facing (marker rotation), alertness, AddBehaviors (Camper, HoldCover, "
                        "Defensive, Hunter), SpawnFromDoor" % len(chars))

