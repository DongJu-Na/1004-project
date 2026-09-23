using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Project1028.PlayFoundation;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>
    /// 테스트 씬을 프리미티브만으로 자동 생성한다 (헌장 원칙 II·III). 수작업 씬 편집 없음.
    /// 메뉴: Tools/PROJECT 1028/Build Play Foundation Test Scene [(2 Players)]
    /// </summary>
    public static class PlayFoundationSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_PlayFoundation.unity";
        public const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("Tools/PROJECT 1028/Build Play Foundation Test Scene")]
        public static void BuildSinglePlayer() => Build(false);

        [MenuItem("Tools/PROJECT 1028/Build Play Foundation Test Scene (2 Players)")]
        public static void BuildTwoPlayers() => Build(true);

        public static void Build(bool twoPlayers)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLight();
            CreateGround();
            CreateWalls();
            CreateOcclusionWall(new Vector3(4f, 1.5f, 2.5f)); // NPC 정면 3.5m 앞 (T032)

            var hud = new GameObject("RuntimeHud");
            hud.AddComponent<RuntimeHud>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                Debug.LogError($"[SceneBuilder] 입력 액션 에셋을 찾을 수 없습니다: {InputActionsPath}");
            }

            CreatePlayer("P1", new Vector3(0f, 1f, -8f), actions, withInput: true);
            if (twoPlayers)
            {
                CreatePlayer("P2", new Vector3(4f, 1f, 9f), actions, withInput: false); // NPC 후면
            }

            CreateNpc("NPC_dock_worker", "npc_dock_worker", "부두 노동자", new Vector3(4f, 1f, 6f), Vector3.back);
            // 선택 규칙 Play 검증용 두 번째 NPC (같은 대화 파일 참조)
            CreateNpc("NPC_dock_worker_2", "npc_dock_worker", "부두 노동자 (2)", new Vector3(1.5f, 1f, 6f), Vector3.back);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] 테스트 씬 생성 완료: {ScenePath} (2 Players: {twoPlayers})");
        }

        public static void CreateLight()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        public static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(4f, 1f, 4f); // 40 x 40
            Tint(ground, new Color(0.35f, 0.4f, 0.3f));
        }

        public static void CreateWalls()
        {
            var parent = new GameObject("Walls").transform;
            const float half = 20f;
            const float h = 3f;
            const float t = 0.5f;
            Wall(parent, "Wall_N", new Vector3(0f, h / 2f, half - t / 2f), new Vector3(2f * half, h, t));
            Wall(parent, "Wall_S", new Vector3(0f, h / 2f, -half + t / 2f), new Vector3(2f * half, h, t));
            Wall(parent, "Wall_E", new Vector3(half - t / 2f, h / 2f, 0f), new Vector3(t, h, 2f * half));
            Wall(parent, "Wall_W", new Vector3(-half + t / 2f, h / 2f, 0f), new Vector3(t, h, 2f * half));
        }

        public static void CreateOcclusionWall(Vector3 position)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "OcclusionWall";
            wall.transform.position = position;
            wall.transform.localScale = new Vector3(4f, 3f, 0.5f);
            Tint(wall, new Color(0.5f, 0.3f, 0.25f));
        }

        private static void Wall(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            Tint(wall, new Color(0.55f, 0.55f, 0.6f));
        }

        public static GameObject CreatePlayer(string id, Vector3 position, InputActionAsset actions, bool withInput)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Player_{id}";
            go.transform.position = position;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Tint(go, withInput ? new Color(0.25f, 0.5f, 0.95f) : new Color(0.3f, 0.8f, 0.9f));

            var controller = go.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = Vector3.zero; // Capsule 프리미티브 pivot = 중심

            var entity = go.AddComponent<PlayerEntity>();
            entity.Configure(id, 0f);

            PlayerInput input = null;
            if (withInput && actions != null)
            {
                input = go.AddComponent<PlayerInput>();
                input.actions = actions;
                input.defaultActionMap = "Player";
                input.neverAutoSwitchControlSchemes = false;
            }

            go.AddComponent<ThirdPersonMotor>();
            go.AddComponent<FallRespawn>();
            go.AddComponent<DialogueRunner>();
            go.AddComponent<InteractionDetector>();

            if (withInput)
            {
                var camGo = new GameObject($"Camera_{id}");
                camGo.tag = "MainCamera";
                camGo.transform.position = position + new Vector3(0f, 2f, -4.5f);
                camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                var orbit = camGo.AddComponent<OrbitCamera>();
                orbit.Configure(go.transform, entity, input);
                entity.Camera = orbit;
            }
            return go;
        }

        public static GameObject CreateNpc(string objectName, string npcId, string displayName, Vector3 position, Vector3 facing) =>
            CreateNpc(objectName, npcId, displayName, position, facing, talkable: true);

        public static GameObject CreateNpc(string objectName, string npcId, string displayName, Vector3 position, Vector3 facing, bool talkable)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = objectName;
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            Tint(go, new Color(0.9f, 0.6f, 0.3f));

            // 정면 표시용 코 (프리미티브)
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            nose.transform.SetParent(go.transform, false);
            nose.transform.localPosition = new Vector3(0f, 0.6f, 0.45f);
            nose.transform.localScale = new Vector3(0.15f, 0.15f, 0.3f);
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            Tint(nose, new Color(0.95f, 0.85f, 0.7f));

            var identity = go.AddComponent<NpcIdentity>();
            identity.Configure(npcId, displayName, 0.6f); // pivot 중심 기준 → 월드 눈 높이 1.6m
            var vision = go.AddComponent<NpcVision>();
            vision.Configure(100f, 10f, 0.15f);
            go.AddComponent<VisionDebugLabel>();
            if (talkable) go.AddComponent<NpcTalkInteractable>();
            return go;
        }

        public static void Tint(GameObject go, Color color)
        {
            // 에디터에서 만든 비에셋 Material은 씬 저장 시 유실되므로 런타임 컴포넌트로 위임한다.
            var tint = go.GetComponent<PrimitiveTint>() ?? go.AddComponent<PrimitiveTint>();
            tint.SetColor(color);
        }
    }
}
