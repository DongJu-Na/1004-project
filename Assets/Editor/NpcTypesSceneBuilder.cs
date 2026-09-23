using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>003 테스트 씬. 메뉴: Tools/PROJECT 1028/Build NPC Types Test Scene</summary>
    public static class NpcTypesSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_NpcTypes.unity";

        [MenuItem("Tools/PROJECT 1028/Build NPC Types Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            PlayFoundationSceneBuilder.CreateGround();
            PlayFoundationSceneBuilder.CreateWalls();
            PlayFoundationSceneBuilder.CreateOcclusionWall(new Vector3(-12f, 1.5f, 2f));

            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("TimeOfDay").AddComponent<TimeOfDay>();
            new GameObject("SuspicionSystem").AddComponent<SuspicionSystem>();
            new GameObject("SuspicionDebugPanel").AddComponent<SuspicionDebugPanel>();
            new GameObject("SuspicionTestConsole").AddComponent<SuspicionTestConsole>();
            new GameObject("NpcTypeSystem").AddComponent<NpcTypeSystem>();
            new GameObject("NpcTypeTestConsole").AddComponent<NpcTypeTestConsole>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            var p1 = PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(0f, 1f, -6f), actions, withInput: true);
            var p2 = PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(14f, 1f, 14f), actions, withInput: false);
            p1.AddComponent<PlayerDisguise>();
            p2.AddComponent<PlayerDisguise>();

            Npc("NPC_W", "type_watcher",     "W 감시자",   new Vector3(0f, 1f, 6f));
            Npc("NPC_I", "type_informer",    "I 밀고자",   new Vector3(-5f, 1f, 4f));
            Npc("NPC_S", "type_sympathizer", "S 동조자",   new Vector3(5f, 1f, 4f));
            Npc("NPC_Q", "type_silent",      "Q 침묵자",   new Vector3(-9f, 1f, 8f));
            Npc("NPC_D", "type_sentinel",    "D 경계자",   new Vector3(9f, 1f, -2f));
            Npc("NPC_M", "type_manager",     "M 관리자",   new Vector3(0f, 1f, 14f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] NPC 유형 테스트 씬 생성 완료: {ScenePath}");
        }

        private static void Npc(string name, string npcId, string display, Vector3 pos)
        {
            var go = PlayFoundationSceneBuilder.CreateNpc(name, npcId, display, pos, Vector3.back, talkable: false);
            go.AddComponent<NpcSuspicionProfile>();
            go.AddComponent<NpcMover>();
            go.AddComponent<NpcSuspicionBehaviour>();
            // NpcTypeProfile·행동 컴포넌트·라벨은 NpcTypeSystem이 Start에서 데이터 배정으로 부착한다.
        }
    }
}
