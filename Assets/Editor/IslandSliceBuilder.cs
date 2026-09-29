using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;
using Project1028.Subdue;
using Project1028.Report;
using Project1028.Vehicle;
using Project1028.Encounter;
using Project1028.Slice;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>008 염전섬 그레이박스. island_layout.json을 읽어 프리미티브로 생성한다. 메뉴: Tools/PROJECT 1028/Build Island Slice</summary>
    public static class IslandSliceBuilder
    {
        private const string ScenePath = "Assets/Scenes/Island_Slice.unity";
        private const float Half = 60f;

        [MenuItem("Tools/PROJECT 1028/Build Island Slice")]
        public static void Build()
        {
            if (!IslandLayoutLoader.TryLoad(out var layout, out var err)) { Debug.LogError("[IslandSlice] " + err); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(Half / 5f, 1f, Half / 5f); // 120×120
            PlayFoundationSceneBuilder.Tint(ground, new Color(0.32f, 0.38f, 0.28f));
            Walls();

            // 시스템 (001~008)
            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("TimeOfDay").AddComponent<TimeOfDay>();
            new GameObject("NightLighting").AddComponent<NightLighting>();
            new GameObject("SuspicionSystem").AddComponent<SuspicionSystem>();
            new GameObject("SuspicionDebugPanel").AddComponent<SuspicionDebugPanel>();
            new GameObject("SuspicionTestConsole").AddComponent<SuspicionTestConsole>();
            new GameObject("NpcTypeSystem").AddComponent<NpcTypeSystem>();
            new GameObject("NpcTypeTestConsole").AddComponent<NpcTypeTestConsole>();
            new GameObject("SubdueSystem").AddComponent<SubdueSystem>();
            new GameObject("SubdueDebugPanel").AddComponent<SubdueDebugPanel>();
            new GameObject("ReportSystem").AddComponent<ReportSystem>();
            new GameObject("ReportDebugPanel").AddComponent<ReportDebugPanel>();
            new GameObject("ReportTestConsole").AddComponent<ReportTestConsole>();
            new GameObject("EncounterSystem").AddComponent<EncounterSystem>();
            new GameObject("EncounterDebugPanel").AddComponent<EncounterDebugPanel>();
            new GameObject("EncounterTestConsole").AddComponent<EncounterTestConsole>();
            new GameObject("RunDirector").AddComponent<RunDirector>();
            new GameObject("ObjectiveHud").AddComponent<ObjectiveHud>();
            new GameObject("ScreenFx").AddComponent<ScreenFx>();
            new GameObject("ProceduralAudio").AddComponent<ProceduralAudio>();
            new GameObject("EyeIndicator").AddComponent<EyeIndicator>();
            new GameObject("FeedbackDirector").AddComponent<FeedbackDirector>();
            new GameObject("ResultScreen").AddComponent<ResultScreen>();
            new GameObject("SliceTestConsole").AddComponent<SliceTestConsole>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            Vector3 spawn = new Vector3(0f, 1f, -50f);
            Vector3 dockPos = Vector3.zero;
            Vector3 boatPos = new Vector3(0f, 1f, -56f);

            foreach (var f in layout.facilities)
            {
                Vector3 pos = f.position.V; Vector3 size = f.size.V; Quaternion rot = Quaternion.Euler(0f, f.rotationY, 0f);
                switch (f.kind)
                {
                    case "dock": dockPos = pos; Box(f.id, pos + Vector3.up * size.y * 0.5f, size, rot, new Color(0.45f, 0.4f, 0.35f), f.label); break;
                    case "boat": { boatPos = pos; var b = Box(f.id, pos, size, rot, new Color(0.2f, 0.35f, 0.7f), f.label); b.AddComponent<DockBoat>(); break; }
                    case "road": { var r = GameObject.CreatePrimitive(PrimitiveType.Plane); r.name = f.id; r.transform.position = pos; r.transform.rotation = rot; r.transform.localScale = new Vector3(size.x / 10f, 1f, size.z / 10f); Object.DestroyImmediate(r.GetComponent<Collider>()); PlayFoundationSceneBuilder.Tint(r, new Color(0.22f, 0.22f, 0.24f)); break; }
                    case "warehouse": Warehouse(f, pos, size, rot); break;
                    case "office": { var o = Box(f.id, pos + Vector3.up * size.y * 0.5f, size, rot, new Color(0.7f, 0.7f, 0.5f), f.label); var p = o.AddComponent<ReportPoint>(); p.Configure(f.id, ReportPointKind.Office, f.label ?? "관리소"); break; }
                    case "house": Box(f.id, pos + Vector3.up * size.y * 0.5f, size, rot, new Color(0.55f, 0.5f, 0.45f), f.label); break;
                    case "saltfield": { var s = GameObject.CreatePrimitive(PrimitiveType.Plane); s.name = f.id; s.transform.position = pos + Vector3.up * 0.01f; s.transform.localScale = new Vector3(size.x / 10f, 1f, size.z / 10f); Object.DestroyImmediate(s.GetComponent<Collider>()); PlayFoundationSceneBuilder.Tint(s, new Color(0.9f, 0.9f, 0.85f)); break; }
                    case "phone": Phone(f, pos); break;
                    case "evidence": { var e = Box(f.id, pos, size, rot, new Color(0.95f, 0.85f, 0.2f), f.label); var c = e.AddComponent<CarriableObject>(); c.Configure(f.label ?? "장부", true, true, true); e.AddComponent<EvidenceItem>(); break; }
                    case "vehicle": VehicleSceneBuilder.CreateTruck(pos); break;
                    case "trigger": { var t = GameObject.CreatePrimitive(PrimitiveType.Cylinder); t.name = f.id; t.transform.position = new Vector3(pos.x, 0.03f, pos.z); t.transform.localScale = new Vector3(size.x * 2f, 0.02f, size.x * 2f); Object.DestroyImmediate(t.GetComponent<Collider>()); PlayFoundationSceneBuilder.Tint(t, new Color(0.9f, 0.6f, 0.2f)); t.AddComponent<EncounterTrigger>().Configure(f.id, size.x); break; }
                    case "spawn": spawn = pos; break;
                }
            }

            foreach (var z in layout.restrictedZones)
            {
                var go = new GameObject($"Zone_{z.id}");
                go.transform.position = z.center.V;
                go.AddComponent<RestrictedZone>().Configure(z.id, z.size.V);
            }

            foreach (var n in layout.npcs)
            {
                var go = PlayFoundationSceneBuilder.CreateNpc($"NPC_{n.npcId}", n.npcId, n.displayName, n.position.V, n.facing.V, talkable: false);
                go.AddComponent<NpcSuspicionProfile>();
                go.AddComponent<NpcMover>();
                go.AddComponent<NpcSuspicionBehaviour>();
            }

            Player(PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(spawn.x, 1.35f, spawn.z), actions, withInput: true)); // 부두 슬라브(0.3) 위
            Player(PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(boatPos.x + 3f, 1.35f, boatPos.z + 3f), actions, withInput: false)); // 배 반경 안 (전원 출항 조건)

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath); // R 재시작이 이 씬을 다시 로드할 수 있게
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[IslandSlice] 섬 생성 완료: {ScenePath} — 시설 {layout.facilities.Length}, NPC {layout.npcs.Length}");
        }

        private static void AddToBuildSettings(string path)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == path)) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        private static void Player(GameObject go)
        {
            go.AddComponent<PlayerDisguise>();
            go.AddComponent<PlayerHands>();
            go.AddComponent<SubdueActor>();
            go.AddComponent<PlayerToolkit>();
            go.AddComponent<PlayerCutter>();
            go.AddComponent<PlayerActivity>();
            go.AddComponent<EvidenceWatcher>();
        }

        private static GameObject Box(string name, Vector3 center, Vector3 size, Quaternion rot, Color color, string label)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = string.IsNullOrEmpty(label) ? name : $"{name} ({label})";
            go.transform.position = center;
            go.transform.rotation = rot;
            go.transform.localScale = size;
            PlayFoundationSceneBuilder.Tint(go, color);
            return go;
        }

        private static void Warehouse(Facility f, Vector3 pos, Vector3 size, Quaternion rot)
        {
            // 벽 4면, 남쪽(-Z) 면 가운데를 문으로 비움. 지붕 없음(그레이박스).
            var parent = new GameObject($"{f.id} ({f.label})"); parent.transform.position = pos; parent.transform.rotation = rot;
            float t = 0.4f, h = size.y;
            Wall(parent, "N", new Vector3(0f, h / 2f, size.z / 2f), new Vector3(size.x, h, t));
            Wall(parent, "E", new Vector3(size.x / 2f, h / 2f, 0f), new Vector3(t, h, size.z));
            Wall(parent, "W", new Vector3(-size.x / 2f, h / 2f, 0f), new Vector3(t, h, size.z));
            float doorW = 3f; float seg = (size.x - doorW) / 2f;
            Wall(parent, "S_L", new Vector3(-(doorW / 2f + seg / 2f), h / 2f, -size.z / 2f), new Vector3(seg, h, t));
            Wall(parent, "S_R", new Vector3(doorW / 2f + seg / 2f, h / 2f, -size.z / 2f), new Vector3(seg, h, t));
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane); floor.name = "Floor"; floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(0f, 0.015f, 0f); floor.transform.localScale = new Vector3(size.x / 10f, 1f, size.z / 10f);
            Object.DestroyImmediate(floor.GetComponent<Collider>()); PlayFoundationSceneBuilder.Tint(floor, new Color(0.35f, 0.25f, 0.2f));
        }

        private static void Wall(GameObject parent, string name, Vector3 localPos, Vector3 size)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube); w.name = $"Wall_{name}"; w.transform.SetParent(parent.transform, false);
            w.transform.localPosition = localPos; w.transform.localScale = size; PlayFoundationSceneBuilder.Tint(w, new Color(0.5f, 0.3f, 0.25f));
        }

        private static void Phone(Facility f, Vector3 pos)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder); root.name = f.id;
            root.transform.position = pos + new Vector3(0f, 1.2f, 0f); root.transform.localScale = new Vector3(0.3f, 1.2f, 0.3f);
            PlayFoundationSceneBuilder.Tint(root, new Color(0.2f, 0.2f, 0.25f));
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = "Handset"; box.transform.SetParent(root.transform, false);
            box.transform.localPosition = new Vector3(0f, 0.9f, 0f); box.transform.localScale = new Vector3(1.4f, 0.35f, 1.4f);
            Object.DestroyImmediate(box.GetComponent<Collider>()); PlayFoundationSceneBuilder.Tint(box, new Color(0.85f, 0.2f, 0.2f));
            var point = root.AddComponent<ReportPoint>(); point.Configure(f.id, ReportPointKind.Phone, f.label ?? "전화기");
            root.AddComponent<PhoneCutInteractable>();
        }

        private static void Walls()
        {
            var parent = new GameObject("Walls").transform;
            const float h = 4f, t = 0.5f;
            foreach (var (n, p, s) in new[] {
                ("N", new Vector3(0f, h / 2f, Half), new Vector3(2f * Half, h, t)), ("S", new Vector3(0f, h / 2f, -Half), new Vector3(2f * Half, h, t)),
                ("E", new Vector3(Half, h / 2f, 0f), new Vector3(t, h, 2f * Half)), ("W", new Vector3(-Half, h / 2f, 0f), new Vector3(t, h, 2f * Half)) })
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cube); w.name = $"Wall_{n}"; w.transform.SetParent(parent, false);
                w.transform.position = p; w.transform.localScale = s; PlayFoundationSceneBuilder.Tint(w, new Color(0.2f, 0.3f, 0.45f));
            }
        }
    }
}
