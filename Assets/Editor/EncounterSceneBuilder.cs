using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Suspicion;
using Project1028.NpcTypes;
using Project1028.Subdue;
using Project1028.Encounter;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>007 테스트 씬. 메뉴: Tools/PROJECT 1028/Build Encounter Test Scene</summary>
    public static class EncounterSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_Encounter.unity";

        [MenuItem("Tools/PROJECT 1028/Build Encounter Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            PlayFoundationSceneBuilder.CreateGround();
            PlayFoundationSceneBuilder.CreateWalls();

            // 도로 띠 (시각용)
            var road = GameObject.CreatePrimitive(PrimitiveType.Plane);
            road.name = "Road";
            road.transform.position = new Vector3(0f, 0.02f, 0f);
            road.transform.localScale = new Vector3(0.8f, 1f, 3.8f);
            Object.DestroyImmediate(road.GetComponent<Collider>());
            PlayFoundationSceneBuilder.Tint(road, new Color(0.2f, 0.2f, 0.22f));

            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("TimeOfDay").AddComponent<TimeOfDay>();
            new GameObject("SuspicionSystem").AddComponent<SuspicionSystem>();
            new GameObject("SuspicionDebugPanel").AddComponent<SuspicionDebugPanel>();
            new GameObject("SuspicionTestConsole").AddComponent<SuspicionTestConsole>();
            new GameObject("NpcTypeSystem").AddComponent<NpcTypeSystem>();
            new GameObject("SubdueSystem").AddComponent<SubdueSystem>();
            new GameObject("EncounterSystem").AddComponent<EncounterSystem>();
            new GameObject("EncounterDebugPanel").AddComponent<EncounterDebugPanel>();
            new GameObject("EncounterTestConsole").AddComponent<EncounterTestConsole>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            var p1 = PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(0f, 1f, -30f), actions, withInput: true);
            var p2 = PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(12f, 1f, 12f), actions, withInput: false);
            foreach (var p in new[] { p1, p2 })
            {
                p.AddComponent<PlayerDisguise>();
                p.AddComponent<PlayerHands>();
                p.AddComponent<SubdueActor>();
                p.AddComponent<PlayerActivity>();
            }

            VehicleSceneBuilder.CreateTruck(new Vector3(5f, 1f, -28f));

            foreach (float z in new[] { -25f, -15f, -5f, 5f, 15f, 25f })
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = $"Trigger_{z:+0;-0}";
                go.transform.position = new Vector3(0f, 0.03f, z);
                go.transform.localScale = new Vector3(6f, 0.02f, 6f);
                Object.DestroyImmediate(go.GetComponent<Collider>());
                PlayFoundationSceneBuilder.Tint(go, new Color(0.9f, 0.6f, 0.2f));
                var trig = go.AddComponent<EncounterTrigger>();
                trig.Configure(go.name, 3f);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] 인카운터 테스트 씬 생성 완료: {ScenePath}");
        }
    }
}
