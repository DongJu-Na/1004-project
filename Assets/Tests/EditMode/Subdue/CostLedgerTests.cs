using NUnit.Framework;
using Project1028.Subdue;

namespace Project1028.Subdue.Tests
{
    public class CostLedgerTests
    {
        [Test] public void Witnesses_FirstTimeTrue_ThenFalse()
        {
            var l = new CostLedger();
            Assert.IsTrue(l.RecordWitnesses("body", new[] { "A", "B" }));
            Assert.IsFalse(l.RecordWitnesses("body", new[] { "A" }));
            Assert.AreEqual(2, l.WitnessCount("body"));
        }
        [Test] public void Witness_CannotDiscover()
        {
            var l = new CostLedger(); l.RecordWitnesses("body", new[] { "A" });
            Assert.IsFalse(l.TryRecordDiscovery("body", "A"));
        }
        [Test] public void Discovery_OncePerObserver()
        {
            var l = new CostLedger(); l.RecordWitnesses("body", new[] { "A" });
            Assert.IsTrue(l.TryRecordDiscovery("body", "C"));
            Assert.IsFalse(l.TryRecordDiscovery("body", "C"));
            Assert.IsTrue(l.TryRecordDiscovery("body2", "C"));
            Assert.AreEqual(1, l.DiscovererCount("body"));
        }
        [Test] public void DiscoveryWithoutWitnesses_True() => Assert.IsTrue(new CostLedger().TryRecordDiscovery("b", "X"));
        [Test] public void Clear_Resets()
        {
            var l = new CostLedger(); l.RecordWitnesses("b", new[] { "A" }); l.Clear("b");
            Assert.IsTrue(l.RecordWitnesses("b", new[] { "A" })); Assert.IsTrue(l.TryRecordDiscovery("b", "Z"));
        }
    }
}
