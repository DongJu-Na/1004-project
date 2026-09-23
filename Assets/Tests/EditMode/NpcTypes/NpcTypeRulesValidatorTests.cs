using System.Linq;
using NUnit.Framework;
using Project1028.NpcTypes;

namespace Project1028.NpcTypes.Tests
{
    public class NpcTypeRulesValidatorTests
    {
        private static NpcTypeRules Valid() => new NpcTypeRules
        {
            version = "t",
            rules = new TypeRuleSet
            {
                watcher = new WatcherRule { investigateEventId = "watcher_investigate", status = "확정" },
                informer = new InformerRule { deliveryDelaySeconds = 8f, deliveryEventId = "informer_delivery", arriveDistance = 1.5f, status = "확정대기" },
                sympathizer = new SympathizerRule { turnZone = "Tension", status = "확정" },
                sentinel = new SentinelRule { detectRadius = 5f, soundRadius = 10f, cooldownSeconds = 5f, noiseEventId = "noise", status = "확정대기" },
            },
            assignments = new[]
            {
                new NpcAssignment { npcId = "a", type = "Watcher", roles = new string[0] },
                new NpcAssignment { npcId = "b", type = "Informer" },
                new NpcAssignment { npcId = "m", type = "Watcher", roles = new[] { "manager" } },
            }
        };

        [Test] public void Valid_NoError_FillsDefaultFaction()
        {
            var v = Valid();
            var r = NpcTypeRulesValidator.Validate(v, _ => true);
            Assert.IsFalse(r.IsError, string.Join("\n", r.Messages));
            Assert.AreEqual("Victim", v.assignments[1].faction);
            Assert.AreEqual(2, r.PendingCount);
        }
        [Test] public void MissingRules_IsError() { var v = Valid(); v.rules = null; Assert.IsTrue(NpcTypeRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void ZeroDelay_IsError() { var v = Valid(); v.rules.informer.deliveryDelaySeconds = 0f; Assert.IsTrue(NpcTypeRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void TurnZoneCalm_IsError() { var v = Valid(); v.rules.sympathizer.turnZone = "Calm"; Assert.IsTrue(NpcTypeRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void DuplicateNpcId_IsError() { var v = Valid(); v.assignments[1].npcId = "a"; Assert.IsTrue(NpcTypeRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void BadType_IsError() { var v = Valid(); v.assignments[0].type = "Watchr"; Assert.IsTrue(NpcTypeRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void WatcherVictim_IsWarning_FactionKept()
        {
            var v = Valid(); v.assignments[0].faction = "Victim";
            var r = NpcTypeRulesValidator.Validate(v, _ => true);
            Assert.IsFalse(r.IsError); Assert.IsTrue(r.Messages.Any(m => m.Contains("진영을 우선"))); Assert.AreEqual("Victim", v.assignments[0].faction);
        }
        [Test] public void BrokerNotAntagonist_IsWarning()
        {
            var v = Valid(); v.assignments[1].roles = new[] { "broker" };
            var r = NpcTypeRulesValidator.Validate(v, _ => true);
            Assert.IsFalse(r.IsError); Assert.IsTrue(r.Messages.Any(m => m.Contains("broker")));
        }
        [Test] public void MissingStatus_IsError() { var v = Valid(); v.rules.watcher.status = null; Assert.IsTrue(NpcTypeRulesValidator.Validate(v, _ => true).IsError); }
        [Test] public void UnknownSuspicionEvent_IsError() { var v = Valid(); Assert.IsTrue(NpcTypeRulesValidator.Validate(v, id => id != "noise").IsError); }
    }
}
