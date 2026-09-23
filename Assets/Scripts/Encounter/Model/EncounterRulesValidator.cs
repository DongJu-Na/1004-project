using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Project1028.Suspicion;

namespace Project1028.Encounter
{
    public sealed class EncounterValidationResult
    {
        public bool IsError;
        public int PendingCount;
        public int SampleCount;
        public int TotalNpcs;
        public int TotalLines;
        public readonly List<string> Messages = new List<string>();
        public void Error(string m) { IsError = true; Messages.Add("오류: " + m); }
        public void Warning(string m) { Messages.Add("경고: " + m); }
    }

    /// <summary>순수 로직: §6.1 단가·§6.2 유형 계약·§6.4 스폰 규격·§6.5 회수 참조 검증.</summary>
    public static class EncounterRulesValidator
    {
        private static readonly Regex IdPattern = new Regex("^[a-z][a-z0-9_]*$", RegexOptions.Compiled);
        public const int MinNpcs = 1, MaxNpcs = 2, MinLines = 3, MaxLines = 5, MinPerRun = 2;

        public static EncounterValidationResult Validate(EncounterRules r, Func<string, bool> eventExists)
        {
            var res = new EncounterValidationResult();
            eventExists = eventExists ?? (_ => true);
            if (r == null) { res.Error("규칙 데이터가 null입니다."); return res; }

            // spawn (§6.4, §6.3)
            var sp = r.spawn;
            if (sp == null) res.Error("spawn 블록이 없습니다.");
            else
            {
                if (sp.perRunMin < MinPerRun) { res.Warning($"spawn.perRunMin {sp.perRunMin} < {MinPerRun} → {MinPerRun}으로 보정 (§6.4 런당 2~4)"); sp.perRunMin = MinPerRun; }
                if (sp.perRunMax < sp.perRunMin) res.Error("spawn.perRunMax는 perRunMin 이상이어야 합니다.");
                if (sp.minIntervalSeconds < 0f) res.Error("spawn.minIntervalSeconds ≥ 0");
                if (sp.footChance < 0f || sp.footChance > 1f) res.Error("spawn.footChance 0~1");
                if (sp.vehicleChance < 0f || sp.vehicleChance > 1f) res.Error("spawn.vehicleChance 0~1");
                if (sp.vehicleChance < sp.footChance) res.Warning("spawn.vehicleChance < footChance — §6.4 '차량 이동 중 확률이 도보보다 높다'와 어긋남");
                if (sp.despawnDelaySeconds < 0f) res.Error("spawn.despawnDelaySeconds ≥ 0");
                if (sp.choiceTimeoutSeconds <= 0f) res.Error("spawn.choiceTimeoutSeconds > 0");
                if (sp.typeWeights == null) res.Error("spawn.typeWeights가 없습니다.");
                else
                {
                    var w = sp.typeWeights;
                    if (w.A < 0f || w.B < 0f || w.C < 0f || w.D < 0f) res.Error("typeWeights는 음수 불가");
                    if (w.A + w.B + w.C + w.D <= 0f) res.Error("typeWeights 합은 > 0");
                }
                Status(res, sp.status, "spawn");
            }

            var ids = new HashSet<string>();
            bool anyD = false;
            if (r.encounters == null || r.encounters.Length == 0) res.Error("encounters가 비어 있습니다.");
            else
            {
                foreach (var e in r.encounters)
                {
                    string label = e?.id ?? "(null)";
                    if (e == null) { res.Error("null 인카운터 항목"); continue; }
                    if (string.IsNullOrEmpty(e.id) || !IdPattern.IsMatch(e.id)) res.Error($"{label}: id는 ^[a-z][a-z0-9_]*$");
                    else if (!ids.Add(e.id)) res.Error($"{label}: id 중복");
                    if (!Enum.TryParse(e.type, out EncounterType type)) { res.Error($"{label}: type은 A|B|C|D"); continue; }
                    if (type == EncounterType.D) anyD = true;
                    if (e.sample) res.SampleCount++;

                    // §6.1 단가
                    int npcCount = e.npcs?.Length ?? 0;
                    if (npcCount < MinNpcs || npcCount > MaxNpcs) res.Error($"{label}: NPC 수 {npcCount} (1~2, §6.1)");
                    res.TotalNpcs += npcCount;
                    int lineCount = e.lines?.Length ?? 0;
                    if (lineCount == 0) res.Error($"{label}: 대사가 없습니다.");
                    else if (lineCount < MinLines || lineCount > MaxLines) res.Warning($"{label}: §6.1 3~5줄 규격 위반: {lineCount}줄");
                    res.TotalLines += lineCount;

                    // §6.2 계약
                    var o = e.outcome ?? new OutcomeDef();
                    bool hasFlags = o.flags != null && o.flags.Length > 0;
                    switch (type)
                    {
                        case EncounterType.A:
                            if (!hasFlags) res.Error($"{label}: A 유형은 이득 플래그 1개 이상");
                            if (!string.IsNullOrEmpty(o.eventId)) res.Error($"{label}: A 유형은 상승 사건을 내지 않습니다.");
                            break;
                        case EncounterType.B:
                            if (o.condition != "in_sight" && o.condition != "always") res.Error($"{label}: B 유형 condition은 in_sight|always");
                            if (string.IsNullOrEmpty(o.eventId) || !eventExists(o.eventId)) res.Error($"{label}: B 유형 eventId '{o.eventId}'가 의심 규칙에 없습니다.");
                            break;
                        case EncounterType.C:
                            if (string.IsNullOrEmpty(o.prompt)) res.Error($"{label}: C 유형 prompt 필수");
                            if (o.optionA == null || string.IsNullOrEmpty(o.optionA.label)) res.Error($"{label}: C 유형 optionA 필수");
                            if (o.optionB == null || string.IsNullOrEmpty(o.optionB.label)) res.Error($"{label}: C 유형 optionB 필수");
                            if (o.defaultOption != "A" && o.defaultOption != "B") res.Error($"{label}: C 유형 defaultOption은 A|B");
                            if (e.choiceAfterLine < 0 || e.choiceAfterLine >= Math.Max(1, lineCount)) res.Error($"{label}: C 유형 choiceAfterLine은 0..{lineCount - 1}");
                            foreach (var opt in new[] { o.optionA, o.optionB })
                                if (opt != null && !string.IsNullOrEmpty(opt.eventId) && !eventExists(opt.eventId)) res.Error($"{label}: 선택지 eventId '{opt.eventId}'가 의심 규칙에 없습니다.");
                            break;
                        case EncounterType.D:
                            if (hasFlags || !string.IsNullOrEmpty(o.eventId)) res.Error($"{label}: D 유형은 결과(플래그·사건)가 없어야 합니다 (§6.2).");
                            break;
                    }
                    if (e.conditions != null)
                    {
                        if (e.conditions.timeOfDay != "any" && e.conditions.timeOfDay != "day" && e.conditions.timeOfDay != "night") res.Error($"{label}: conditions.timeOfDay any|day|night");
                        if (!Enum.TryParse<AlertZone>(e.conditions.minZone ?? "Calm", out _)) res.Error($"{label}: conditions.minZone 구간 이름 오류");
                    }
                }
                foreach (var e in r.encounters)
                    if (e != null && !string.IsNullOrEmpty(e.variantOf) && !ids.Contains(e.variantOf)) res.Error($"{e.id}: variantOf '{e.variantOf}' 없음");
                if (!anyD) res.Warning("D 유형(결과 없음)이 하나도 없습니다 — §6.2 'D가 반드시 있어야 한다'");
            }

            if (r.pools == null || r.pools.Length == 0) res.Error("pools가 비어 있습니다.");
            else foreach (var p in r.pools)
            {
                if (p == null || string.IsNullOrEmpty(p.poolId) || string.IsNullOrEmpty(p.islandId)) { res.Error("pool: poolId/islandId 필수"); continue; }
                if (p.encounterIds != null) foreach (var id in p.encounterIds) if (!ids.Contains(id)) res.Error($"pool {p.poolId}: 인카운터 '{id}' 없음");
            }

            if (r.callbacks != null) foreach (var c in r.callbacks)
            {
                if (c == null || string.IsNullOrEmpty(c.flag)) { res.Error("callback: flag 필수"); continue; }
                if (c.targetKind != "encounter_variant" && c.targetKind != "document" && c.targetKind != "npc_line") res.Error($"callback {c.flag}: targetKind 오류");
                if (c.targetKind == "encounter_variant" && !ids.Contains(c.targetId)) res.Error($"callback {c.flag}: 변주 '{c.targetId}' 없음");
            }
            return res;
        }

        private static void Status(EncounterValidationResult res, string status, string label)
        {
            if (status == SuspicionRules.StatusPending) res.PendingCount++;
            else if (status != SuspicionRules.StatusConfirmed) res.Error($"{label}: status는 '확정' 또는 '확정대기'");
        }
    }
}
