using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>002 테스트 씬. 001 빌더의 정적 메서드를 재사용한다. 메뉴: Tools/PROJECT 1028/Build Suspicion Test Scene</summary>
    public static class SuspicionSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_Suspicion.unity";

        [MenuItem("Tools/PROJECT 1028/Build Suspicion Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            PlayFoundationSceneBuilder.CreateGround();
            PlayFoundationSceneBuilder.CreateWalls();
            PlayFoundationSceneBuilder.CreateOcclusionWall(new Vector3(-4f, 1.5f, 0.5f));

            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("TimeOfDay").AddComponent<TimeOfDay>();
            new GameObject("SuspicionSystem").AddComponent<SuspicionSystem>();
            new GameObject("SuspicionDebugPanel").AddComponent<SuspicionDebugPanel>();
            new GameObject("SuspicionTestConsole").AddComponent<SuspicionTestConsole>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            var p1 = PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(0f, 1f, -6f), actions, withInput: true);
            var p2 = PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(10f, 1f, 10f), actions, withInput: false);
            p1.AddComponent<PlayerDisguise>();
            p2.AddComponent<PlayerDisguise>();

            var a = PlayFoundationSceneBuilder.CreateNpc("NPC_A", "npc_dock_worker", "NPC_A 일반", new Vector3(2f, 1f, 4f), Vector3.back);
            var b = PlayFoundationSceneBuilder.CreateNpc("NPC_B", "npc_dock_worker", "NPC_B 전파안함", new Vector3(-4f, 1f, 4f), Vector3.back);
            var m = PlayFoundationSceneBuilder.CreateNpc("NPC_M", "npc_dock_worker", "NPC_M 관리자", new Vector3(6f, 1f, 12f), Vector3.back);

            Decorate(a, propagates: true, manager: false);
            Decorate(b, propagates: false, manager: false);
            Decorate(m, propagates: true, manager: true);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] 의심 테스트 씬 생성 완료: {ScenePath}");
        }

        private static void Decorate(GameObject npc, bool propagates, bool manager)
        {
            var profile = npc.AddComponent<NpcSuspicionProfile>();
            profile.PropagatesSuspicion = propagates;
            profile.IsManager = manager;
            npc.AddComponent<NpcMover>();
            npc.AddComponent<NpcSuspicionBehaviour>();
        }
    }
}
