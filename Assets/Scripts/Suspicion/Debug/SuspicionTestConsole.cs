using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Project1028.PlayFoundation;

namespace Project1028.Suspicion
{
    /// <summary>
    /// 테스트 씬 키 콘솔 (FR-028). F1~F7 사건, N 낮/밤, R 런 종료, T 시간 배속, [ ] 섬 ±10.
    /// witness/target 사건은 P1 카메라 전방 최근접 NPC를 대상으로 삼는다.
    /// </summary>
    public sealed class SuspicionTestConsole : MonoBehaviour
    {
        [SerializeField] private float pickRange = 12f;

        private static readonly Key[] FKeys = { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7 };

        private void Update()
        {
            var kb = Keyboard.current;
            var system = SuspicionSystem.Instance;
            if (kb == null || system == null) return;

            for (int i = 0; i < FKeys.Length; i++)
            {
                if (kb[FKeys[i]].wasPressedThisFrame) FireEvent(system, i);
            }

            if (kb.nKey.wasPressedThisFrame && TimeOfDay.Instance != null)
            {
                TimeOfDay.Instance.Toggle();
                RuntimeHud.Instance?.Warn($"시간대 → {TimeOfDay.Instance.Phase}");
            }
            if (kb.rKey.wasPressedThisFrame) system.EndRun();
            if (kb.tKey.wasPressedThisFrame)
            {
                Time.timeScale = Mathf.Approximately(Time.timeScale, 1f) ? 10f : 1f;
                RuntimeHud.Instance?.Warn($"시간 배속 x{Time.timeScale:0}");
            }
            if (kb.leftBracketKey.wasPressedThisFrame) { system.DebugAdjustIsland(-10); RuntimeHud.Instance?.Warn("섬 의심도 -10 (디버그)"); }
            if (kb.rightBracketKey.wasPressedThisFrame) { system.DebugAdjustIsland(+10); RuntimeHud.Instance?.Warn("섬 의심도 +10 (디버그)"); }
        }

        private void FireEvent(SuspicionSystem system, int index)
        {
            if (!system.IsReady || system.Rules.events == null || index >= system.Rules.events.Length) return;
            var rule = system.Rules.events[index];
            var actor = PlayerEntity.All.Count > 0 ? PlayerEntity.All[0] : null;
            if (actor == null) { RuntimeHud.Instance?.Warn("플레이어 개체가 없습니다."); return; }

            if (rule.nightOnly && TimeOfDay.CurrentOrDay != DayPhase.Night)
            {
                RuntimeHud.Instance?.Warn($"'{rule.id}'는 밤 전용 사건, 현재 낮 (N으로 전환)");
                return;
            }

            SuspicionEvent e;
            string targetName = "-";
            if (rule.scope == SuspicionEventRule.ScopeRadius)
            {
                e = SuspicionEvent.At(rule.id, actor, actor.Position);
            }
            else
            {
                var npc = PickNpcInFront(actor);
                if (npc == null) { RuntimeHud.Instance?.Warn($"'{rule.id}': 카메라 전방 {pickRange}m 안에 NPC가 없습니다."); return; }
                targetName = npc.DisplayName;
                e = rule.scope == SuspicionEventRule.ScopeTarget ? SuspicionEvent.Target(rule.id, actor, npc) : SuspicionEvent.Witness(rule.id, actor);
            }
            system.Raise(e);
            RuntimeHud.Instance?.Warn($"F{index + 1} '{rule.id}' 발생 (개인 +{rule.personalDelta}, 섬 +{rule.islandDelta}, 대상 {targetName}, {rule.status})");
        }

        private NpcIdentity PickNpcInFront(PlayerEntity actor)
        {
            var cands = new List<InteractableSelector.Candidate>();
            foreach (var npc in NpcIdentity.All) cands.Add(new InteractableSelector.Candidate(npc.Position, pickRange, npc));
            Vector3 fwd = actor.Camera != null ? actor.Camera.transform.forward : actor.Forward;
            var picked = InteractableSelector.Pick(cands, actor.Position, fwd);
            return picked?.Payload as NpcIdentity;
        }
    }
}
