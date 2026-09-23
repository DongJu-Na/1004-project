using System.Collections.Generic;
using UnityEngine;
using Project1028.PlayFoundation;
using Project1028.Suspicion;

namespace Project1028.Encounter
{
    /// <summary>
    /// 인카운터 인스턴스 상태기계: Spawning → Dialogue → (Choice) → Outcome → Ended / Interrupted.
    /// 결과 계약(§6.2): A 플래그만, B 조건부 상승 사건, C 이지선다 강제, D 무결과.
    /// </summary>
    public sealed class EncounterInstance : MonoBehaviour
    {
        public EncounterDef Def { get; private set; }
        public PlayerEntity Player { get; private set; }
        public InstancePhase Phase { get; private set; } = InstancePhase.Spawning;
        public EncounterType Type => EncounterRules.TypeOf(Def);
        public string Chosen { get; private set; }
        public IReadOnlyList<NpcIdentity> SpawnedNpcs => npcs;

        private readonly List<NpcIdentity> npcs = new List<NpcIdentity>();
        private readonly List<string> grantedFlags = new List<string>();
        private DialogueRunner runner;
        private ChoiceResolver choice;
        private EncounterSystem system;
        private bool finished;

        public void Begin(EncounterDef def, PlayerEntity player, Vector3 anchor, EncounterSystem owner)
        {
            Def = def; Player = player; system = owner;
            Quaternion facing = Quaternion.LookRotation(new Vector3(player.Forward.x, 0f, player.Forward.z).normalized, Vector3.up);
            npcs.AddRange(EncounterNpcSpawner.Spawn(def, anchor, facing));
            if (npcs.Count == 0) { RuntimeHud.Instance?.Warn($"{def.id}: NPC를 스폰할 수 없어 취소"); Finish(InstancePhase.Interrupted, recordHistory: false); return; }

            runner = player.GetComponent<DialogueRunner>();
            DialogueEvents.OnDialogueStarted += HandleStarted;
            DialogueEvents.OnLineAdvanced += HandleLine;
            DialogueEvents.OnDialogueEnded += HandleEnded;

            Phase = InstancePhase.Dialogue;
            if (runner == null || !runner.TryBeginInline(npcs[0], def.lines))
            {
                RuntimeHud.Instance?.Warn($"{def.id}: 대화를 시작할 수 없어 취소");
                Finish(InstancePhase.Interrupted, recordHistory: false);
            }
        }

        private void HandleStarted(NpcIdentity npc, PlayerEntity p)
        {
            if (p != Player || !IsOurs(npc)) return;
            if (Type == EncounterType.C && Def.choiceAfterLine == 0) RequestChoice();
        }

        private void HandleLine(NpcIdentity npc, PlayerEntity p, int index)
        {
            if (p != Player || !IsOurs(npc)) return;
            if (Type == EncounterType.C && Phase == InstancePhase.Dialogue && index == Def.choiceAfterLine) RequestChoice();
        }

        private void RequestChoice()
        {
            Phase = InstancePhase.Choice;
            runner.Pause();
            choice = new ChoiceResolver(Def.outcome.defaultOption, system.Rules.spawn.choiceTimeoutSeconds);
            RuntimeHud.Instance?.ShowSecondaryPrompt(Player, $"{Def.outcome.prompt}   1: {Def.outcome.optionA.label}  /  2: {Def.outcome.optionB.label}");
            EncounterEvents.RaiseChoiceRequested(Def, Player);
        }

        public bool Choose(string option)
        {
            if (Phase != InstancePhase.Choice || choice == null || !choice.Choose(option)) return false;
            ResolveChoice();
            return true;
        }

        private void ResolveChoice()
        {
            Chosen = choice.Option;
            RuntimeHud.Instance?.ShowSecondaryPrompt(Player, null);
            EncounterEvents.RaiseChoiceMade(Def, Player, Chosen, choice.ByTimeout);
            RuntimeHud.Instance?.Warn($"{Def.id}: 선택 {(Chosen == "A" ? Def.outcome.optionA.label : Def.outcome.optionB.label)}{(choice.ByTimeout ? " (기본, 시간 초과)" : "")}");
            Phase = InstancePhase.Dialogue;
            runner.Resume();
        }

        private void Update()
        {
            if (Phase == InstancePhase.Choice && choice != null && choice.Tick(Time.deltaTime)) ResolveChoice();
        }

        private void HandleEnded(NpcIdentity npc, PlayerEntity p)
        {
            if (p != Player || !IsOurs(npc) || finished) return;
            Phase = InstancePhase.Outcome;
            ApplyOutcome();
            Finish(InstancePhase.Ended, recordHistory: true);
        }

        // §6.2 결과 계약
        private void ApplyOutcome()
        {
            var o = Def.outcome ?? new OutcomeDef();
            var suspicion = SuspicionSystem.Instance;
            switch (Type)
            {
                case EncounterType.A:
                    Grant(o.flags);
                    break;
                case EncounterType.B:
                    Grant(o.flags);
                    bool fire = o.condition == "always";
                    NpcIdentity witness = npcs.Count > 0 ? npcs[0] : null;
                    if (!fire) foreach (var n in npcs) { var v = n != null ? n.GetComponent<NpcVision>() : null; if (v != null && v.IsSeeing(Player)) { fire = true; witness = n; break; } }
                    if (fire && suspicion != null && !string.IsNullOrEmpty(o.eventId)) RaiseEvent(suspicion, o.eventId, witness);
                    break;
                case EncounterType.C:
                    if (choice == null || !choice.Resolved) { choice = choice ?? new ChoiceResolver(o.defaultOption, 1f); choice.ForceDefault(); Chosen = choice.Option; }
                    var opt = Chosen == "A" ? o.optionA : o.optionB;
                    if (opt != null)
                    {
                        Grant(opt.flags);
                        if (suspicion != null && !string.IsNullOrEmpty(opt.eventId)) RaiseEvent(suspicion, opt.eventId, npcs.Count > 0 ? npcs[0] : null);
                    }
                    break;
                default: // D: 아무 결과 없음
                    break;
            }
        }

        private void RaiseEvent(SuspicionSystem suspicion, string eventId, NpcIdentity target)
        {
            var rule = suspicion.Rules.FindEvent(eventId);
            if (rule != null && rule.scope == SuspicionEventRule.ScopeTarget && target != null)
                suspicion.Raise(SuspicionEvent.Target(eventId, Player, target));
            else
                suspicion.Raise(SuspicionEvent.At(eventId, Player, transform.position));
        }

        private void Grant(string[] flags)
        {
            if (flags == null) return;
            foreach (var f in flags)
            {
                if (string.IsNullOrEmpty(f)) continue;
                grantedFlags.Add(f);
                if (system.Ledger.AddFlags(new[] { f }) > 0) EncounterEvents.RaiseFlag(f);
            }
        }

        /// <summary>004 제압 등으로 중단: 플래그 미기록, 이력만.</summary>
        public void Interrupt()
        {
            if (finished) return;
            if (runner != null && runner.IsActive && runner.CurrentNpc != null && IsOurs(runner.CurrentNpc)) runner.Abort();
            RuntimeHud.Instance?.ShowSecondaryPrompt(Player, null);
            EncounterEvents.RaiseInterrupted(Def, Player);
            RuntimeHud.Instance?.Warn($"{Def.id}: 인카운터 중단 (플래그 없음, 이력만)");
            Finish(InstancePhase.Interrupted, recordHistory: true);
        }

        private bool IsOurs(NpcIdentity npc) => npc != null && npcs.Contains(npc);

        private void Finish(InstancePhase final, bool recordHistory)
        {
            if (finished) return;
            finished = true;
            Phase = final;
            DialogueEvents.OnDialogueStarted -= HandleStarted;
            DialogueEvents.OnLineAdvanced -= HandleLine;
            DialogueEvents.OnDialogueEnded -= HandleEnded;
            if (recordHistory) system.Ledger.Record(Def.id);
            if (final == InstancePhase.Ended)
            {
                EncounterEvents.RaiseEnded(Def, Player, grantedFlags);
                RuntimeHud.Instance?.Warn($"{Def.id} 종료 [{EncounterNames.Korean(Type)}] 플래그 {(grantedFlags.Count == 0 ? "없음" : string.Join(",", grantedFlags))}");
            }
            EncounterNpcSpawner.Despawn(npcs, system.Rules.spawn.despawnDelaySeconds);
            system.NotifyEnded(this);
            Destroy(gameObject, system.Rules.spawn.despawnDelaySeconds + 0.1f);
        }
    }
}
