using NUnit.Framework;
using Project1028.NpcTypes;
using Project1028.Suspicion;

namespace Project1028.NpcTypes.Tests
{
    public class SympathizerTurnRuleTests
    {
        [TestCase(AlertZone.Calm, false)] [TestCase(AlertZone.Watch, false)] [TestCase(AlertZone.Tension, true)] [TestCase(AlertZone.Lockdown, true)]
        public void TurnAtTension(AlertZone zone, bool expected) => Assert.AreEqual(expected, SympathizerTurnRule.IsTurned(zone, AlertZone.Tension));
        [Test] public void TurnAtWatch_WatchIsTurned() => Assert.IsTrue(SympathizerTurnRule.IsTurned(AlertZone.Watch, AlertZone.Watch));
        [Test] public void TurnAtLockdown_TensionNot() => Assert.IsFalse(SympathizerTurnRule.IsTurned(AlertZone.Tension, AlertZone.Lockdown));
    }
}
