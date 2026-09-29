using UnityEngine;

namespace Project1028.Suspicion
{
    /// <summary>
    /// §7 낮/밤을 실제로 보이게 한다: 방향광 세기·색, 환경광, 안개. TimeOfDay는 플래그만 바꾸므로 이 컴포넌트가 표현을 맡는다.
    /// 값은 표현 파라미터(규칙 아님). 씬 빌더가 002 이후 씬마다 추가한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NightLighting : MonoBehaviour
    {
        [SerializeField] private float dayIntensity = 1.2f;
        [SerializeField] private float nightIntensity = 0.12f;
        [SerializeField] private Color dayColor = new Color(1f, 0.96f, 0.9f);
        [SerializeField] private Color nightColor = new Color(0.45f, 0.55f, 0.85f);
        [SerializeField] private Color dayAmbient = new Color(0.55f, 0.58f, 0.62f);
        [SerializeField] private Color nightAmbient = new Color(0.06f, 0.07f, 0.12f);
        [SerializeField] private float nightFogDensity = 0.018f;
        [SerializeField] private float transitionSeconds = 3f;

        private Light sun;
        private float target; // 0 = 낮, 1 = 밤
        private float current;
        private Vector3 daySunEuler;

        private void Start()
        {
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional) { sun = l; break; }
            if (sun != null) daySunEuler = sun.transform.eulerAngles;
            RenderSettings.fog = false;
            target = current = TimeOfDay.CurrentOrDay == DayPhase.Night ? 1f : 0f;
            Apply();
            if (TimeOfDay.Instance != null) TimeOfDay.Instance.OnChanged += OnChanged;
        }

        private void OnDestroy() { if (TimeOfDay.Instance != null) TimeOfDay.Instance.OnChanged -= OnChanged; }
        private void OnChanged(DayPhase p) => target = p == DayPhase.Night ? 1f : 0f;

        private void Update()
        {
            if (Mathf.Approximately(current, target)) return;
            current = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime / Mathf.Max(0.1f, transitionSeconds));
            Apply();
        }

        private void Apply()
        {
            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(dayIntensity, nightIntensity, current);
                sun.color = Color.Lerp(dayColor, nightColor, current);
                // 밤에는 해를 낮게 눕혀 그림자가 길어진다
                sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(daySunEuler.x, 12f, current), daySunEuler.y, daySunEuler.z);
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, current);
            RenderSettings.fog = current > 0.05f;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Color.Lerp(new Color(0.7f, 0.75f, 0.8f), new Color(0.03f, 0.04f, 0.07f), current);
            RenderSettings.fogDensity = Mathf.Lerp(0f, nightFogDensity, current);
        }
    }
}
