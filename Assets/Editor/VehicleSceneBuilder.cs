using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;
using Project1028.Vehicle;

namespace Project1028.PlayFoundation.Editor
{
    /// <summary>006 테스트 씬(001만 의존). 메뉴: Tools/PROJECT 1028/Build Vehicle Test Scene</summary>
    public static class VehicleSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test_Vehicle.unity";

        [MenuItem("Tools/PROJECT 1028/Build Vehicle Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PlayFoundationSceneBuilder.CreateLight();
            PlayFoundationSceneBuilder.CreateGround();
            PlayFoundationSceneBuilder.CreateWalls();
            new GameObject("RuntimeHud").AddComponent<RuntimeHud>();
            new GameObject("VehicleDebugPanel").AddComponent<VehicleDebugPanel>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayFoundationSceneBuilder.InputActionsPath);
            PlayFoundationSceneBuilder.CreatePlayer("P1", new Vector3(0f, 1f, -6f), actions, withInput: true);
            PlayFoundationSceneBuilder.CreatePlayer("P2", new Vector3(10f, 1f, 10f), actions, withInput: false);

            // 경사·둔덕 (프리미티브)
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ramp";
            ramp.transform.position = new Vector3(-8f, 1f, 8f);
            ramp.transform.rotation = Quaternion.Euler(-15f, 0f, 0f);
            ramp.transform.localScale = new Vector3(6f, 0.5f, 8f);
            PlayFoundationSceneBuilder.Tint(ramp, new Color(0.5f, 0.5f, 0.55f));
            var bump = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bump.name = "Bump";
            bump.transform.position = new Vector3(0f, 0.2f, 12f);
            bump.transform.localScale = new Vector3(4f, 0.4f, 1f);
            PlayFoundationSceneBuilder.Tint(bump, new Color(0.6f, 0.5f, 0.4f));

            // NPC 1 (001만: 정적 콜라이더, Rigidbody 없음 → 차량이 밀지 못한다)
            PlayFoundationSceneBuilder.CreateNpc("NPC_Bystander", "veh_npc", "구경꾼", new Vector3(12f, 1f, -6f), Vector3.back, talkable: false);

            CreateTruck(new Vector3(5f, 1f, 0f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[SceneBuilder] 차량 테스트 씬 생성 완료: {ScenePath}");
        }

        public static GameObject CreateTruck(Vector3 position)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Truck";
            body.transform.position = position;
            body.transform.localScale = new Vector3(2.2f, 1.0f, 4.2f);
            PlayFoundationSceneBuilder.Tint(body, new Color(0.3f, 0.45f, 0.7f));

            var rb = body.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.linearDamping = 0.2f;
            rb.angularDamping = 1.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var wheels = new Transform[4];
            var offsets = new[] { new Vector3(-0.55f, -0.4f, 0.34f), new Vector3(0.55f, -0.4f, 0.34f), new Vector3(-0.55f, -0.4f, -0.34f), new Vector3(0.55f, -0.4f, -0.34f) };
            for (int i = 0; i < 4; i++)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                w.name = $"Wheel_{i}";
                w.transform.SetParent(body.transform, false);
                w.transform.localPosition = offsets[i]; // 부모 스케일 기준 로컬
                w.transform.localScale = new Vector3(0.35f, 0.15f, 0.18f);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Object.DestroyImmediate(w.GetComponent<Collider>());
                PlayFoundationSceneBuilder.Tint(w, new Color(0.1f, 0.1f, 0.1f));
                wheels[i] = w.transform;
            }

            var controller = body.AddComponent<VehicleController>();
            var so = new SerializedObject(controller);
            var arr = so.FindProperty("wheelVisuals");
            arr.arraySize = 4;
            for (int i = 0; i < 4; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = wheels[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            body.AddComponent<VehicleSeats>();
            body.AddComponent<VehicleEnterInteractable>();
            body.AddComponent<VehicleDriverInput>();
            return body;
        }
    }
}
