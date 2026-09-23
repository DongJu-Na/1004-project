using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;
using Project1028.Subdue;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>004 테스트 씬. 메뉴: Tools/PROJECT 1028/Build Subdue Test Scene</summary>
    public static class SubdueSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_Subdue.unity";

        [MenuItem("Tools/PROJECT 1028/Build Subdue Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            PlayFoundationSceneBuilder.CreateGround();
            PlayFoundationSceneBuilder.CreateWalls();
            PlayFoundationSceneBuilder.CreateOcclusionWall(new Vector3(-6f, 1.5f, 10f));

            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("TimeOfDay").AddComponent<TimeOfDay>();
            new GameObject("SuspicionSystem").AddComponent<SuspicionSystem>();
            new GameObject("SuspicionDebugPanel").AddComponent<SuspicionDebugPanel>();
            new GameObject("SuspicionTestConsole").AddComponent<SuspicionTestConsole>();
            new GameObject("NpcTypeSystem").AddComponent<NpcTypeSystem>();
            new GameObject("NpcTypeTestConsole").AddComponent<NpcTypeTestConsole>();
            new GameObject("SubdueSystem").AddComponent<SubdueSystem>();
            new GameObject("SubdueDebugPanel").AddComponent<SubdueDebugPanel>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            Player(PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(0f, 1f, -6f), actions, withInput: true));
            Player(PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(12f, 1f, 12f), actions, withInput: false));

            Npc("NPC_W1", "sub_w1", "W1 감시자(등)", new Vector3(0f, 1f, 6f), Vector3.forward);   // 등을 돌림
            Npc("NPC_W2", "sub_w2", "W2 감시자(정면)", new Vector3(6f, 1f, 4f), Vector3.back);
            Npc("NPC_Q",  "sub_q",  "Q 침묵자(불가)", new Vector3(-6f, 1f, 4f), Vector3.back);
            Npc("NPC_V",  "sub_v",  "V 목격자", new Vector3(0f, 1f, 14f), Vector3.back);

            Crate("Crate_A", new Vector3(-2f, 0.25f, 0f), 0.5f, twoHanded: false);
            Crate("Crate_B", new Vector3(2f, 0.4f, 0f), 0.8f, twoHanded: true);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] 제압 테스트 씬 생성 완료: {ScenePath}");
        }

        private static void Player(GameObject go)
        {
            go.AddComponent<PlayerDisguise>();
            go.AddComponent<PlayerHands>();
            go.AddComponent<SubdueActor>();
        }

        private static void Npc(string name, string npcId, string display, Vector3 pos, Vector3 facing)
        {
            var go = PlayFoundationSceneBuilder.CreateNpc(name, npcId, display, pos, facing, talkable: false);
            go.AddComponent<NpcSuspicionProfile>();
            go.AddComponent<NpcMover>();
            go.AddComponent<NpcSuspicionBehaviour>();
        }

        private static void Crate(string name, Vector3 pos, float size, bool twoHanded)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size;
            PlayFoundationSceneBuilder.Tint(go, twoHanded ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.8f, 0.7f, 0.4f));
            var c = go.AddComponent<CarriableObject>();
            c.Configure(twoHanded ? "큰 상자" : "작은 상자", twoHanded);
        }
    }
}
