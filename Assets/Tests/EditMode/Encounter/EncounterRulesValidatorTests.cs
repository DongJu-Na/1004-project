using System.Linq;
using NUnit.Framework;
using Project1028.Encounter;

namespace Project1028.Encounter.Tests
{
    public class EncounterRulesValidatorTests
    {
        private static EncounterDef Def(string id, string type, int npcs = 1, int lines = 3, OutcomeDef outcome = null, int choiceAfter = -1, string variantOf = null) => new EncounterDef
        {
            id = id, type = type, sample = true, variantOf = variantOf, choiceAfterLine = choiceAfter,
            trigger = new TriggerDef(), npcs = Enumerable.Range(0, npcs).Select(i => new NpcDef { displayName = "n", offset = new Vec3() }).ToArray(),
            lines = Enumerable.Range(0, lines).Select(i => $"l{i}").ToArray(), outcome = outcome ?? new OutcomeDef(), conditions = new ConditionDef(),
        };
        private static OutcomeDef A() => new OutcomeDef { flags = new[] { "f" } };
        private static OutcomeDef B() => new OutcomeDef { condition = "in_sight", eventId = "encounter_stared_at" };
        private static OutcomeDef C() => new OutcomeDef { prompt = "?", optionA = new ChoiceOption { label = "a", flags = new[] { "x" } }, optionB = new ChoiceOption { label = "b" }, defaultOption = "B" };

        private static EncounterRules Valid() => new EncounterRules
        {
            version = "t",
            spawn = new SpawnRule { perRunMin = 2, perRunMax = 4, minIntervalSeconds = 20f, footChance = 0.35f, vehicleChance = 0.7f, despawnDelaySeconds = 6f, choiceTimeoutSeconds = 12f, typeWeights = new TypeWeights { A = 30, B = 25, C = 20, D = 25 }, status = "확정대기" },
            pools = new[] { new PoolDef { poolId = "p", islandId = "common", encounterIds = new[] { "a1", "b1", "c1", "d1" } } },
            encounters = new[] { Def("a1", "A", outcome: A(), lines: 4), Def("b1", "B", npcs: 2, outcome: B()), Def("c1", "C", outcome: C(), choiceAfter: 1, lines: 4), Def("d1", "D") },
            callbacks = new[] { new CallbackEntry { flag = "x", targetKind = "encounter_variant", targetId = "a1", description = "d", sample = true } },
        };

        [Test] public void Valid_Ok_Counts()
        {
            var r = EncounterRulesValidator.Validate(Valid(), _ => true);
            Assert.IsFalse(r.IsError, string.Join("\n", r.Messages));
            Assert.AreEqual(4, r.SampleCount); Assert.AreEqual(5, r.TotalNpcs); Assert.AreEqual(14, r.TotalLines); Assert.AreEqual(1, r.PendingCount);
        }
        [Test] public void DuplicateId_Error() { var v = Valid(); v.encounters[1].id = "a1"; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void A_WithEvent_Error() { var v = Valid(); v.encounters[0].outcome.eventId = "noise"; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void D_WithFlags_Error() { var v = Valid(); v.encounters[3].outcome.flags = new[] { "f" }; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void C_NoChoiceLine_Error() { var v = Valid(); v.encounters[2].choiceAfterLine = -1; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void C_NoOptionA_Error() { var v = Valid(); v.encounters[2].outcome.optionA = null; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void TwoLines_Warning() { var v = Valid(); v.encounters[3].lines = new[] { "a", "b" }; var r = EncounterRulesValidator.Validate(v, _ => true); Assert.IsFalse(r.IsError); Assert.IsTrue(r.Messages.Any(m => m.Contains("3~5줄"))); }
        [Test] public void ZeroLines_Error() { var v = Valid(); v.encounters[3].lines = new string[0]; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void ThreeNpcs_Error() { var v = Valid(); v.encounters[3].npcs = new[] { new NpcDef(), new NpcDef(), new NpcDef() }; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void PerRunMin1_WarningFixed() { var v = Valid(); v.spawn.perRunMin = 1; var r = EncounterRulesValidator.Validate(v, _ => true); Assert.IsFalse(r.IsError); Assert.AreEqual(2, v.spawn.perRunMin); }
        [Test] public void NoD_Warning() { var v = Valid(); v.encounters = v.encounters.Take(3).ToArray(); v.pools[0].encounterIds = new[] { "a1", "b1", "c1" }; var r = EncounterRulesValidator.Validate(v, _ => true); Assert.IsTrue(r.Messages.Any(m => m.Contains("D 유형"))); }
        [Test] public void PoolUnknownId_Error() { var v = Valid(); v.pools[0].encounterIds = new[] { "zzz" }; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void VariantOfUnknown_Error() { var v = Valid(); v.encounters[0].variantOf = "nope"; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void CallbackVariantUnknown_Error() { var v = Valid(); v.callbacks[0].targetId = "nope"; Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void WeightsZero_Error() { var v = Valid(); v.spawn.typeWeights = new TypeWeights(); Assert.IsTrue(EncounterRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void B_UnknownEvent_Error() => Assert.IsTrue(EncounterRulesValidator.Validate(Valid(), id => id != "encounter_stared_at").IsError);
    }
}
