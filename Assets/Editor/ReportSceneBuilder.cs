using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;
using Project1028.Subdue;
using Project1028.Report;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>005 테스트 씬. 메뉴: Tools/PROJECT 1028/Build Report Test Scene</summary>
    public static class ReportSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_Report.unity";

        [MenuItem("Tools/PROJECT 1028/Build Report Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            PlayFoundationSceneBuilder.CreateGround();
            PlayFoundationSceneBuilder.CreateWalls();

            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("TimeOfDay").AddComponent<TimeOfDay>();
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

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            Player(PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(0f, 1f, -6f), actions, withInput: true));
            Player(PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(12f, 1f, 12f), actions, withInput: false));

            Npc("NPC_W1", "rep_w1", "W1 감시자", new Vector3(0f, 1f, 6f));
            Npc("NPC_W2", "rep_w2", "W2 감시자", new Vector3(6f, 1f, 2f));
            Npc("NPC_Q",  "rep_q",  "Q 침묵자",  new Vector3(-6f, 1f, 2f));

            Phone("Phone_A", "phone_a", "전화기 A", new Vector3(-6f, 0f, 10f));
            Phone("Phone_B", "phone_b", "전화기 B", new Vector3(14f, 0f, -10f));
            Office("Office", "office", "관리소", new Vector3(0f, 0f, 17f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] 신고 테스트 씬 생성 완료: {ScenePath}");
        }

        private static void Player(GameObject go)
        {
            go.AddComponent<PlayerDisguise>();
            go.AddComponent<PlayerHands>();
            go.AddComponent<SubdueActor>();
            go.AddComponent<PlayerToolkit>();
            go.AddComponent<PlayerCutter>();
        }

        private static void Npc(string name, string npcId, string display, Vector3 pos)
        {
            var go = PlayFoundationSceneBuilder.CreateNpc(name, npcId, display, pos, Vector3.back, talkable: false);
            go.AddComponent<NpcSuspicionProfile>();
            go.AddComponent<NpcMover>();
            go.AddComponent<NpcSuspicionBehaviour>();
        }

        private static void Phone(string name, string id, string display, Vector3 pos)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            root.name = name;
            root.transform.position = pos + new Vector3(0f, 1.2f, 0f);
            root.transform.localScale = new Vector3(0.3f, 1.2f, 0.3f);
            PlayFoundationSceneBuilder.Tint(root, new Color(0.2f, 0.2f, 0.25f));
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Handset";
            box.transform.SetParent(root.transform, false);
            box.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            box.transform.localScale = new Vector3(1.4f, 0.35f, 1.4f);
            Object.DestroyImmediate(box.GetComponent<Collider>());
            PlayFoundationSceneBuilder.Tint(box, new Color(0.85f, 0.2f, 0.2f));
            var point = root.AddComponent<ReportPoint>();
            point.Configure(id, ReportPointKind.Phone, display);
            root.AddComponent<PhoneCutInteractable>();
        }

        private static void Office(string name, string id, string display, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos + new Vector3(0f, 1.25f, 0f);
            go.transform.localScale = new Vector3(3f, 2.5f, 3f);
            PlayFoundationSceneBuilder.Tint(go, new Color(0.7f, 0.7f, 0.5f));
            var point = go.AddComponent<ReportPoint>();
            point.Configure(id, ReportPointKind.Office, display);
        }
    }
}
