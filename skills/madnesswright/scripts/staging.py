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
    if not chars and not any(i["kind"] == "Squad_Spawner" for i in instances):
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
    # Nexus Prominence round 7: placed enemies plus every squad's units, per squad, so a count table (a film's,
    # a difficulty budget's) can be checked from the scene: placed + backup, not placed alone.
    squads = {}
    for q in (i for i in instances if i["kind"] == "Squad_Spawner"):
        n = sum(int(v) for p, v, _ in mod_refs(docs[q["fid"]]["body"]) if p.startswith("Wave.WaveUnits.Array.data[") and p.endswith("].Total") and v.isdigit())
        squads[q["name"]] = squads.get(q["name"], 0) + n
    info["enemy_staging"] = {"enemies": len(chars), "alertness": alerts, "facings": len(faces),
                             "behaviors": dict(sorted(beh.items())), "spawn_from_door": from_door,
                             "squad_units": sum(squads.values()), "squads": squads,
                             "total_with_backup": len(chars) + sum(squads.values())}
    if len(chars) >= 6 and len(faces) == 1 and len(alerts) == 1 and not beh and not from_door:
        warnings.append("S19 all %d enemies face one way, share one alertness and have no added behaviour: every room plays "
                        "as the same fight. Stage them: facing (marker rotation), alertness, AddBehaviors (Camper, HoldCover, "
                        "Defensive, Hunter), SpawnFromDoor" % len(chars))



def sample_shell_check(docs, warnings, info, field, ref):
    """S21: the SceneTools sample room's wall mesh (room_pg_*walls*, not the hidden 4th wall) is still drawn.
    Players who know the Playground read every room built from it as the Playground (Nexus Prominence round 9).
    Keep its collider (room size, doors, camera and AI stay the same), switch the renderer off (the game never
    re-enables room MeshRenderers) and draw the map's own walls where the collider is solid."""
    drawn, hidden = {}, {}
    for d in docs.values():
        if d["cls"] != 23 or d["stripped"]:
            continue
        go = docs.get(ref(d["body"], "m_GameObject")[0])
        name = field(go["body"], "m_Name") if go else None
        if not name or not name.startswith("room_pg_"):
            continue
        bucket = hidden if field(d["body"], "m_Enabled") == "0" else drawn
        bucket[name] = bucket.get(name, 0) + 1
    if not drawn and not hidden:
        return
    info["sample_shell"] = {"drawn": drawn, "hidden": hidden}
    walls = sum(n for k, n in drawn.items() if "walls" in k)
    if walls:
        warnings.append("S21 the sample room's wall mesh is drawn in %d rooms; players read it as the Playground. "
                        "Keep its collider, turn its renderer off and draw your own walls where the collider is solid" % walls)
