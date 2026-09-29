using UnityEngine;
using UnityEngine.UI;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.Slice
{
    /// <summary>상단: 목표 문구·마커까지 거리·시계. 시작 30초 조작 안내. 마커는 001 DestinationMarker 재사용.</summary>
    public sealed class ObjectiveHud : MonoBehaviour
    {
        private Text title;
        private Text hint;
        private DestinationMarker marker;
        private string markedFacility;
        private float highlightUntil;
        private float nextHintAt;
        private int hintIndex;

        private void OnEnable() => SliceEvents.OnObjectiveAdvanced += OnAdvanced;
        private void OnDisable() => SliceEvents.OnObjectiveAdvanced -= OnAdvanced;
        private void OnAdvanced(ObjectiveStep s) => highlightUntil = Time.unscaledTime + 0.8f;

        private void Update()
        {
            var director = RunDirector.Instance;
            if (director == null || !director.IsReady) return;
            if (title == null)
            {
                if (RuntimeHud.Instance == null) return;
                title = RuntimeHud.Instance.CreateFixedText("ObjectiveTitle", new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(1100f, 44f), 30, TextAnchor.UpperCenter);
                hint = RuntimeHud.Instance.CreateFixedText("ObjectiveHint", new Vector2(0.5f, 1f), new Vector2(0f, -56f), new Vector2(1100f, 32f), 22, TextAnchor.UpperCenter);
                hint.color = new Color(0.8f, 0.9f, 1f);
            }

            var p1 = PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;
            var step = director.Objectives.Current;
            var clock = director.Clock;
            string clockText = clock.Phase == RunPhase.Lockdown
                ? $"봉쇄 — 다음 배 {RunDirector.Mmss(clock.RemainingToNextBoat)}"
                : clock.IsDepartureOpen ? "배가 떠난다 — 부두로" : $"출항까지 {RunDirector.Mmss(clock.RemainingToDeparture)}";

            if (director.IsEnded) { title.text = "런 종료"; hint.text = string.Empty; return; }

            if (step != null)
            {
                UpdateMarker(director, step.markerFacilityId, p1);
                float dist = marker != null && p1 != null ? Vector3.Distance(p1.Position, marker.transform.position) : 0f;
                var fac = director.Layout.Find(step.markerFacilityId);
                string facName = fac != null && !string.IsNullOrEmpty(fac.label) ? fac.label : step.markerFacilityId;
                title.text = $"{step.text}  ·  {facName}까지 {dist:0}m  ·  {clockText}";
            }
            else title.text = $"목표 완료 — {clockText}";
            title.color = Time.unscaledTime < highlightUntil ? new Color(1f, 0.9f, 0.3f) : Color.white;

            float t = clock.Elapsed;
            if (t <= director.Rules.hintSeconds && director.Rules.hints.Length > 0)
            {
                if (Time.unscaledTime >= nextHintAt) { hint.text = director.Rules.hints[hintIndex % director.Rules.hints.Length]; hintIndex++; nextHintAt = Time.unscaledTime + 5f; }
            }
            else hint.text = TimeOfDay.CurrentOrDay == DayPhase.Night ? "밤 — 돌아다니는 것만으로 의심받는다" : string.Empty;
        }

        private void UpdateMarker(RunDirector director, string facilityId, PlayerEntity p1)
        {
            if (markedFacility == facilityId && marker != null) return;
            var fac = director.Layout.Find(facilityId);
            if (fac == null || p1 == null) return;
            if (marker != null) Destroy(marker.gameObject);
            var dest = new Destination(fac.label ?? facilityId, new Vector3(fac.position.x, 0f, fac.position.z), "objective", p1);
            marker = DestinationMarker.Spawn(dest);
            markedFacility = facilityId;
        }
    }
}
