"""scene-lint S28-S30 (stdlib). S28: a stock spike strip shown and hidden by a CO_ togglable with no Hazard_Core in
its lists never hurts (the strip acts only on its own SetState). S29 (--expect-spawn): a spawner door type the map
must have is missing or unused. S30: a Hazard_Core an alert character dodges (HitAvoid not -1, damageEffect not
IgnoreThreshold) or with no trigger collider / Rigidbody. Sources: scenetools-rules.md, Nexus Prominence zzi, zzk."""
import re

SPIKES = "20"  # ProxyObj_Togglable.ProxyType.Spikes, also the PX_Togglable prefab's default
DOOR_TYPES = {"Manhole": 200, "FloorGrate": 201, "Fissure": 202, "FissureGlow": 203, "Ground": 204, "GroundWood": 205,
              "Vent": 206, "VentDrop": 207, "VentSide": 208, "VentFan": 209, "ZedFan": 210, "Rapel": 211}


def _ref_list(body, name):
    m = re.search(r"^(\s*)%s:(.*)$" % re.escape(name), body, re.MULTILINE)
    if not m or m.group(2).strip() == "[]":
        return []
    out = []
    for line in body[m.end():].splitlines()[1:]:
        r = re.match(r"\s*- \{fileID: (-?\d+)", line)
        if not r:
            break
        out.append(r.group(1))
    return out


def _gos_of_instance(docs, inst_fid, cls=1):
    return {fid for fid, d in docs.items() if (cls is None or d["cls"] == cls) and d["stripped"]
            and re.search(r"m_PrefabInstance: \{fileID: %s\}" % re.escape(inst_fid), d["body"])}


def spike_hazard_check(docs, instances, errors, info, mod_refs, ref, default_type=SPIKES):
    spikes = {}
    for i in (i for i in instances if i["kind"] == "PX_Togglable"):
        t = {p: v for p, v, _ in mod_refs(docs[i["fid"]]["body"])}.get("Type", default_type)
        if t == SPIKES:
            for go in _gos_of_instance(docs, i["fid"]):
                spikes[go] = i["name"]
    if not spikes:
        return
    hazard_gos = {ref(d["body"], "m_GameObject")[0] for d in docs.values()
                  if d["cls"] == 114 and not d["stripped"] and re.search(r"^\s*contactDmg:", d["body"], re.MULTILINE)}
    rows, bad = [], 0
    for d in docs.values():
        if d["cls"] != 114 or d["stripped"] or not re.search(r"^\s*UpObjs:", d["body"], re.MULTILINE):
            continue
        listed = _ref_list(d["body"], "UpObjs") + _ref_list(d["body"], "DownObjs")
        hit = [spikes[g] for g in listed if g in spikes]
        if not hit:
            continue
        hurts = any(g in hazard_gos for g in listed)
        serial = (re.search(r"^\s*SerialNumber: (.*)$", d["body"], re.MULTILINE) or [None, "?"])[1]
        rows.append({"driver": serial.strip(), "strips": hit, "hazard": hurts})
        if not hurts:
            bad += 1
    info["spike_drivers"] = rows
    if bad:
        errors.append("S28 %d spike strip(s) are shown and hidden by a custom togglable with no hazard in its lists: the game's strip "
                      "only rises and hurts when its own state changes, so these never hurt anyone. Add an invisible trigger with "
                      "Hazard_Core (myHitFrequency OnceTilReset, HitAvoid -1, damageEffect IgnoreThreshold) to the same Up list (info spike_drivers)" % bad)


def spawn_expect_check(docs, instances, errors, info, mod_refs, expect):
    if not expect:
        return
    door_type = {}
    for i in (i for i in instances if i["kind"] == "PX_Door"):
        t = {p: v for p, v, _ in mod_refs(docs[i["fid"]]["body"])}.get("Type", "0")
        for obj in _gos_of_instance(docs, i["fid"], cls=None):
            door_type[obj] = t
        door_type[i["fid"]] = t
    used = set()
    for q in (i for i in instances if i["kind"] == "Squad_Spawner"):
        for p, v, obj in mod_refs(docs[q["fid"]]["body"]):
            if p.startswith("SpawnFromDoors.Array.data[") and obj in door_type:
                used.add(door_type[obj])
    found = {}
    for name in expect:
        code = str(DOOR_TYPES.get(name, name))
        have = sum(1 for i in instances if i["kind"] == "PX_Door"
                   and {p: v for p, v, _ in mod_refs(docs[i["fid"]]["body"])}.get("Type", "0") == code)
        found[name] = {"doors": have, "used_by_squad": code in used}
        if not have:
            errors.append("S29 the map is expected to have %s spawn doors and has none: whatever was meant to arrive that way "
                          "(a rappel team, vent crawlers) never comes" % name)
        elif code not in used:
            errors.append("S29 %d %s spawn door(s) but no Squad_Spawner lists one in SpawnFromDoors: nobody arrives through them" % (have, name))
    info["expected_spawns"] = found


COLLIDERS = {64, 65, 135, 136}  # MeshCollider, BoxCollider, SphereCollider, CapsuleCollider
DODGE_PROOF = {"2"}  # DamageEffect.IgnoreThreshold (SceneTools enum: NoKill, Standard, IgnoreThreshold, NoKillPlayer)


def _field(body, name):
    m = re.search(r"^\s*%s: (.*)$" % re.escape(name), body, re.MULTILINE)
    return m.group(1).strip() if m else None


def hazard_dodge_check(docs, errors, warnings, info, ref):
    """S30. Decompiled game: Hazard_Core.hitRange = (int)HitAvoid, Hazard_Main marks the hit DamageSource.Terrain unless
    IgnoreThreshold or KillHit, and Combat_CharacterHit skips the dodge only when range == -1, so an alert character's
    tac bar turns the hit into a TacDodge (no damage, no log). Hazard_Main acts on OnTriggerEnter: it needs a trigger
    collider on its object and a Rigidbody there or on a parent. The game's Samples/CO_Vessel uses -1 and 2."""
    comps, parent, tgo = {}, {}, {}
    for fid, d in docs.items():
        if d["stripped"] or d["cls"] == 1:
            continue
        g = ref(d["body"], "m_GameObject")
        if not g:
            continue
        comps.setdefault(g[0], []).append(d)
        if d["cls"] == 4:
            f = ref(d["body"], "m_Father")
            parent[g[0]] = f[0] if f else "0"
            tgo[fid] = g[0]

    def has(go, test):
        return any(test(c) for c in comps.get(go, []))

    rows, dodged, broken = [], 0, 0
    for d in docs.values():
        if d["cls"] != 114 or d["stripped"] or _field(d["body"], "contactDmg") is None:
            continue
        g = ref(d["body"], "m_GameObject")
        if not g:
            continue
        go = g[0]
        avoid, effect = _field(d["body"], "HitAvoid") or "-1", _field(d["body"], "damageEffect") or "1"
        trigger = has(go, lambda c: c["cls"] in COLLIDERS and _field(c["body"], "m_IsTrigger") == "1")
        body, cur, seen = False, go, set()
        while cur and cur != "0" and cur not in seen:
            seen.add(cur)
            if has(cur, lambda c: c["cls"] == 54):
                body = True
                break
            cur = tgo.get(parent.get(cur, "0"))
        name = _field(docs.get(go, {"body": ""})["body"], "m_Name") or go
        rows.append({"hazard": name, "HitAvoid": avoid, "damageEffect": effect, "trigger": trigger, "rigidbody": body})
        dodged += avoid != "-1" and effect not in DODGE_PROOF
        broken += not (trigger and body)
    if rows:
        info["hazards"] = rows
    if dodged:
        warnings.append("S30 %d Hazard_Core(s) with HitAvoid not -1 and damageEffect not IgnoreThreshold: the tac bar dodges it, "
                        "no damage (an alert player never gets hurt). Set HitAvoid Undodgable (-1) and damageEffect "
                        "IgnoreThreshold, as the game's CO_Vessel sample (info hazards)" % dodged)
    if broken:
        errors.append("S30 %d Hazard_Core(s) without a trigger collider on the object or a Rigidbody on it or a parent: "
                      "OnTriggerEnter never fires, so it never hurts (info hazards)" % broken)
