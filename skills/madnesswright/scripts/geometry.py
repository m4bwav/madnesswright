"""Surface checks for mpn.py scene-lint (S26 coplanar faces, S27 narrow passages). Standard library only.

S26: two drawn faces facing the same way in the same plane (within `eps`) that overlap in area flicker in game
(z-fighting): the depth buffer cannot pick one, so they swap as the camera moves. Seen in Nexus Prominence build
zzg (2026-11-16): a red wall stripe on the red wall band, floor papers on each other, a spike plate on its hazard
paint, a skyline on its sill glow. Fix: lift one by a few millimetres, shorten or cut one, or merge them.
Checks Unity's built-in cube and quad meshes (the scene file holds their geometry); other meshes are counted.
S27: two solid colliders whose footprints leave a gap narrower than a character (default 1.6 units: the builder
gates' 1.2-unit body plus clearance; an assumption, not a measured capsule) read as a way through but stop the player.
"""
import math
import re

CUBE, QUAD = "10202", "10210"
BUILTIN = "0000000000000000e000000000000000"
NUM = r"(-?[\d.]+(?:e-?\d+)?)"


def _vec(body, name, keys, default):
    m = re.search(r"%s: \{%s\}" % (re.escape(name), ", ".join("%s: %s" % (k, NUM) for k in keys)), body)
    return tuple(float(v) for v in m.groups()) if m else default


def _qrot(q, v):
    x, y, z, w = q
    tx, ty, tz = 2 * (y * v[2] - z * v[1]), 2 * (z * v[0] - x * v[2]), 2 * (x * v[1] - y * v[0])
    return (v[0] + w * tx + y * tz - z * ty, v[1] + w * ty + z * tx - x * tz, v[2] + w * tz + x * ty - y * tx)


def _qmul(a, b):
    ax, ay, az, aw = a
    bx, by, bz, bw = b
    return (aw * bx + ax * bw + ay * bz - az * by, aw * by - ax * bz + ay * bw + az * bx,
            aw * bz + ax * by - ay * bx + az * bw, aw * bw - ax * bx - ay * by - az * bz)


class World:
    """World poses of scene Transforms (stripped prefab roots read from their instance's modifications)."""

    def __init__(self, docs, field, ref):
        self.docs, self.field, self.ref, self.cache, self.t_of_go = docs, field, ref, {}, {}
        for fid, d in docs.items():
            if d["cls"] == 4 and not d["stripped"]:
                self.t_of_go[ref(d["body"], "m_GameObject")[0]] = fid

    def local(self, fid):
        d = self.docs.get(fid)
        if not d:
            return None
        b = d["body"]
        if d["stripped"]:
            inst = self.docs.get(self.ref(b, "m_PrefabInstance")[0])
            if not inst:
                return None
            ib = inst["body"]
            mods, src = {}, self.ref(b, "m_CorrespondingSourceObject")[0]
            flat = re.sub(r",\s*\n\s+type: (\d)", r", type: \1", ib)
            for m in re.finditer(r"target: \{fileID: (-?\d+)[^}]*\}\s*\n\s*propertyPath: (m_Local(?:Position|Rotation|Scale)\.[xyzw])\n\s*value: (\S+)", flat):
                if src is None or m.group(1) == src:
                    mods[m.group(2)] = float(m.group(3))
            g = lambda n, k, dv: mods.get("%s.%s" % (n, k), dv)
            pos = tuple(g("m_LocalPosition", k, 0.0) for k in "xyz")
            rot = tuple(g("m_LocalRotation", k, 1.0 if k == "w" else 0.0) for k in "xyzw")
            scl = tuple(g("m_LocalScale", k, 1.0) for k in "xyz")
            name = next((m.group(1) for m in re.finditer(r"propertyPath: m_Name\n\s*value: (.*)", ib)), None)
            return self.ref(ib, "m_TransformParent")[0], pos, rot, scl, name
        pos = _vec(b, "m_LocalPosition", "xyz", (0.0, 0.0, 0.0))
        rot = _vec(b, "m_LocalRotation", "xyzw", (0.0, 0.0, 0.0, 1.0))
        scl = _vec(b, "m_LocalScale", "xyz", (1.0, 1.0, 1.0))
        go = self.docs.get(self.ref(b, "m_GameObject")[0])
        return self.ref(b, "m_Father")[0], pos, rot, scl, self.field(go["body"], "m_Name") if go else None

    def pose(self, fid, depth=0):
        """(apply(point) -> world point, room label) for a Transform."""
        if fid in self.cache:
            return self.cache[fid]
        loc = self.local(fid) if depth < 64 else None
        if not loc:
            res = (lambda p: p, None)
        else:
            father, pos, rot, scl, name = loc
            papply, room = self.pose(father, depth + 1) if father not in (None, "0") else ((lambda p: p), None)
            if room is None and name and name.startswith("Room_"):
                room = name

            def apply(p, papply=papply, pos=pos, rot=rot, scl=scl):
                r = _qrot(rot, (p[0] * scl[0], p[1] * scl[1], p[2] * scl[2]))
                return papply((r[0] + pos[0], r[1] + pos[1], r[2] + pos[2]))
            res = (apply, room)
        self.cache[fid] = res
        return res


def _faces(kind):
    if kind == QUAD:
        return [[(-.5, -.5, 0), (.5, -.5, 0), (.5, .5, 0), (-.5, .5, 0)]], [(0, 0, -1)]
    faces, normals = [], []
    for ax in range(3):
        for s in (-.5, .5):
            u, v = [a for a in range(3) if a != ax]
            pts = []
            for a, b in ((-.5, -.5), (.5, -.5), (.5, .5), (-.5, .5)):
                p = [0.0, 0.0, 0.0]
                p[ax], p[u], p[v] = s, a, b
                pts.append(tuple(p))
            faces.append(pts)
            normals.append(tuple(1.0 if (i == ax) else 0.0 for i in range(3)) if s > 0 else tuple(-1.0 if (i == ax) else 0.0 for i in range(3)))
    return faces, normals


def _sub(a, b):
    return (a[0] - b[0], a[1] - b[1], a[2] - b[2])


def _dot(a, b):
    return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]


def _cross(a, b):
    return (a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0])


def _clip(poly, a, b):
    """Sutherland-Hodgman: keep the part of poly left of the edge a->b (2D)."""
    out = []
    side = lambda p: (b[0] - a[0]) * (p[1] - a[1]) - (b[1] - a[1]) * (p[0] - a[0])
    for i, p in enumerate(poly):
        q = poly[(i + 1) % len(poly)]
        sp, sq = side(p), side(q)
        if sp >= 0:
            out.append(p)
        if (sp >= 0) != (sq >= 0) and sp != sq:
            t = sp / (sp - sq)
            out.append((p[0] + t * (q[0] - p[0]), p[1] + t * (q[1] - p[1])))
    return out


def _area(poly):
    return 0.5 * sum(poly[i][0] * poly[(i + 1) % len(poly)][1] - poly[(i + 1) % len(poly)][0] * poly[i][1] for i in range(len(poly)))


def _ccw(poly):
    return poly if _area(poly) >= 0 else poly[::-1]


def overlap_area(p, q):
    out = _ccw(p)
    q = _ccw(q)
    for i in range(len(q)):
        if not out:
            return 0.0
        out = _clip(out, q[i], q[(i + 1) % len(q)])
    return abs(_area(out)) if len(out) >= 3 else 0.0


def surfaces(docs, field, ref, seen_only=True):
    """Yield (name, room, world corners, unit normal) for every face of an enabled built-in cube or quad renderer."""
    world = World(docs, field, ref)
    mesh_of, drawn, other = {}, set(), 0
    for d in docs.values():
        if d["stripped"]:
            continue
        if d["cls"] == 33:
            m = re.search(r"m_Mesh: \{fileID: (-?\d+)(?:, guid: ([0-9a-f]+))?", d["body"])
            if m:
                mesh_of[ref(d["body"], "m_GameObject")[0]] = m.group(1) if (m.group(2) or "") == BUILTIN else "other"
        elif d["cls"] == 23 and field(d["body"], "m_Enabled") != "0":
            drawn.add(ref(d["body"], "m_GameObject")[0])
    out = []
    for go in drawn:
        kind = mesh_of.get(go)
        if kind not in (CUBE, QUAD):
            other += kind is not None
            continue
        g = docs.get(go)
        if not g or field(g["body"], "m_IsActive") == "0" or go not in world.t_of_go:
            continue
        apply, room = world.pose(world.t_of_go[go])
        name = field(g["body"], "m_Name")
        centre = apply((0.0, 0.0, 0.0))
        for pts, n in zip(*_faces(kind)):
            w = [apply(p) for p in pts]
            nn = _cross(_sub(w[1], w[0]), _sub(w[3], w[0]))
            ln = math.sqrt(_dot(nn, nn))
            if ln < 1e-9:
                continue
            nn = (nn[0] / ln, nn[1] / ln, nn[2] / ln)
            facing = _sub(apply(n), centre) if kind == CUBE else _sub(apply((0, 0, -1)), apply((0, 0, 0)))
            if _dot(nn, facing) < 0:
                nn = (-nn[0], -nn[1], -nn[2])
            out.append((name, room, w, nn, go))
    if seen_only:
        # the game camera looks down into each room from above its open front (-z), over the middle: a face is seen
        # only when that point lies on its front side (drops floor-level undersides, backs and wall-embedded ends)
        box = {}
        for f in out:
            b = box.setdefault(f[1], [[1e9] * 3, [-1e9] * 3])
            for p in f[2]:
                for i in range(3):
                    b[0][i], b[1][i] = min(b[0][i], p[i]), max(b[1][i], p[i])
        cam = {r: ((lo[0] + hi[0]) / 2, hi[1] + (hi[1] - lo[1]), lo[2] - (hi[2] - lo[2])) for r, (lo, hi) in box.items()}
        out = [f for f in out if _dot(f[3], _sub(cam[f[1]], f[2][0])) > 1e-6]
    return out, other


DECAL = re.compile(r"decal|paper|stain|splat|poster|sign", re.I)


def coplanar_check(docs, warnings, info, field, ref, eps=0.002, min_area=0.004, cap=40):
    """S26: same-facing coplanar faces (within eps units) of different renderers that overlap by min_area or more."""
    faces, other = surfaces(docs, field, ref)
    if not faces:
        return
    groups = {}
    for i, (_, room, w, n, _g) in enumerate(faces):
        key = (room, round(n[0], 2), round(n[1], 2), round(n[2], 2))
        groups.setdefault(key, []).append((_dot(n, w[0]), i))
    pairs = []
    for key, items in groups.items():
        items.sort()
        n = faces[items[0][1]][3]
        a = (1.0, 0.0, 0.0) if abs(n[0]) < 0.9 else (0.0, 1.0, 0.0)
        e1 = _cross(n, a)
        l1 = math.sqrt(_dot(e1, e1))
        e1 = (e1[0] / l1, e1[1] / l1, e1[2] / l1)
        e2 = _cross(n, e1)
        flat = lambda w: [(_dot(p, e1), _dot(p, e2)) for p in w]
        for x in range(len(items)):
            dx, ix = items[x]
            for y in range(x + 1, len(items)):
                dy, iy = items[y]
                if dy - dx > eps:
                    break
                fa, fb = faces[ix], faces[iy]
                if fa[4] == fb[4]:
                    continue
                pa, pb = flat(fa[2]), flat(fb[2])
                if (max(p[0] for p in pa) <= min(p[0] for p in pb) or max(p[0] for p in pb) <= min(p[0] for p in pa)
                        or max(p[1] for p in pa) <= min(p[1] for p in pb) or max(p[1] for p in pb) <= min(p[1] for p in pa)):
                    continue
                ar = overlap_area(pa, pb)
                if ar >= min_area:
                    pairs.append({"room": key[0], "a": fa[0], "b": fb[0], "normal": [round(v, 2) for v in key[1:]],
                                  "plane": round(dx, 3), "area": round(ar, 3), "decals": bool(DECAL.search(fa[0] or "") and DECAL.search(fb[0] or ""))})
    pairs.sort(key=lambda p: -p["area"])
    rooms = {}
    for p in pairs:
        rooms[p["room"]] = rooms.get(p["room"], 0) + 1
    info["coplanar"] = {"faces": len(faces), "other_meshes": other, "pairs": len(pairs),
                        "decal_pairs": sum(p["decals"] for p in pairs), "by_room": rooms, "worst": pairs[:cap]}
    if pairs:
        warnings.append("S26 %d pairs of same-facing coplanar faces overlap (z-fighting: they flicker in game), %d of them flat "
                        "decals on each other; lift one a few millimetres, cut or merge them (info coplanar.worst)"
                        % (len(pairs), info["coplanar"]["decal_pairs"]))


def narrow_gap_check(docs, warnings, info, field, ref, player_w=1.6, min_gap=0.3, min_h=0.8, cap=30):
    """S27: two solid BoxColliders (not Interactive ramps, not floors) in one room, both at least min_h tall and
    overlapping in height, whose footprints are min_gap to player_w apart: it looks like a way through and is not."""
    world = World(docs, field, ref)
    solids = []
    for d in docs.values():
        if d["cls"] != 65 or d["stripped"] or field(d["body"], "m_Enabled") == "0":
            continue
        go = ref(d["body"], "m_GameObject")[0]
        g = docs.get(go)
        if not g or go not in world.t_of_go or field(g["body"], "m_Layer") in ("8", "12") or field(g["body"], "m_TagString") == "Floor":
            continue
        c = _vec(d["body"], "m_Center", "xyz", (0.0, 0.0, 0.0))
        s = _vec(d["body"], "m_Size", "xyz", (1.0, 1.0, 1.0))
        apply, room = world.pose(world.t_of_go[go])
        pts = [apply((c[0] + a * s[0] / 2, c[1] + b * s[1] / 2, c[2] + e * s[2] / 2)) for a in (-1, 1) for b in (-1, 1) for e in (-1, 1)]
        lo = [min(p[i] for p in pts) for i in range(3)]
        hi = [max(p[i] for p in pts) for i in range(3)]
        if hi[1] - lo[1] >= min_h:
            solids.append((field(g["body"], "m_Name"), room, lo, hi))
    found = []
    for i, (na, ra, la, ha) in enumerate(solids):
        for nb, rb, lb, hb in solids[i + 1:]:
            if ra != rb or min(ha[1], hb[1]) - max(la[1], lb[1]) < min_h / 2:
                continue
            gx = max(0.0, lb[0] - ha[0], la[0] - hb[0])
            gz = max(0.0, lb[2] - ha[2], la[2] - hb[2])
            gap = math.hypot(gx, gz)
            if not min_gap <= gap < player_w:
                continue
            # the space between them, in plan; a third solid inside it (a pillar between two barricades) closes it
            span = [(min(ha[i], hb[i]), max(la[i], lb[i])) if (lb[i] > ha[i] or la[i] > hb[i]) else (max(la[i], lb[i]), min(ha[i], hb[i])) for i in (0, 2)]
            if any(r == ra and n not in (na, nb) and lo[0] < span[0][1] and hi[0] > span[0][0] and lo[2] < span[1][1] and hi[2] > span[1][0]
                   for n, r, lo, hi in solids):
                continue
            found.append({"room": ra, "a": na, "b": nb, "gap": round(gap, 2)})
    found.sort(key=lambda p: p["gap"])
    info["narrow_gaps"] = {"solids": len(solids), "player_width": player_w, "pairs": len(found), "worst": found[:cap]}
    if found:
        warnings.append("S27 %d pairs of solids leave a gap of %.1f to %.1f units, narrower than a character (%.1f): it reads as a "
                        "way through and stops the player; close it or widen it (info narrow_gaps.worst)" % (len(found), min_gap, player_w, player_w))
