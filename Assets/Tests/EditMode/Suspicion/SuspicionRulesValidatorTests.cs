using System.Linq;
using NUnit.Framework;
using Project1028.Suspicion;

namespace Project1028.Suspicion.Tests
{
    public class SuspicionRulesValidatorTests
    {
        private static SuspicionEventRule Ev(string id, int p = 1, int i = 0, string scope = "witness", float radius = 0f, string status = "확정대기") =>
            new SuspicionEventRule { id = id, personalDelta = p, islandDelta = i, scope = scope, radius = radius, requiresSight = true, nightOnly = false, status = status };

        private static SuspicionRules Valid()
        {
            return new SuspicionRules
            {
                version = "t",
                events = new[]
                {
                    Ev("a"), Ev("b"), Ev("c"), Ev("d"), Ev("night_wander", scope: "target"), Ev("f", scope: "radius", radius: 5f), Ev("g"),
                    Ev("propagation", scope: "target", status: "확정"),
                },
                decay = new DecayRule { personalIntervalSeconds = 60f, islandPerMinute = 0.5f, status = "확정대기" },
                propagation = new PropagationRule { distance = 3f, delaySeconds = 4f, status = "확정" },
                timeOfDay = new TimeOfDayRule { dayMultiplier = 1f, nightMultiplier = 0.7f, nightWanderEventId = "night_wander", nightWanderIntervalSeconds = 3f, status = "확정대기" },
                carryOver = new CarryOverRule { personalFactor = 0.5f, islandFactor = 0.5f, islandMin = 5, status = "확정대기" },
                barks = new[] { "x" },
            };
        }

        [Test] public void Valid_NoError_PendingCount()
        {
            var r = SuspicionRulesValidator.Validate(Valid());
            Assert.IsFalse(r.IsError, string.Join("\n", r.Messages));
            Assert.AreEqual(7 + 3, r.PendingCount); // events 7 확정대기 + decay/timeOfDay/carryOver
        }

        [Test] public void EmptyEvents_IsError() { var v = Valid(); v.events = new SuspicionEventRule[0]; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void DuplicateId_IsError() { var v = Valid(); v.events[1].id = "a"; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void UppercaseId_IsError() { var v = Valid(); v.events[0].id = "Bad Id"; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }

        [Test] public void NegativeDelta_IsWarningAndClamped()
        {
            var v = Valid(); v.events[0].personalDelta = -2;
            var r = SuspicionRulesValidator.Validate(v);
            Assert.IsFalse(r.IsError); Assert.IsTrue(r.Messages.Any(m => m.Contains("클램프"))); Assert.AreEqual(0, v.events[0].personalDelta);
        }

        [Test] public void RadiusScope_ZeroRadius_IsError() { var v = Valid(); v.events[5].radius = 0f; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void BadScope_IsError() { var v = Valid(); v.events[0].scope = "witnes"; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void MissingStatus_IsError() { var v = Valid(); v.events[0].status = null; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void UnknownNightWanderId_IsError() { var v = Valid(); v.timeOfDay.nightWanderEventId = "nope"; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void FactorOver1_IsError() { var v = Valid(); v.carryOver.personalFactor = 1.5f; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void EmptyBarks_IsError() { var v = Valid(); v.barks = new string[0]; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void ZeroInterval_IsError() { var v = Valid(); v.decay.personalIntervalSeconds = 0f; Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
        [Test] public void MissingPropagationEvent_IsError() { var v = Valid(); v.events = v.events.Where(e => e.id != "propagation").ToArray(); Assert.IsTrue(SuspicionRulesValidator.Validate(v).IsError); }
    }
}
