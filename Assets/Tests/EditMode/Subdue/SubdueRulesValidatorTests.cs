using NUnit.Framework;
using Project1028.Subdue;

namespace Project1028.Subdue.Tests
{
    public class SubdueRulesValidatorTests
    {
        private static ActionRule A(float range = 1.8f, float cone = 120f, float fail = 0f, string noise = "Low", string id = "noise_low", string status = "확정") =>
            new ActionRule { range = range, backConeDegrees = cone, failChance = fail, noiseLevel = noise, noiseEventId = id, status = status };

        private static SubdueRules Valid() => new SubdueRules
        {
            version = "t",
            actions = new ActionRuleSet { backstab = A(), shove = A(fail: 0.35f, noise: "Medium", id: "noise_medium", status: "확정대기"), objectStrike = A(range: 2f, noise: "High", id: "noise_high") },
            unconscious = new UnconsciousRule { minSeconds = 90f, maxSeconds = 180f, status = "확정" },
            carry = new CarryRule { speedMultiplier = 0.45f, status = "확정대기" },
            costs = new CostRule { witnessedEventId = "subdue_witnessed", foundIslandEventId = "unconscious_found_island", foundPersonalEventId = "unconscious_found", wakeEventId = "unconscious_wake", shoveFailEventId = "shove_failed", status = "확정" },
        };

        [Test] public void Valid_Ok_Pending2()
        {
            var r = SubdueRulesValidator.Validate(Valid(), _ => true);
            Assert.IsFalse(r.IsError, string.Join("\n", r.Messages)); Assert.AreEqual(2, r.PendingCount);
        }
        [Test] public void MissingActions_Error() { var v = Valid(); v.actions = null; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void ZeroRange_Error() { var v = Valid(); v.actions.backstab.range = 0f; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void Cone200_Error() { var v = Valid(); v.actions.backstab.backConeDegrees = 200f; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void FailChance15_Error() { var v = Valid(); v.actions.shove.failChance = 1.5f; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void MaxBelowMin_Error() { var v = Valid(); v.unconscious.maxSeconds = 10f; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void Speed1_Error() { var v = Valid(); v.carry.speedMultiplier = 1f; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void UnknownCostEvent_Error() => Assert.IsTrue(SubdueRulesValidator.Validate(Valid(), id => id != "unconscious_wake").IsError);
        [Test] public void MissingStatus_Error() { var v = Valid(); v.costs.status = null; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void BadNoise_Error() { var v = Valid(); v.actions.objectStrike.noiseLevel = "Loud"; Assert.IsTrue(SubdueRulesValidator.Validate(v, _ => true).IsError); }
    }
}
