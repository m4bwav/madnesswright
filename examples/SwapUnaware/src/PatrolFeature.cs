using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwapUnaware
{
    // F3: patrol squares like Ancient Warfare 3's patrol mode. They belong to the body you control
    // when you place them and only show while you control it. A black square follows the mouse;
    // left click drops it; U starts another; click a square to select it, then Delete removes it
    // and P turns it red (a stop: the unit halts there). F3 hides and shows them. Whenever the owner
    // is not yours and has not noticed an enemy, it walks the squares in a loop.
    public class PatrolFeature : Feature
    {
        public KeyCode ToggleKey = KeyCode.F3;
        public KeyCode NewKey = KeyCode.U;
        public KeyCode StopKey = KeyCode.P;
        public KeyCode DeleteKey = KeyCode.Delete;

        // Close enough to count as arrived (world units, flat distance).
        const float ArriveDistance = 0.8f;
        const float ReissueSeconds = 1f;
        const float SquarePixels = 18f;

        internal class Point { public Vector3 pos; public bool stop; }
        internal class Route { public List<Point> points = new List<Point>(); public int next; public bool done; public float nextIssue; public bool wander; }

        internal static readonly Dictionary<Combatant_Base, Route> routes = new Dictionary<Combatant_Base, Route>();

        bool shown, placing;
        int selected = -1;
        Combatant_Base lastMe;
        Texture2D white;

        protected override string Name { get { return "patrol (F3)"; } }

        internal static bool HasRoute(Combatant_Base c)
        {
            Route r;
            return c != null && routes.TryGetValue(c, out r) && r.points.Count > 0;
        }

        protected override void Tick()
        {
            var me = Game.Me();
            if (me != lastMe)
            {
                // The body you just left starts its patrol from a calm state.
                if (HasRoute(lastMe) && Game.IsNpc(lastMe))
                {
                    Game.ReleaseClaims(lastMe.myController);
                    Controller_Base.SetAlertStatus(lastMe.myController, AI_Wakeup.AlertStatus.Unaware);
                    routes[lastMe].done = false;
                }
                lastMe = me;
                selected = -1;
                placing = false;
            }
            if (me != null) Edit(me);
            Game.MouseBusy = shown && me != null && (placing || SquareUnderMouse(me) >= 0);
            Walk(me);
        }

        void Edit(Combatant_Base me)
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                shown = !shown;
                placing = shown && !HasRoute(me);
                selected = -1;
                Game.Say(shown ? "patrol squares shown: left click drops, U new square, click a square then P stop / Delete remove, F3 hide" : "patrol squares hidden");
            }
            if (Input.GetKeyDown(NewKey))
            {
                shown = true;
                placing = true;
                selected = -1;
            }
            if (!shown) return;
            Route r = RouteOf(me);
            if (Input.GetMouseButtonDown(0))
            {
                int hit = SquareUnderMouse(me);
                if (placing)
                {
                    r.points.Add(new Point { pos = me.aim_Intended });
                    placing = false;
                    selected = r.points.Count - 1;
                    Restart(me, r);
                    LockClick();
                }
                else if (hit >= 0)
                {
                    selected = hit;
                    LockClick();
                }
                else selected = -1;
            }
            if (selected >= 0 && selected < r.points.Count)
            {
                if (Input.GetKeyDown(StopKey))
                {
                    r.points[selected].stop = !r.points[selected].stop;
                    Restart(me, r);
                }
                if (Input.GetKeyDown(DeleteKey))
                {
                    r.points.RemoveAt(selected);
                    selected = -1;
                    Restart(me, r);
                    if (r.points.Count == 0) DropRoute(me);
                }
            }
        }

        // The press that dropped or selected a square must not also fire a shot.
        static void LockClick()
        {
            var pc = Controller_Player.PlayerController;
            if (pc != null) pc.LockAttackKey(0);
        }

        static Route RouteOf(Combatant_Base c)
        {
            Route r;
            if (!routes.TryGetValue(c, out r))
            {
                r = new Route { wander = c.wanderingDisabled };
                routes[c] = r;
            }
            return r;
        }

        static void Restart(Combatant_Base c, Route r)
        {
            r.next = 0;
            r.done = false;
            r.nextIssue = 0f;
            c.wanderingDisabled = true;
        }

        static void DropRoute(Combatant_Base c)
        {
            Route r;
            if (!routes.TryGetValue(c, out r)) return;
            routes.Remove(c);
            if (c != null && !GuardFeature.IsGuard(c)) c.wanderingDisabled = r.wander;
        }

        internal static void SetRoute(Combatant_Base c, List<Point> points)
        {
            var r = RouteOf(c);
            r.points = points;
            Restart(c, r);
        }

        // Owners that are NPCs, alive, not frozen or guarding and not in a fight walk their squares.
        static void Walk(Combatant_Base me)
        {
            foreach (var pair in routes.ToList())
            {
                var c = pair.Key;
                var r = pair.Value;
                if (c == null || c.AmDead()) { routes.Remove(c); continue; }
                if (c == me || r.done || r.points.Count == 0 || !Game.IsNpc(c)) continue;
                if (FreezeFeature.IsFrozen(c) || GuardFeature.IsGuard(c)) continue;
                if (c.myController.myAlertStatus != AI_Wakeup.AlertStatus.Unaware) continue;
                if (r.next >= r.points.Count) r.next = 0;
                var p = r.points[r.next];
                var d = c.Position - p.pos;
                d.y = 0f;
                if (d.magnitude <= ArriveDistance)
                {
                    if (p.stop)
                    {
                        r.done = true;
                        Pathfind_Main.ClearWaypointsAndTarget(c);
                        continue;
                    }
                    r.next = (r.next + 1) % r.points.Count;
                    r.nextIssue = 0f;
                }
                else if (Time.time >= r.nextIssue && c.myWaypoints.Count == 0)
                {
                    // Within a room this paths all the way; to another room it only reaches the door,
                    // so it is re-issued once the waypoints run out.
                    AI_Manager.AssignWalkToPosition(c.myController, p.pos);
                    r.nextIssue = Time.time + ReissueSeconds;
                }
            }
        }

        int SquareUnderMouse(Combatant_Base me)
        {
            Route r;
            var cam = Game.Cam();
            if (cam == null || !routes.TryGetValue(me, out r)) return -1;
            float size = SquarePixels * Screen.height / 1080f;
            Vector2 mouse = Input.mousePosition;
            for (int i = 0; i < r.points.Count; i++)
            {
                var sp = cam.WorldToScreenPoint(r.points[i].pos);
                if (sp.z > 0f && Mathf.Abs(sp.x - mouse.x) <= size && Mathf.Abs(sp.y - mouse.y) <= size) return i;
            }
            return -1;
        }

        void OnGUI()
        {
            if (!shown) return;
            var me = Game.Me();
            var cam = Game.Cam();
            if (me == null || cam == null) return;
            if (white == null) white = Texture2D.whiteTexture;
            float size = SquarePixels * Screen.height / 1080f;
            Route r;
            if (routes.TryGetValue(me, out r))
            {
                for (int i = 0; i < r.points.Count; i++)
                {
                    var sp = cam.WorldToScreenPoint(r.points[i].pos);
                    if (sp.z <= 0f) continue;
                    var rect = new Rect(sp.x - size / 2f, Screen.height - sp.y - size / 2f, size, size);
                    if (i == selected) Square(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), Color.yellow);
                    Square(rect, r.points[i].stop ? Color.red : Color.black);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(rect.x + size, rect.y - 4f, 40f, 22f), (i + 1).ToString());
                }
            }
            if (placing)
            {
                Vector2 m = Input.mousePosition;
                Square(new Rect(m.x - size / 2f, Screen.height - m.y - size / 2f, size, size), new Color(0f, 0f, 0f, 0.7f));
            }
            GUI.color = Color.white;
        }

        void Square(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, white);
        }

        void OnDestroy()
        {
            Game.MouseBusy = false;
            foreach (var c in routes.Keys.ToList()) DropRoute(c);
        }
    }
}
